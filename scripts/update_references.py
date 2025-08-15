#!/usr/bin/env python3
"""
Script to update project references after reorganization
"""
import os
import re
from pathlib import Path

def update_project_references():
    """Update all .csproj files with new relative paths"""
    
    # Define the reference mappings (old -> new)
    reference_mappings = {
        "../RefactorMCP.Core/RefactorMCP.Core.csproj": "../src/RefactorMCP.Core/RefactorMCP.Core.csproj",
        "../RefactorMCP.MCP.Server/RefactorMCP.MCP.Server.csproj": "../src/RefactorMCP.MCP.Server/RefactorMCP.MCP.Server.csproj", 
        "../RefactorMCP.Web/RefactorMCP.Web.csproj": "../src/RefactorMCP.Web/RefactorMCP.Web.csproj",
        "..\\RefactorMCP.Core\\RefactorMCP.Core.csproj": "..\\src\\RefactorMCP.Core\\RefactorMCP.Core.csproj",
        "..\\RefactorMCP.MCP.Server\\RefactorMCP.MCP.Server.csproj": "..\\src\\RefactorMCP.MCP.Server\\RefactorMCP.MCP.Server.csproj",
        "..\\RefactorMCP.Web\\RefactorMCP.Web.csproj": "..\\src\\RefactorMCP.Web\\RefactorMCP.Web.csproj",
    }
    
    # Find all .csproj files in test and src directories
    for root, dirs, files in os.walk("."):
        for file in files:
            if file.endswith(".csproj"):
                file_path = Path(root) / file
                print(f"Checking {file_path}")
                
                # Read the file
                try:
                    with open(file_path, 'r', encoding='utf-8') as f:
                        content = f.read()
                    
                    # Track if any changes were made
                    original_content = content
                    
                    # Update references
                    for old_ref, new_ref in reference_mappings.items():
                        if old_ref in content:
                            print(f"  Updating reference: {old_ref} -> {new_ref}")
                            content = content.replace(old_ref, new_ref)
                    
                    # Write back if changed
                    if content != original_content:
                        with open(file_path, 'w', encoding='utf-8') as f:
                            f.write(content)
                        print(f"  Updated {file_path}")
                    else:
                        print(f"  No changes needed for {file_path}")
                        
                except Exception as e:
                    print(f"  Error processing {file_path}: {e}")

if __name__ == "__main__":
    print("Updating project references...")
    update_project_references()
    print("Project reference updates completed!")