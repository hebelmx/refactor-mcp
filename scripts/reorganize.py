#!/usr/bin/env python3
"""
Script to reorganize RefactorMCP solution into src/ and test/ folders
"""
import os
import shutil
import re
from pathlib import Path

def main():
    base_dir = Path(".")
    src_dir = base_dir / "src"
    test_dir = base_dir / "test"
    
    # Create directories if they don't exist
    src_dir.mkdir(exist_ok=True)
    test_dir.mkdir(exist_ok=True)
    
    # Source projects to move to src/
    source_projects = [
        "RefactorMCP.Core",
        "RefactorMCP.MCP.Server", 
        "RefactorMCP.Web"
    ]
    
    # Test projects to move to test/
    test_projects = [
        "RefactorMCP.Tests",
        "RefactorMCP.Core.Tests",
        "RefactorMCP.MCP.Server.Tests",
        "RefactorMCP.Web.Tests"
    ]
    
    # Move source projects
    for project in source_projects:
        if (base_dir / project).exists():
            target = src_dir / project
            if not target.exists():
                print(f"Moving {project} to src/")
                shutil.move(str(base_dir / project), str(target))
            else:
                print(f"src/{project} already exists, skipping")
    
    # Move test projects  
    for project in test_projects:
        if (base_dir / project).exists():
            target = test_dir / project
            if not target.exists():
                print(f"Moving {project} to test/")
                shutil.move(str(base_dir / project), str(target))
            else:
                print(f"test/{project} already exists, skipping")
    
    print("Project reorganization completed!")
    print("\nNext steps:")
    print("1. Update solution file")
    print("2. Update project references")
    print("3. Test build")

if __name__ == "__main__":
    main()