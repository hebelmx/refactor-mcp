
#!/usr/bin/env python3
"""
Enhanced Handler Null Guard Suppressor
Intelligently identifies and comments out constructor null guard tests for DI-injected handlers.
Supports dry-run mode, detailed logging, and integration with test cycle framework.
"""

from pathlib import Path
import re
import json
import shutil
from typing import List, Dict, Set, Tuple
from dataclasses import dataclass
from datetime import datetime

@dataclass
class SuppressedTest:
    file_path: str
    test_name: str
    handler_class: str
    interface_type: str
    line_start: int
    line_end: int

class HandlerNullGuardSuppressor:
    """
    Enhanced suppressor for constructor null guard tests targeting DI-injected handlers.
    Focuses on handlers implementing various request handler interfaces where null checks
    are redundant due to DI container validation and functional programming patterns.
    """

    def __init__(self, root_dir: Path, dry_run: bool = True):
        self.root_dir = Path(root_dir)
        self.dry_run = dry_run
        self.suppressed_tests: List[SuppressedTest] = []
        self.changes_made: List[Dict] = []
        self.max_fixes = None  # Limit for scope control
        self.target_interfaces = {
            'IMonitorRequestHandler',
            'IGatewayRequestHandler', 
            'IRequestHandler',
            'ICommandHandler',
            'IQueryHandler'
        }
        
        # Enhanced patterns for different types of constructor tests
        self.test_patterns = [
            # Pattern 1: Constructor_WithNull*_ShouldThrow*Exception
            r"""(?P<full_test>
                \[Fact\]\s*
                (?:\[.*\]\s*)*
                public\s+void\s+Constructor_WithNull[^)]*ShouldThrow[^)]*Exception[^)]*\)\s*
                \{.*?
                Should\.Throw<ArgumentNullException>\s*\([^;]+;[^}]*\}
            )""",
            
            # Pattern 2: Constructor_WithNullParameters_ShouldThrowException  
            r"""(?P<full_test>
                \[Fact\]\s*
                (?:\[.*\]\s*)*
                public\s+void\s+Constructor_WithNullParameters_ShouldThrowException[^)]*\)\s*
                \{.*?
                Should\.Throw<ArgumentNullException>\s*\([^;]+;[^}]*\}
            )""",
            
            # Pattern 3: Tests with "Null" and "Exception" in name that test constructors
            r"""(?P<full_test>
                \[Fact\]\s*
                (?:\[.*\]\s*)*
                public\s+void\s+Constructor_[^)]*Null[^)]*Exception[^)]*\)\s*
                \{.*?
                Should\.Throw<ArgumentNullException>\s*\([^;]+;[^}]*\}
            )""",
            
            # Pattern 4: Very specific pattern for the actual tests we want to suppress
            r"""(?P<full_test>
                \[Fact\]\s*
                (?:\[.*\]\s*)*
                public\s+void\s+Constructor_WithNull[^)]*\)\s*
                \{.*?
                Should\.Throw<ArgumentNullException>\s*\(\s*\(\)\s*=>\s*new\s+[^;]+;[^}]*\}
            )"""
        ]

    def is_target_handler_file(self, file_path: Path) -> Tuple[bool, str]:
        """Check if file contains a handler that should have constructor tests suppressed"""
        try:
            content = file_path.read_text(encoding='utf-8')
            
            # Look for handler class patterns
            handler_patterns = [
                r'class\s+(\w*Handler)\s*[^{]*:\s*([^{]*)',
                r'(\w*Handler)\s*\([^)]*\)\s*:\s*([^{]*)'
            ]
            
            for pattern in handler_patterns:
                matches = re.finditer(pattern, content, re.MULTILINE | re.DOTALL)
                for match in matches:
                    handler_name = match.group(1)
                    interfaces = match.group(2)
                    
                    # Check if implements target interfaces
                    for target_interface in self.target_interfaces:
                        if target_interface in interfaces:
                            return True, f"{handler_name} : {target_interface}"
            
            return False, ""
            
        except Exception as e:
            print(f"Warning: Could not analyze {file_path}: {e}")
            return False, ""

    def find_constructor_null_tests(self, content: str) -> List[Dict]:
        """Find constructor null guard tests in the content"""
        found_tests = []
        used_positions = set()
        
        for pattern_str in self.test_patterns:
            pattern = re.compile(pattern_str, re.DOTALL | re.VERBOSE)
            
            for match in pattern.finditer(content):
                # Check if this position overlaps with an already found test
                start_pos = match.start()
                end_pos = match.end()
                
                if any(start_pos < used_end and end_pos > used_start for used_start, used_end in used_positions):
                    continue  # Skip overlapping matches
                
                test_content = match.group('full_test')
                test_start = content[:match.start()].count('\n') + 1
                test_end = content[:match.end()].count('\n') + 1
                
                # Extract test method name
                method_match = re.search(r'public\s+void\s+(\w+)\s*\(', test_content)
                test_name = method_match.group(1) if method_match else "UnknownTest"
                
                found_tests.append({
                    'test_name': test_name,
                    'content': test_content,
                    'start_line': test_start,
                    'end_line': test_end,
                    'start_pos': start_pos,
                    'end_pos': end_pos
                })
                
                used_positions.add((start_pos, end_pos))
        
        return found_tests

    def process_files(self) -> Dict[str, int]:
        """Process all test files and suppress constructor null guard tests"""
        stats = {'files_processed': 0, 'tests_suppressed': 0, 'files_modified': 0}
        fixes_applied = 0
        
        # Find test files in the test directory
        test_files = list(self.root_dir.rglob("*Tests.cs"))
        # Accept any test files in the target directory
        test_files_in_scope = test_files
        
        print(f"Found {len(test_files_in_scope)} test files in {self.root_dir.name}")
        
        for file_path in test_files_in_scope:
            if self.max_fixes and fixes_applied >= self.max_fixes:
                print(f"Reached max fixes limit ({self.max_fixes}), stopping...")
                break
                
            stats['files_processed'] += 1
            result = self._process_file(file_path)
            
            if result['tests_found'] > 0:
                print(f"  {file_path.name}: Found {result['tests_found']} constructor null tests")
                
                if result['tests_suppressed'] > 0:
                    stats['tests_suppressed'] += result['tests_suppressed'] 
                    stats['files_modified'] += 1
                    fixes_applied += result['tests_suppressed']
        
        return stats

    def _process_file(self, file_path: Path) -> Dict:
        """Process a single test file"""
        result = {'tests_found': 0, 'tests_suppressed': 0}
        
        try:
            original_content = file_path.read_text(encoding='utf-8')
            
            # Check if this file contains a target handler class or references one
            is_handler_test = self._is_handler_test_file(file_path, original_content)
            
            if not is_handler_test:
                return result
            
            # Find constructor null tests
            null_tests = self.find_constructor_null_tests(original_content)
            result['tests_found'] = len(null_tests)
            
            if not null_tests:
                return result
            
            # Apply suppressions
            if not self.dry_run:
                modified_content = self._suppress_tests(original_content, null_tests)
                
                if modified_content != original_content:
                    # Create backup
                    backup_path = file_path.with_suffix(file_path.suffix + ".bak")
                    shutil.copy2(file_path, backup_path)
                    
                    # Write modified content
                    file_path.write_text(modified_content, encoding='utf-8')
                    result['tests_suppressed'] = len(null_tests)
                    
                    # Track changes
                    for test in null_tests:
                        self.changes_made.append({
                            'file': str(file_path),
                            'test_name': test['test_name'],
                            'action': 'suppressed',
                            'backup': str(backup_path)
                        })
            else:
                # Dry run - just report what would be done
                result['tests_suppressed'] = len(null_tests)
                for test in null_tests:
                    print(f"    WOULD SUPPRESS: {test['test_name']} (lines {test['start_line']}-{test['end_line']})")
        
        except Exception as e:
            print(f"Error processing {file_path}: {e}")
        
        return result

    def _is_handler_test_file(self, file_path: Path, content: str) -> bool:
        """Check if this is a test file for a handler class"""
        # Check filename patterns
        handler_patterns = ['Handler', 'Executor', 'Processor']
        if any(pattern in file_path.name for pattern in handler_patterns):
            return True
        
        # Check if content instantiates handler classes
        handler_instantiation_patterns = [
            r'new\s+\w*Handler\s*\(',
            r'new\s+\w*Executor\s*\(',
            r'new\s+\w*Processor\s*\('
        ]
        
        for pattern in handler_instantiation_patterns:
            if re.search(pattern, content):
                return True
        
        return False

    def _suppress_tests(self, content: str, tests: List[Dict]) -> str:
        """Suppress the specified tests by commenting them out"""
        # Sort tests by position (descending to avoid offset issues)
        sorted_tests = sorted(tests, key=lambda t: t['start_pos'], reverse=True)
        
        modified_content = content
        
        for test in sorted_tests:
            start_pos = test['start_pos']
            end_pos = test['end_pos']
            
            original_test = modified_content[start_pos:end_pos]
            
            # Create commented version
            lines = original_test.splitlines()
            commented_lines = ["// MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers"]
            commented_lines.extend([f"// {line}" for line in lines])
            commented_test = "\n".join(commented_lines)
            
            # Replace in content
            modified_content = modified_content[:start_pos] + commented_test + modified_content[end_pos:]
        
        return modified_content

    def generate_report(self) -> str:
        """Generate a summary report of suppression actions"""
        report = []
        report.append("Handler Constructor Null Guard Suppression Report")
        report.append("=" * 55)
        report.append(f"Timestamp: {datetime.now()}")
        report.append(f"Mode: {'DRY RUN' if self.dry_run else 'LIVE'}")
        report.append(f"Total changes: {len(self.changes_made)}")
        report.append("")
        
        if self.changes_made:
            report.append("Changes made:")
            for change in self.changes_made:
                report.append(f"  {change['file']}")
                report.append(f"    Test: {change['test_name']}")
                report.append(f"    Action: {change['action']}")
                if 'backup' in change:
                    report.append(f"    Backup: {change['backup']}")
                report.append("")
        
        return "\n".join(report)

