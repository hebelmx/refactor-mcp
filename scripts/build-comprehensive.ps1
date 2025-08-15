#Requires -Version 5.1

<#
.SYNOPSIS
    Comprehensive build script for RefactorMCP that outputs all artifacts to external folder
    
.DESCRIPTION
    Builds all RefactorMCP projects, runs tests, generates reports, and organizes all build artifacts
    in a dedicated 'Refactor' folder outside the repository. Supports multiple configurations and
    comprehensive artifact collection.
    
.PARAMETER Configuration
    Build configuration (Debug, Release). Default: Release
    
.PARAMETER Platform  
    Target platform (Any CPU, x64, x86). Default: Any CPU
    
.PARAMETER OutputRoot
    Root directory for build outputs. Default: ../Refactor
    
.PARAMETER Clean
    Clean before building
    
.PARAMETER SkipTests
    Skip test execution
    
.PARAMETER SkipPackaging
    Skip NuGet package creation
    
.PARAMETER Verbose
    Enable verbose output
    
.EXAMPLE
    .\build-comprehensive.ps1 -Configuration Release
    
.EXAMPLE
    .\build-comprehensive.ps1 -Configuration Debug -Clean -Verbose
#>

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [ValidateSet("Any CPU", "x64", "x86")]
    [string]$Platform = "Any CPU",
    
    [string]$OutputRoot = "../Refactor",
    
    [switch]$Clean,
    [switch]$SkipTests,
    [switch]$SkipPackaging,
    [switch]$Verbose
)

# Script configuration
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Build information
$BuildId = [System.DateTime]::UtcNow.ToString("yyyyMMdd-HHmmss")
$BuildVersion = "1.0.0-dev-$BuildId"

# Color output functions
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Header { param($Message) Write-Host "`n🔨 $Message" -ForegroundColor Magenta -BackgroundColor Black }

# Get script directory and solution root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SolutionRoot = Split-Path -Parent $ScriptDir
$SolutionFile = Join-Path $SolutionRoot "RefactorMCP.sln"

# Validate solution exists
if (-not (Test-Path $SolutionFile)) {
    Write-Error "Solution file not found: $SolutionFile"
    exit 1
}

# Create output directory structure
$OutputPath = [System.IO.Path]::GetFullPath((Join-Path $SolutionRoot $OutputRoot))
$BuildOutputs = @{
    Root = $OutputPath
    Build = Join-Path $OutputPath "build/$Configuration"
    Tests = Join-Path $OutputPath "tests/$Configuration"
    Packages = Join-Path $OutputPath "packages"
    Reports = Join-Path $OutputPath "reports/$Configuration"
    Logs = Join-Path $OutputPath "logs/$Configuration"
    Artifacts = Join-Path $OutputPath "artifacts/$Configuration"
    Intermediate = Join-Path $OutputPath "intermediate/$Configuration"
    Symbols = Join-Path $OutputPath "symbols/$Configuration"
    Documentation = Join-Path $OutputPath "docs"
}

# Create all output directories
Write-Header "Setting up build environment"
Write-Info "Build ID: $BuildId"
Write-Info "Configuration: $Configuration"
Write-Info "Platform: $Platform"
Write-Info "Output Root: $OutputPath"

foreach ($dir in $BuildOutputs.Values) {
    if (-not (Test-Path $dir)) {
        New-Item -Path $dir -ItemType Directory -Force | Out-Null
        Write-Info "Created directory: $dir"
    }
}

# Build timestamp
$BuildStart = Get-Date
$BuildLog = Join-Path $BuildOutputs.Logs "build-$BuildId.log"

function Write-BuildLog {
    param([string]$Message)
    $Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "[$Timestamp] $Message" | Add-Content -Path $BuildLog
    if ($Verbose) { Write-Host $Message }
}

Write-BuildLog "=== RefactorMCP Comprehensive Build Started ==="
Write-BuildLog "Configuration: $Configuration"
Write-BuildLog "Platform: $Platform"
Write-BuildLog "Output: $OutputPath"

