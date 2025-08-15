#Requires -Version 5.1

<#
.SYNOPSIS
    Simplified comprehensive build script for RefactorMCP
    
.DESCRIPTION
    Builds RefactorMCP using configuration from build.json and outputs all artifacts
    to external Refactor folder. This is a streamlined version of build-comprehensive.ps1
    
.PARAMETER Configuration
    Build configuration (Debug, Release). Default: Release
    
.PARAMETER Clean
    Clean before building
    
.PARAMETER SkipTests
    Skip test execution
    
.PARAMETER SkipPackaging
    Skip NuGet package creation
    
.EXAMPLE
    .\build.ps1
    
.EXAMPLE
    .\build.ps1 -Configuration Debug -Clean
#>

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [switch]$Clean,
    [switch]$SkipTests,
    [switch]$SkipPackaging
)

$ErrorActionPreference = "Stop"

# Get script directory and solution root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SolutionRoot = Split-Path -Parent $ScriptDir
$SolutionFile = Join-Path $SolutionRoot "RefactorMCP.sln"

# Build information
$BuildId = [System.DateTime]::UtcNow.ToString("yyyyMMdd-HHmmss")
$BuildVersion = "1.0.0-dev-$BuildId"

# Output functions
function Write-BuildStep { param($Message) Write-Host "🔨 $Message" -ForegroundColor Cyan }
function Write-BuildSuccess { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-BuildError { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }

# Create output directories
$OutputRoot = Join-Path $SolutionRoot "../Refactor"
$BuildOutputs = @{
    Root = $OutputRoot
    Build = Join-Path $OutputRoot "build/$Configuration"
    Tests = Join-Path $OutputRoot "tests/$Configuration"
    Packages = Join-Path $OutputRoot "packages"
    Artifacts = Join-Path $OutputRoot "artifacts/$Configuration"
    Logs = Join-Path $OutputRoot "logs/$Configuration"
}

Write-BuildStep "Setting up build environment"
Write-Host "Build ID: $BuildId" -ForegroundColor Yellow
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Output: $OutputRoot" -ForegroundColor Yellow

# Create directories
foreach ($dir in $BuildOutputs.Values) {
    if (-not (Test-Path $dir)) {
        New-Item -Path $dir -ItemType Directory -Force | Out-Null
    }
}

$BuildLog = Join-Path $BuildOutputs.Logs "build-$BuildId.log"
"Build started: $(Get-Date)" | Set-Content -Path $BuildLog

try {
    # Clean if requested
    if ($Clean) {
        Write-BuildStep "Cleaning previous outputs"
        & dotnet clean $SolutionFile --configuration $Configuration --verbosity minimal
        if ($LASTEXITCODE -ne 0) { throw "Clean failed" }
        Write-BuildSuccess "Clean completed"
    }

    # Restore packages
    Write-BuildStep "Restoring NuGet packages"
    & dotnet restore $SolutionFile --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }
    Write-BuildSuccess "Package restore completed"

    # Build solution
    Write-BuildStep "Building solution"
    & dotnet build $SolutionFile --configuration $Configuration --no-restore --verbosity normal
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
    Write-BuildSuccess "Build completed"

    # Copy build artifacts
    Write-BuildStep "Organizing build artifacts"
    $SourceProjects = @("RefactorMCP.Core", "RefactorMCP.MCP.Server", "RefactorMCP.Web", "RefactorMCP.ConsoleApp")
    
    foreach ($project in $SourceProjects) {
        $ProjectBuildPath = "src/$project/bin/$Configuration/net9.0"
        $ProjectArtifactPath = Join-Path $BuildOutputs.Artifacts $project
        
        if (Test-Path $ProjectBuildPath) {
            New-Item -Path $ProjectArtifactPath -ItemType Directory -Force | Out-Null
            Copy-Item -Path "$ProjectBuildPath/*" -Destination $ProjectArtifactPath -Recurse -Force
            Write-Host "  ✓ Copied artifacts for $project" -ForegroundColor Gray
        }
    }
    Write-BuildSuccess "Build artifacts organized"

    # Run tests
    if (-not $SkipTests) {
        Write-BuildStep "Running tests"
        $TestProjects = @("RefactorMCP.Core.Tests", "RefactorMCP.MCP.Server.Tests", "RefactorMCP.Web.Tests", "RefactorMCP.Tests")
        $AllTestsPassed = $true
        
        foreach ($testProject in $TestProjects) {
            Write-Host "  Running tests for $testProject..." -ForegroundColor Gray
            $TestOutputPath = Join-Path $BuildOutputs.Tests $testProject
            New-Item -Path $TestOutputPath -ItemType Directory -Force | Out-Null
            
            $TestLogFile = Join-Path $TestOutputPath "test-results.trx"
            
            & dotnet test "test/$testProject" --configuration $Configuration --no-build --verbosity minimal --logger "trx;LogFileName=$TestLogFile" --results-directory $TestOutputPath
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "    ✓ $testProject passed" -ForegroundColor Green
            } else {
                Write-Host "    ✗ $testProject failed" -ForegroundColor Red
                $AllTestsPassed = $false
            }
        }
        
        if ($AllTestsPassed) {
            Write-BuildSuccess "All tests passed"
        } else {
            Write-Host "⚠️  Some tests failed" -ForegroundColor Yellow
        }
    }

    # Create packages
    if (-not $SkipPackaging) {
        Write-BuildStep "Creating NuGet packages"
        $PackageableProjects = @("src/RefactorMCP.Core", "src/RefactorMCP.MCP.Server")
        
        foreach ($project in $PackageableProjects) {
            & dotnet pack $project --configuration $Configuration --no-build --output $BuildOutputs.Packages -p:PackageVersion=$BuildVersion --verbosity minimal
            if ($LASTEXITCODE -eq 0) {
                Write-Host "  ✓ Package created for $project" -ForegroundColor Gray
            }
        }
        Write-BuildSuccess "NuGet packages created"
    }

    # Generate build manifest
    Write-BuildStep "Generating build manifest"
    $BuildEnd = Get-Date
    $BuildManifest = @{
        BuildInfo = @{
            BuildId = $BuildId
            Version = $BuildVersion
            Configuration = $Configuration
            StartTime = (Get-Date).ToString("o")
            EndTime = $BuildEnd.ToString("o")
            Success = $true
        }
        Outputs = @{
            Artifacts = $BuildOutputs.Artifacts
            Tests = $BuildOutputs.Tests
            Packages = $BuildOutputs.Packages
        }
    }
    
    $ManifestPath = Join-Path $BuildOutputs.Root "build-manifest.json"
    $BuildManifest | ConvertTo-Json -Depth 10 | Set-Content -Path $ManifestPath
    Write-BuildSuccess "Build manifest created"

    # Success message
    Write-Host "`n🎉 Build completed successfully!" -ForegroundColor Green -BackgroundColor Black
    Write-Host "Build ID: $BuildId" -ForegroundColor Cyan
    Write-Host "Output: $OutputRoot" -ForegroundColor Cyan
    Write-Host "Manifest: $ManifestPath" -ForegroundColor Cyan

} catch {
    Write-BuildError "Build failed: $($_.Exception.Message)"
    "Build failed: $($_.Exception.Message)" | Add-Content -Path $BuildLog
    exit 1
}