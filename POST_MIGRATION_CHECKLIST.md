# Post-Migration Verification Checklist

This checklist ensures the migration from `src/` and `test/` to `code/src/` and `code/test/` completed successfully and the solution is ready for clean builds.

## Pre-Verification Setup
- [ ] Navigate to the migrated solution directory
- [ ] Ensure you have .NET 9.0 SDK installed
- [ ] Close Visual Studio if open

## 1. Directory Structure Verification

### Expected Source Projects in `code/src/`:
- [ ] `RefactorMCP.ConsoleApp` - Console application project
- [ ] `RefactorMCP.Core` - Core library with abstractions and services
- [ ] `RefactorMCP.MCP.Server` - MCP server implementation
- [ ] `RefactorMCP.UI` - New MudBlazor UI project (clean)
- [ ] `RefactorMCP.Web` - Original web project (may have issues, but migrated anyway)

### Expected Test Projects in `code/test/`:
- [ ] `RefactorMCP.Core.Tests` - Core library tests
- [ ] `RefactorMCP.MCP.Server.Tests` - MCP server tests
- [ ] `RefactorMCP.Tests` - General solution tests
- [ ] `RefactorMCP.Web.Tests` - Web project tests

### Configuration Files:
- [ ] `Directory.Packages.props` - Package version management (migrated)
- [ ] New solution file created (old RefactorMCP.sln excluded from migration)
- [ ] Directory.Build.props excluded from migration (as planned)

## 2. Create Minimal Directory.Build.props

Create a new minimal `Directory.Build.props` in the root of the migrated solution:

```xml
<Project>
  <PropertyGroup>
    <!-- External build configuration -->
    <BaseOutputPath>F:\Dynamic\Refactor\Refactor\bin\</BaseOutputPath>
    <OutputPath>$(BaseOutputPath)$(MSBuildProjectName)\$(Configuration)\</OutputPath>
    <BaseIntermediateOutputPath>F:\Dynamic\Refactor\Refactor\obj\</BaseIntermediateOutputPath>
    <IntermediateOutputPath>$(BaseIntermediateOutputPath)$(MSBuildProjectName)\$(Configuration)\</IntermediateOutputPath>
  </PropertyGroup>
</Project>
```

- [ ] Created minimal Directory.Build.props for external builds
- [ ] Verified build paths point to `F:\Dynamic\Refactor\Refactor\`
- [ ] No package versions in Directory.Build.props (all in Directory.Packages.props)

## 3. Solution File Recreation

- [ ] Create new solution file: `dotnet new sln -n RefactorMCP`
- [ ] Add all projects to solution:
  ```bash
  dotnet sln add code/src/RefactorMCP.ConsoleApp/RefactorMCP.ConsoleApp.csproj
  dotnet sln add code/src/RefactorMCP.Core/RefactorMCP.Core.csproj
  dotnet sln add code/src/RefactorMCP.MCP.Server/RefactorMCP.MCP.Server.csproj
  dotnet sln add code/src/RefactorMCP.UI/RefactorMCP.UI.csproj
  dotnet sln add code/src/RefactorMCP.Web/RefactorMCP.Web.csproj
  dotnet sln add code/test/RefactorMCP.Core.Tests/RefactorMCP.Core.Tests.csproj
  dotnet sln add code/test/RefactorMCP.MCP.Server.Tests/RefactorMCP.MCP.Server.Tests.csproj
  dotnet sln add code/test/RefactorMCP.Tests/RefactorMCP.Tests.csproj
  dotnet sln add code/test/RefactorMCP.Web.Tests/RefactorMCP.Web.Tests.csproj
  ```

## 4. Build Verification

### External Build Path Verification:
- [ ] Run `dotnet clean` to ensure clean start
- [ ] Run `dotnet restore` to restore packages
- [ ] Verify packages restore correctly with CPM
- [ ] Check that no build outputs appear in project folders
- [ ] Verify build outputs go to `F:\Dynamic\Refactor\Refactor\bin\`
- [ ] Verify intermediate outputs go to `F:\Dynamic\Refactor\Refactor\obj\`

### Project Build Status:
- [ ] `RefactorMCP.Core` builds successfully
- [ ] `RefactorMCP.MCP.Server` builds successfully  
- [ ] `RefactorMCP.ConsoleApp` builds successfully
- [ ] `RefactorMCP.UI` builds successfully (new clean project)
- [ ] `RefactorMCP.Web` builds (may have minor issues, but should be resolvable)
- [ ] All test projects build successfully

### Full Solution Build:
- [ ] Run `dotnet build` on solution - should complete without errors
- [ ] Verify all projects compile to external directory
- [ ] No package recognition errors
- [ ] No CPM duplicate package errors

## 5. Package Management Verification

- [ ] Verify Directory.Packages.props contains all required packages
- [ ] No duplicate PackageVersion entries
- [ ] All projects reference packages correctly via CPM
- [ ] MudBlazor packages available for RefactorMCP.UI
- [ ] OpenTelemetry packages available for metrics/tracing
- [ ] Entity Framework packages available
- [ ] No package downgrade warnings

## 6. Project-Specific Checks

### RefactorMCP.UI (New Clean Project):
- [ ] MudBlazor components render correctly
- [ ] Identity/authentication works
- [ ] No package recognition issues
- [ ] Pages load without errors

### RefactorMCP.Web (Legacy Project):
- [ ] Builds without critical errors
- [ ] Minor bind-value syntax issues acceptable (easily fixable)
- [ ] Entity Framework references resolved

### RefactorMCP.Core:
- [ ] All abstractions and services present
- [ ] No missing dependencies
- [ ] Builds cleanly

### RefactorMCP.MCP.Server:
- [ ] MCP server functionality intact
- [ ] Tools and resources migrated correctly
- [ ] No missing OpenTelemetry references

## 7. Migration Completeness Check

Compare with original project structure:
- [ ] All source files migrated (676 files, 2.7 MB as planned)
- [ ] File count matches migration plan
- [ ] No critical files missing
- [ ] Directory.Packages.props migrated correctly
- [ ] Directory.Build.props correctly excluded
- [ ] RefactorMCP.sln correctly excluded
- [ ] OldCode, OldTests, PythonScript folders correctly excluded

## 8. Final Integration Test

- [ ] Open solution in Visual Studio
- [ ] All projects load correctly
- [ ] Solution Explorer shows clean structure
- [ ] Set RefactorMCP.UI as startup project
- [ ] Run the application - should start without errors
- [ ] Basic functionality test (navigate pages, verify no crashes)

## 9. Clean Environment Verification

- [ ] No build artifacts in project directories
- [ ] All builds go to external F:\Dynamic\Refactor\Refactor\ folder
- [ ] No cache/corruption issues from old project
- [ ] Package restore works reliably
- [ ] No unknown/mysterious errors

## Success Criteria

✅ **Migration Successful When:**
- All 9 projects present and build successfully
- Builds go to external directory as configured
- No package recognition errors
- No CPM duplicate package issues
- RefactorMCP.UI (clean project) works perfectly
- RefactorMCP.Web compiles (minor issues acceptable)
- Solution can be opened and run in Visual Studio
- No critical functionality lost

## Notes

- RefactorMCP.Web was the problematic project but migrated anyway
- RefactorMCP.UI is the new clean MudBlazor project that should work perfectly
- Minor bind-value syntax errors in RefactorMCP.Web are acceptable and easily fixable
- Focus is on having a clean, corruption-free development environment
- External builds ensure no local folder pollution

---
*This checklist ensures the migration resolved the original package recognition and build corruption issues while maintaining all project functionality.*