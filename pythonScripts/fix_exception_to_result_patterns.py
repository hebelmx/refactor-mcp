#!/usr/bin/env python3
"""
Exception to Result<T> Pattern Converter
Converts tests expecting ArgumentNullException to Result<T> failure patterns
"""

import os
import re
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Tuple, Optional
import shutil

class ExceptionToResultFixer:
    def __init__(self, dry_run=True):
        self.dry_run = dry_run
        self.backup_dir = Path("test_backups") / f"exception_to_result_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
        self.target_dir = Path(r"F:\Dynamic\IndTraceV2025\Src\Tests\Core\Application.UnitTests")
        self.changes_made = []
        self.fixes_applied = 0
        
    def backup_file(self, file_path: Path) -> None:
        """Create backup of file before modification"""
        if not self.dry_run:
            backup_path = self.backup_dir / file_path.relative_to(self.target_dir)
            backup_path.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(file_path, backup_path)
    
    def find_exception_tests(self) -> List[Tuple[Path, int]]:
        """Find all tests expecting ArgumentNullException"""
        exception_tests = []
        
        # Search for ShouldThrowArgumentNullException pattern
        for file_path in self.target_dir.rglob("*Tests.cs"):
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                    
                # Count occurrences of exception patterns
                patterns = [
                    r'ShouldThrowArgumentNullException',
                    r'Should\.Throw<ArgumentNullException>',
                    r'Throws<ArgumentNullException>',
                    r'Assert\.Throws<ArgumentNullException>'
                ]
                
                count = 0
                for pattern in patterns:
                    count += len(re.findall(pattern, content))
                    
                if count > 0:
                    exception_tests.append((file_path, count))
                    
            except Exception as e:
                print(f"Error reading {file_path}: {e}")
                
        return sorted(exception_tests, key=lambda x: x[1], reverse=True)
    
    def convert_exception_to_result(self, content: str, file_path: Path) -> Tuple[str, List[str]]:
        """Convert exception expectations to Result<T> patterns"""
        changes = []
        lines = content.split('\n')
        modified_lines = []
        i = 0
        
        while i < len(lines):
            line = lines[i]
            
            # Skip constructor tests - they should remain as exceptions until static factory pattern
            if 'Constructor' in line and 'ShouldThrowArgumentNullException' in line:
                modified_lines.append(line)
                i += 1
                continue
            
            # Pattern 1: Method name with ShouldThrowArgumentNullException
            if 'ShouldThrowArgumentNullException' in line and ('public void' in line or '[Fact]' in lines[max(0, i-2):i+1]):
                # Change method name
                new_line = line.replace('ShouldThrowArgumentNullException', 'ShouldReturnFailureResult')
                modified_lines.append(new_line)
                changes.append(f"Line {i+1}: Updated method name to use Result pattern")
                i += 1
                continue
                
            # Pattern 2: Should.Throw<ArgumentNullException>
            elif 'Should.Throw<ArgumentNullException>' in line:
                # Find the method call being tested
                method_match = re.search(r'Should\.Throw<ArgumentNullException>\s*\(\s*\(\s*\)\s*=>\s*(.+?)\)', line)
                if method_match:
                    method_call = method_match.group(1).strip()
                    
                    # Replace with Result<T> assertion
                    indent = len(line) - len(line.lstrip())
                    new_lines = [
                        ' ' * indent + '// Act',
                        ' ' * indent + f'var result = {method_call};',
                        ' ' * indent + '',
                        ' ' * indent + '// Assert', 
                        ' ' * indent + 'result.IsFailure.ShouldBeTrue();',
                        ' ' * indent + 'result.Errors.ShouldNotBeNull();',
                        ' ' * indent + 'result.Errors.Count.ShouldBeGreaterThan(0);'
                    ]
                    
                    # Check if there's a parameter name assertion
                    if i + 1 < len(lines) and 'ParamName' in lines[i + 1]:
                        param_match = re.search(r'\.ParamName\.ShouldBe\("(.+?)"\)', lines[i + 1])
                        if param_match:
                            param_name = param_match.group(1)
                            # Add appropriate error message check based on parameter
                            if param_name == 'src':
                                # Extract type name from method call
                                type_match = re.search(r'(\w+)\.ToDto', method_call)
                                if type_match:
                                    type_name = type_match.group(1)
                                    new_lines.append(' ' * indent + f'result.Errors.ShouldContain("{type_name} source cannot be null");')
                            else:
                                new_lines.append(' ' * indent + f'result.Errors.ShouldContain("{param_name}");')
                        i += 1  # Skip the ParamName line
                    
                    modified_lines.extend(new_lines)
                    changes.append(f"Line {i+1}: Converted Should.Throw to Result<T> pattern")
                    i += 1
                    continue
                    
            # Pattern 3: Assert.Throws<ArgumentNullException>
            elif 'Assert.Throws<ArgumentNullException>' in line:
                # Similar to pattern 2 but for xUnit style
                method_match = re.search(r'Assert\.Throws<ArgumentNullException>\s*\(\s*\(\s*\)\s*=>\s*(.+?)\)', line)
                if method_match:
                    method_call = method_match.group(1).strip()
                    indent = len(line) - len(line.lstrip())
                    
                    new_lines = [
                        ' ' * indent + '// Act',
                        ' ' * indent + f'var result = {method_call};',
                        ' ' * indent + '',
                        ' ' * indent + '// Assert',
                        ' ' * indent + 'result.IsFailure.ShouldBeTrue();',
                        ' ' * indent + 'result.Errors.ShouldNotBeNull();'
                    ]
                    
                    modified_lines.extend(new_lines)
                    changes.append(f"Line {i+1}: Converted Assert.Throws to Result<T> pattern")
                    i += 1
                    continue
                    
            # Default: keep line as is
            modified_lines.append(line)
            i += 1
            
        return '\n'.join(modified_lines), changes
    
    def fix_file(self, file_path: Path) -> Tuple[bool, List[str]]:
        """Fix exception patterns in a single file"""
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
                
            original_content = content
            new_content, changes = self.convert_exception_to_result(content, file_path)
            
            if new_content != original_content:
                if not self.dry_run:
                    self.backup_file(file_path)
                    with open(file_path, 'w', encoding='utf-8') as f:
                        f.write(new_content)
                        
                self.fixes_applied += len(changes)
                return True, changes
            else:
                return False, []
                
        except Exception as e:
            print(f"Error processing {file_path}: {str(e)}")
            return False, []
    
    def run(self):
        """Run the exception to result fixer"""
        print("Exception to Result<T> Pattern Converter")
        print("=" * 60)
        print(f"Mode: {'DRY RUN' if self.dry_run else 'LIVE EXECUTION'}")
        print(f"Target directory: {self.target_dir}")
        print()
        
        # Find files with exception tests
        exception_files = self.find_exception_tests()
        print(f"Found {len(exception_files)} files with exception test patterns")
        print()
        
        # Process top files
        for file_path, count in exception_files[:10]:
            print(f"Processing: {file_path.relative_to(self.target_dir)} ({count} patterns)")
            
            modified, changes = self.fix_file(file_path)
            if modified:
                print(f"  [FIXED] {len(changes)} patterns")
                for change in changes[:3]:
                    print(f"    {change}")
                if len(changes) > 3:
                    print(f"    ... and {len(changes) - 3} more changes")
            else:
                print(f"  - No changes needed")
                
        # Summary
        print("\n" + "=" * 60)
        print("SUMMARY")
        print("=" * 60)
        print(f"Files processed: {min(len(exception_files), 10)}")
        print(f"Total fixes applied: {self.fixes_applied}")
        
        if self.dry_run:
            print("\nDRY RUN COMPLETE - No files were modified")
            print("Run with --apply to execute changes")
        else:
            print(f"\nLIVE EXECUTION COMPLETE")
            print(f"Backups created in: {self.backup_dir}")

if __name__ == "__main__":
    import sys
    
    dry_run = "--apply" not in sys.argv
    fixer = ExceptionToResultFixer(dry_run=dry_run)
    fixer.run()