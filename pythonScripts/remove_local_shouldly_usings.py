#!/usr/bin/env python3
"""
Remove local 'using Shouldly;' statements from test files
since Shouldly is now imported via GlobalUsings.cs
"""

from pathlib import Path
import re

def remove_local_shouldly_usings(file_path: Path):
    """Remove local using Shouldly statements from a file"""
    try:
        content = file_path.read_text(encoding='utf-8')
        
        # Remove lines that are exactly "using Shouldly;"
        lines = content.splitlines()
        modified_lines = []
        removed = False
        
        for line in lines:
            if line.strip() == 'using Shouldly;':
                removed = True
                continue  # Skip this line
            modified_lines.append(line)
        
        if removed:
            # Write back the modified content
            file_path.write_text('\n'.join(modified_lines), encoding='utf-8')
            print(f"Removed 'using Shouldly;' from {file_path.name}")
            return True
        
        return False
        
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
        return False

def main():
    """Process all test files to remove local Shouldly usings"""
    test_files = [
        "RefactorMCP.Core.Tests/Extensions/ServiceCollectionExtensionsTests.cs",
        "RefactorMCP.Core.Tests/Services/RefactoringServiceTests.cs",
        "RefactorMCP.Core.Tests/SyntaxRewriters/ConstructorInjectionRewriterTests.cs",
        "RefactorMCP.Core.Tests/Tools/CleanupUsingsToolTests.cs",
        "RefactorMCP.Core.Tests/Tools/RefactoringHelpersTests.cs",
        "RefactorMCP.MCP.Server.Tests/Services/McpServerBuilderTests.cs",
        "RefactorMCP.Web.Tests/Components/IndexPageTests.cs",
        "RefactorMCP.Web.Tests/Controllers/McpControllerTests.cs",
        "RefactorMCP.Web.Tests/Integration/WebApplicationTests.cs",
        "RefactorMCP.Web.Tests/Services/DashboardServiceTests.cs"
    ]
    
    base_dir = Path("F:/Dynamic/Refactor/refactor-mcp/code/test")
    total_processed = 0
    
    for file_path in test_files:
        full_path = base_dir / file_path
        if full_path.exists():
            if remove_local_shouldly_usings(full_path):
                total_processed += 1
        else:
            print(f"File not found: {full_path}")
    
    print(f"\nTotal files processed: {total_processed}")

if __name__ == "__main__":
    main()