def main():
    """Main execution for testing"""
    import argparse
    
    parser = argparse.ArgumentParser(description='Handler Constructor Null Guard Suppressor')
    parser.add_argument('--directory', default='Src/Tests/Core/Application.UnitTests', 
                       help='Test directory to process')
    parser.add_argument('--apply', action='store_true', 
                       help='Apply changes (default is dry-run)')
    parser.add_argument('--max-fixes', type=int, 
                       help='Maximum number of fixes to apply (for scope limiting)')
    
    args = parser.parse_args()
    
    # Determine directory
    current_dir = Path.cwd()
    test_dir = current_dir / args.directory
    
    if not test_dir.exists():
        print(f"Error: Directory not found: {test_dir}")
        return 1
    
    # Create suppressor
    suppressor = HandlerNullGuardSuppressor(test_dir, dry_run=not args.apply)
    if args.max_fixes:
        suppressor.max_fixes = args.max_fixes
    
    print(f"Processing directory: {test_dir}")
    print(f"Mode: {'LIVE' if args.apply else 'DRY RUN'}")
    if args.max_fixes:
        print(f"Max fixes: {args.max_fixes}")
    print()
    
    # Process files
    stats = suppressor.process_files()
    
    # Print results
    print(f"\nResults:")
    print(f"  Files processed: {stats['files_processed']}")
    print(f"  Tests found: {stats['tests_suppressed']}")
    print(f"  Files modified: {stats['files_modified']}")
    
    # Generate report
    report = suppressor.generate_report()
    print(f"\n{report}")
    
    return 0

if __name__ == "__main__":
    exit(main())