try {
    # Clean if requested
    if ($Clean) {
        Write-Header "Cleaning previous build outputs"
        Write-BuildLog "Cleaning solution..."
        
        & dotnet clean $SolutionFile --configuration $Configuration --verbosity minimal
        if ($LASTEXITCODE -ne 0) { throw "Clean failed" }
        
        # Clean output directories
        foreach ($dir in $BuildOutputs.Values) {
            if (Test-Path $dir) {
                Get-ChildItem -Path $dir -Recurse | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
            }
        }
        
        Write-Success "Clean completed"
        Write-BuildLog "Clean completed successfully"
    }

    # Restore packages
    Write-Header "Restoring NuGet packages"
    Write-BuildLog "Restoring packages..."
    
    & dotnet restore $SolutionFile --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Package restore failed" }
    
    Write-Success "Package restore completed"
    Write-BuildLog "Package restore completed"

    # Build solution with custom output paths
    Write-Header "Building solution"
    Write-BuildLog "Building solution with configuration $Configuration..."
    
    $BuildArgs = @(
        "build", $SolutionFile
        "--configuration", $Configuration
        "--no-restore"
        "--verbosity", "normal"
        "-p:Platform=`"$Platform`""
        "-p:BuildId=$BuildId"
        "-p:AssemblyVersion=$BuildVersion"
        "-p:FileVersion=$BuildVersion"
        "-p:InformationalVersion=$BuildVersion"
        "-p:OutputPath=`"$($BuildOutputs.Build)`""
        "-p:BaseIntermediateOutputPath=`"$($BuildOutputs.Intermediate)\`""
    )
    
    & dotnet @BuildArgs
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
    
    Write-Success "Build completed successfully"
    Write-BuildLog "Build completed successfully"

    # Copy build artifacts
    Write-Header "Organizing build artifacts"
    Write-BuildLog "Copying build artifacts..."
    
    # Find and copy all built assemblies, executables, and related files
    $SourceProjects = @(
        @{Name="RefactorMCP.Core"; Type="Library"}
        @{Name="RefactorMCP.MCP.Server"; Type="Library"}  
        @{Name="RefactorMCP.Web"; Type="Web"}
        @{Name="RefactorMCP.ConsoleApp"; Type="Executable"}
    )
    
    foreach ($project in $SourceProjects) {
        $ProjectName = $project.Name
        $ProjectType = $project.Type
        
        Write-Info "Processing $ProjectName ($ProjectType)..."
        Write-BuildLog "Processing project: $ProjectName"
        
        # Find project build outputs
        $ProjectBuildPath = Join-Path $BuildOutputs.Build $ProjectName
        $ProjectArtifactPath = Join-Path $BuildOutputs.Artifacts $ProjectName
        
        if (Test-Path $ProjectBuildPath) {
            # Create project artifact directory
            New-Item -Path $ProjectArtifactPath -ItemType Directory -Force | Out-Null
            
            # Copy all build outputs
            Copy-Item -Path "$ProjectBuildPath\*" -Destination $ProjectArtifactPath -Recurse -Force
            
            # Copy symbols to dedicated symbols folder
            $SymbolFiles = Get-ChildItem -Path $ProjectBuildPath -Filter "*.pdb" -Recurse
            foreach ($symbol in $SymbolFiles) {
                $SymbolDest = Join-Path $BuildOutputs.Symbols $symbol.Name
                Copy-Item -Path $symbol.FullName -Destination $SymbolDest -Force
            }
            
            Write-Success "Copied artifacts for $ProjectName"
        } else {
            Write-Warning "Build output not found for $ProjectName at $ProjectBuildPath"
        }
    }

    # Run tests if not skipped
    if (-not $SkipTests) {
        Write-Header "Running tests"
        Write-BuildLog "Starting test execution..."
        
        $TestProjects = @(
            "RefactorMCP.Core.Tests"
            "RefactorMCP.MCP.Server.Tests"  
            "RefactorMCP.Web.Tests"
            "RefactorMCP.Tests"
        )
        
        $TestResults = @()
        $AllTestsPassed = $true
        
        foreach ($testProject in $TestProjects) {
            Write-Info "Running tests for $testProject..."
            Write-BuildLog "Running tests: $testProject"
            
            $TestOutputPath = Join-Path $BuildOutputs.Tests $testProject
            New-Item -Path $TestOutputPath -ItemType Directory -Force | Out-Null
            
            $TestLogFile = Join-Path $TestOutputPath "test-results.trx"
            $CoverageFile = Join-Path $TestOutputPath "coverage.xml"
            
            $TestArgs = @(
                "test", "test\$testProject"
                "--configuration", $Configuration
                "--no-build"
                "--verbosity", "normal"
                "--logger", "trx;LogFileName=$TestLogFile"
                "--collect:XPlat Code Coverage"
                "--results-directory", $TestOutputPath
            )
            
            $TestStartTime = Get-Date
            & dotnet @TestArgs
            $TestEndTime = Get-Date
            $TestDuration = $TestEndTime - $TestStartTime
            
            $TestResult = @{
                Project = $testProject
                Success = ($LASTEXITCODE -eq 0)
                Duration = $TestDuration
                OutputPath = $TestOutputPath
            }
            
            $TestResults += $TestResult
            
            if ($TestResult.Success) {
                Write-Success "Tests passed for $testProject (Duration: $($TestDuration.TotalSeconds.ToString('F2'))s)"
                Write-BuildLog "Tests passed: $testProject in $($TestDuration.TotalSeconds.ToString('F2'))s"
            } else {
                Write-Error "Tests failed for $testProject"
                Write-BuildLog "Tests failed: $testProject"
                $AllTestsPassed = $false
            }
        }
        
        # Generate test summary report
        $TestSummaryPath = Join-Path $BuildOutputs.Reports "test-summary.json"
        $TestSummary = @{
            BuildId = $BuildId
            Timestamp = $BuildStart.ToString("o")
            Configuration = $Configuration
            Platform = $Platform
            AllTestsPassed = $AllTestsPassed
            TestResults = $TestResults
            TotalDuration = ($TestResults | Measure-Object -Property Duration -Sum).Sum
        }
        
        $TestSummary | ConvertTo-Json -Depth 10 | Set-Content -Path $TestSummaryPath
        Write-Success "Test summary saved to: $TestSummaryPath"
        
        if (-not $AllTestsPassed) {
            Write-Warning "Some tests failed. Check individual test results for details."
        }
    }

    # Create NuGet packages if not skipped
    if (-not $SkipPackaging) {
        Write-Header "Creating NuGet packages"
        Write-BuildLog "Creating NuGet packages..."
        
        $PackageableProjects = @(
            "src\RefactorMCP.Core"
            "src\RefactorMCP.MCP.Server"
        )
        
        foreach ($project in $PackageableProjects) {
            Write-Info "Packaging $project..."
            Write-BuildLog "Creating package for: $project"
            
            $PackArgs = @(
                "pack", $project
                "--configuration", $Configuration
                "--no-build"
                "--output", $BuildOutputs.Packages
                "-p:PackageVersion=$BuildVersion"
                "--verbosity", "minimal"
            )
            
            & dotnet @PackArgs
            if ($LASTEXITCODE -eq 0) {
                Write-Success "Package created for $project"
                Write-BuildLog "Package created successfully: $project"
            } else {
                Write-Warning "Package creation failed for $project"
                Write-BuildLog "Package creation failed: $project"
            }
        }
    }

    # Generate build manifest
    Write-Header "Generating build manifest"
    Write-BuildLog "Generating build manifest..."
    
    $BuildEnd = Get-Date
    $BuildDuration = $BuildEnd - $BuildStart
    
    $BuildManifest = @{
        BuildInfo = @{
            BuildId = $BuildId
            Version = $BuildVersion
            Configuration = $Configuration
            Platform = $Platform
            StartTime = $BuildStart.ToString("o")
            EndTime = $BuildEnd.ToString("o")
            Duration = $BuildDuration.ToString()
            Success = $true
        }
        Environment = @{
            MachineName = $env:COMPUTERNAME
            UserName = $env:USERNAME
            DotNetVersion = (& dotnet --version)
            OSVersion = [System.Environment]::OSVersion.VersionString
        }
        Outputs = @{
            BuildArtifacts = $BuildOutputs.Artifacts
            TestResults = $BuildOutputs.Tests
            Packages = $BuildOutputs.Packages
            Reports = $BuildOutputs.Reports
            Symbols = $BuildOutputs.Symbols
        }
        Statistics = @{
            BuildDurationSeconds = $BuildDuration.TotalSeconds
            ArtifactCount = (Get-ChildItem -Path $BuildOutputs.Artifacts -Recurse -File | Measure-Object).Count
            PackageCount = if (Test-Path $BuildOutputs.Packages) { (Get-ChildItem -Path $BuildOutputs.Packages -Filter "*.nupkg" | Measure-Object).Count } else { 0 }
            TestProjects = if (-not $SkipTests) { $TestResults.Count } else { "Skipped" }
        }
    }
    
    $ManifestPath = Join-Path $BuildOutputs.Root "build-manifest.json"
    $BuildManifest | ConvertTo-Json -Depth 10 | Set-Content -Path $ManifestPath
    
    Write-Success "Build manifest created: $ManifestPath"
    Write-BuildLog "Build manifest generated successfully"

    # Create distribution archives
    Write-Header "Creating distribution archives"
    Write-BuildLog "Creating distribution archives..."
    
    $DistributionPath = Join-Path $BuildOutputs.Root "distributions"
    New-Item -Path $DistributionPath -ItemType Directory -Force | Out-Null
    
    # Create console app distribution
    $ConsoleAppPath = Join-Path $BuildOutputs.Artifacts "RefactorMCP.ConsoleApp"
    if (Test-Path $ConsoleAppPath) {
        $ConsoleZip = Join-Path $DistributionPath "RefactorMCP-Console-$Configuration-$BuildId.zip"
        Compress-Archive -Path "$ConsoleAppPath\*" -DestinationPath $ConsoleZip -Force
        Write-Success "Console app distribution: $ConsoleZip"
    }
    
    # Create web app distribution
    $WebAppPath = Join-Path $BuildOutputs.Artifacts "RefactorMCP.Web"
    if (Test-Path $WebAppPath) {
        $WebZip = Join-Path $DistributionPath "RefactorMCP-Web-$Configuration-$BuildId.zip"
        Compress-Archive -Path "$WebAppPath\*" -DestinationPath $WebZip -Force
        Write-Success "Web app distribution: $WebZip"
    }

    # Final success message
    Write-Header "Build completed successfully! 🎉"
    Write-Success "Build ID: $BuildId"
    Write-Success "Duration: $($BuildDuration.TotalMinutes.ToString('F2')) minutes"
    Write-Success "Output directory: $OutputPath"
    Write-Success "Manifest: $ManifestPath"
    
    Write-BuildLog "=== Build completed successfully ==="
    Write-BuildLog "Total duration: $($BuildDuration.TotalMinutes.ToString('F2')) minutes"

} catch {
    $BuildEnd = Get-Date
    $BuildDuration = $BuildEnd - $BuildStart
    
    Write-Error "Build failed: $($_.Exception.Message)"
    Write-BuildLog "BUILD FAILED: $($_.Exception.Message)"
    Write-BuildLog "Build duration before failure: $($BuildDuration.TotalMinutes.ToString('F2')) minutes"
    
    # Create failure manifest
    $FailureManifest = @{
        BuildInfo = @{
            BuildId = $BuildId
            Configuration = $Configuration
            Platform = $Platform
            StartTime = $BuildStart.ToString("o")
            FailureTime = $BuildEnd.ToString("o")
            Duration = $BuildDuration.ToString()
            Success = $false
            Error = $_.Exception.Message
        }
        Environment = @{
            MachineName = $env:COMPUTERNAME
            UserName = $env:USERNAME
        }
    }
    
    $FailureManifestPath = Join-Path $BuildOutputs.Root "build-failure.json"
    $FailureManifest | ConvertTo-Json -Depth 10 | Set-Content -Path $FailureManifestPath
    
    exit 1
}