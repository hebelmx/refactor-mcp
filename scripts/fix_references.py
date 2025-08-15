#!/usr/bin/env python3
"""
Script to fix remaining project reference issues
"""
import os
import re
from pathlib import Path

def fix_all_references():
    """Fix all remaining project reference issues"""
    
    # Project reference fixes for test projects
    test_project_fixes = [
        # Core Tests - needs to reference Core from ../src
        ("test/RefactorMCP.Core.Tests/RefactorMCP.Core.Tests.csproj", 
         '../src/RefactorMCP.Core/RefactorMCP.Core.csproj'),
        
        # MCP Server Tests - needs Core and MCP.Server from ../src  
        ("test/RefactorMCP.MCP.Server.Tests/RefactorMCP.MCP.Server.Tests.csproj",
         '../src/RefactorMCP.Core/RefactorMCP.Core.csproj'),
        ("test/RefactorMCP.MCP.Server.Tests/RefactorMCP.MCP.Server.Tests.csproj",
         '../src/RefactorMCP.MCP.Server/RefactorMCP.MCP.Server.csproj'),
         
        # Web Tests - needs all src projects
        ("test/RefactorMCP.Web.Tests/RefactorMCP.Web.Tests.csproj",
         '../src/RefactorMCP.Web/RefactorMCP.Web.csproj'),
        ("test/RefactorMCP.Web.Tests/RefactorMCP.Web.Tests.csproj", 
         '../src/RefactorMCP.Core/RefactorMCP.Core.csproj'),
        ("test/RefactorMCP.Web.Tests/RefactorMCP.Web.Tests.csproj",
         '../src/RefactorMCP.MCP.Server/RefactorMCP.MCP.Server.csproj'),
    ]
    
    # Source project fixes
    src_project_fixes = [
        # MCP Server needs Core
        ("src/RefactorMCP.MCP.Server/RefactorMCP.MCP.Server.csproj",
         "../RefactorMCP.Core/RefactorMCP.Core.csproj"),
         
        # Web needs Core and MCP.Server
        ("src/RefactorMCP.Web/RefactorMCP.Web.csproj",
         "../RefactorMCP.Core/RefactorMCP.Core.csproj"),
        ("src/RefactorMCP.Web/RefactorMCP.Web.csproj", 
         "../RefactorMCP.MCP.Server/RefactorMCP.MCP.Server.csproj"),
    ]
    
    # Read and update each project file
    all_csproj_files = []
    for root, dirs, files in os.walk("."):
        for file in files:
            if file.endswith(".csproj"):
                all_csproj_files.append(Path(root) / file)
    
    print(f"Found {len(all_csproj_files)} .csproj files")
    
    for csproj_file in all_csproj_files:
        print(f"\nProcessing {csproj_file}")
        try:
            with open(csproj_file, 'r', encoding='utf-8') as f:
                content = f.read()
            
            original_content = content
            
            # Fix incorrect paths
            problematic_patterns = [
                (r'\.\.\\src\\RefactorMCP\.Core\\RefactorMCP\.Core\.csproj', 
                 '..\\src\\RefactorMCP.Core\\RefactorMCP.Core.csproj'),
                (r'\.\.\\src\\RefactorMCP\.MCP\.Server\\RefactorMCP\.MCP\.Server\.csproj',
                 '..\\src\\RefactorMCP.MCP.Server\\RefactorMCP.MCP.Server.csproj'),
                (r'\.\.\\src\\RefactorMCP\.Web\\RefactorMCP\.Web\.csproj',
                 '..\\src\\RefactorMCP.Web\\RefactorMCP.Web.csproj'),
                # Fix double src paths
                (r'\.\.\\src\\src\\', '..\\src\\'),
                (r'../src/src/', '../src/'),
            ]
            
            for pattern, replacement in problematic_patterns:
                content = re.sub(pattern, replacement, content)
            
            # Write back if changed
            if content != original_content:
                with open(csproj_file, 'w', encoding='utf-8') as f:
                    f.write(content)
                print(f"  Updated {csproj_file}")
            else:
                print(f"  No changes needed")
                
        except Exception as e:
            print(f"  Error: {e}")

if __name__ == "__main__":
    print("Fixing project references...")
    fix_all_references()
    print("\nReference fixes completed!")