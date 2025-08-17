#!/usr/bin/env python3
"""
FluentAssertions to Shouldly Migration Script
Converts FluentAssertions syntax to Shouldly syntax in C# test files.
Supports dry-run mode, detailed logging, and integration with test cycle framework.
"""

from pathlib import Path
import re
import json
import shutil
from typing import List, Dict, Set, Tuple, Optional
from dataclasses import dataclass
from datetime import datetime

@dataclass
class ConversionResult:
    file_path: str
    pattern_name: str
    original_text: str
    converted_text: str
    line_number: int
    success: bool

class FluentAssertionsToShouldlyConverter:
    """
    Converts FluentAssertions syntax to Shouldly syntax in C# test files.
    Handles various assertion patterns while preserving code formatting and structure.
    """

    def __init__(self, root_dir: Path, dry_run: bool = True):
        self.root_dir = Path(root_dir)
        self.dry_run = dry_run
        self.conversions_made: List[ConversionResult] = []
        self.changes_made: List[Dict] = []
        self.max_fixes = None  # Limit for scope control
        
        # Define conversion patterns with regex and replacement
        self.conversion_patterns = [
            # Basic equality
            {
                'name': 'Be',
                'pattern': r'\.Should\(\)\.Be\(([^)]+)\)',
                'replacement': r'.ShouldBe(\1)',
                'description': 'Basic equality assertion'
            },
            {
                'name': 'NotBe',
                'pattern': r'\.Should\(\)\.NotBe\(([^)]+)\)',
                'replacement': r'.ShouldNotBe(\1)',
                'description': 'Basic inequality assertion'
            },
            
            # Boolean assertions
            {
                'name': 'BeTrue',
                'pattern': r'\.Should\(\)\.BeTrue\(\)',
                'replacement': r'.ShouldBeTrue()',
                'description': 'Boolean true assertion'
            },
            {
                'name': 'BeFalse',
                'pattern': r'\.Should\(\)\.BeFalse\(\)',
                'replacement': r'.ShouldBeFalse()',
                'description': 'Boolean false assertion'
            },
            
            # Null assertions
            {
                'name': 'BeNull',
                'pattern': r'\.Should\(\)\.BeNull\(\)',
                'replacement': r'.ShouldBeNull()',
                'description': 'Null assertion'
            },
            {
                'name': 'NotBeNull',
                'pattern': r'\.Should\(\)\.NotBeNull\(\)',
                'replacement': r'.ShouldNotBeNull()',
                'description': 'Not null assertion'
            },
            
            # String assertions
            {
                'name': 'Contain',
                'pattern': r'\.Should\(\)\.Contain\(([^)]+)\)',
                'replacement': r'.ShouldContain(\1)',
                'description': 'String/collection contains'
            },
            {
                'name': 'NotContain',
                'pattern': r'\.Should\(\)\.NotContain\(([^)]+)\)',
                'replacement': r'.ShouldNotContain(\1)',
                'description': 'String/collection not contains'
            },
            {
                'name': 'StartWith',
                'pattern': r'\.Should\(\)\.StartWith\(([^)]+)\)',
                'replacement': r'.ShouldStartWith(\1)',
                'description': 'String starts with'
            },
            {
                'name': 'EndWith',
                'pattern': r'\.Should\(\)\.EndWith\(([^)]+)\)',
                'replacement': r'.ShouldEndWith(\1)',
                'description': 'String ends with'
            },
            
            # Collection assertions
            {
                'name': 'BeEmpty',
                'pattern': r'\.Should\(\)\.BeEmpty\(\)',
                'replacement': r'.ShouldBeEmpty()',
                'description': 'Empty collection'
            },
            {
                'name': 'NotBeEmpty',
                'pattern': r'\.Should\(\)\.NotBeEmpty\(\)',
                'replacement': r'.ShouldNotBeEmpty()',
                'description': 'Non-empty collection'
            },
            {
                'name': 'HaveCount',
                'pattern': r'\.Should\(\)\.HaveCount\(([^)]+)\)',
                'replacement': r'.Count().ShouldBe(\1)',
                'description': 'Collection count'
            },
            {
                'name': 'HaveCountGreaterThan',
                'pattern': r'\.Should\(\)\.HaveCountGreaterThan\(([^)]+)\)',
                'replacement': r'.Count().ShouldBeGreaterThan(\1)',
                'description': 'Collection count greater than'
            },
            {
                'name': 'ContainKey',
                'pattern': r'\.Should\(\)\.ContainKey\(([^)]+)\)',
                'replacement': r'.ShouldContainKey(\1)',
                'description': 'Dictionary contains key'
            },
            {
                'name': 'OnlyContain',
                'pattern': r'\.Should\(\)\.OnlyContain\(([^)]+)\)',
                'replacement': r'.ShouldAllBe(\1)',
                'description': 'All items match predicate'
            },
            
            # Type assertions
            {
                'name': 'BeOfType',
                'pattern': r'\.Should\(\)\.BeOfType<([^>]+)>\(\)',
                'replacement': r'.ShouldBeOfType<\1>()',
                'description': 'Type assertion'
            },
            {
                'name': 'BeAssignableTo',
                'pattern': r'\.Should\(\)\.BeAssignableTo<([^>]+)>\(\)',
                'replacement': r'.ShouldBeAssignableTo<\1>()',
                'description': 'Type assignability'
            },
            
            # Reference assertions
            {
                'name': 'BeSameAs',
                'pattern': r'\.Should\(\)\.BeSameAs\(([^)]+)\)',
                'replacement': r'.ShouldBeSameAs(\1)',
                'description': 'Reference equality'
            },
            {
                'name': 'NotBeSameAs',
                'pattern': r'\.Should\(\)\.NotBeSameAs\(([^)]+)\)',
                'replacement': r'.ShouldNotBeSameAs(\1)',
                'description': 'Reference inequality'
            },
            
            # Numeric assertions
            {
                'name': 'BeGreaterOrEqualTo',
                'pattern': r'\.Should\(\)\.BeGreaterOrEqualTo\(([^)]+)\)',
                'replacement': r'.ShouldBeGreaterThanOrEqualTo(\1)',
                'description': 'Greater or equal comparison'
            },
            {
                'name': 'BePositive',
                'pattern': r'\.Should\(\)\.BePositive\(\)',
                'replacement': r'.ShouldBeGreaterThan(0)',
                'description': 'Positive number'
            },
            
            # Time assertions
            {
                'name': 'BeCloseTo',
                'pattern': r'\.Should\(\)\.BeCloseTo\(([^,]+),\s*([^)]+)\)',
                'replacement': r'.ShouldBe(\1, tolerance: \2)',
                'description': 'Time proximity'
            },
            
            # String null/empty assertions
            {
                'name': 'NotBeNullOrEmpty',
                'pattern': r'\.Should\(\)\.NotBeNullOrEmpty\(\)',
                'replacement': r'.ShouldNotBeNullOrEmpty()',
                'description': 'String not null or empty'
            },
            {
                'name': 'BeNullOrEmpty',
                'pattern': r'\.Should\(\)\.BeNullOrEmpty\(\)',
                'replacement': r'.ShouldBeNullOrEmpty()',
                'description': 'String null or empty'
            }
        ]
        
        # Exception patterns need special handling
        self.exception_patterns = [
            {
                'name': 'Throw',
                'pattern': r'(\w+)\.Should\(\)\.Throw<([^>]+)>\(\)',
                'replacement': r'Should.Throw<\2>(\1)',
                'description': 'Exception throwing'
            },
            {
                'name': 'NotThrow',
                'pattern': r'(\w+)\.Should\(\)\.NotThrow\(\)',
                'replacement': r'Should.NotThrow(\1)',
                'description': 'No exception throwing'
            }
        ]

    def should_process_file(self, file_path: Path) -> bool:
        """Check if file should be processed"""
        # Skip non-test files
        if not (file_path.name.endswith('Tests.cs') or file_path.name.endswith('Test.cs')):
            return False
        
        # Skip files that don't contain FluentAssertions
        try:
            content = file_path.read_text(encoding='utf-8')
            return '.Should()' in content
        except Exception:
            return False

    def convert_file(self, file_path: Path) -> Dict[str, int]:
        """Convert FluentAssertions to Shouldly in a single file"""
        stats = {'patterns_found': 0, 'conversions_made': 0}
        
        try:
            original_content = file_path.read_text(encoding='utf-8')
            modified_content = original_content
            
            # Track all conversions for this file
            file_conversions = []
            
            # Apply standard conversion patterns
            for pattern in self.conversion_patterns:
                regex = re.compile(pattern['pattern'])
                matches = list(regex.finditer(modified_content))
                
                for match in reversed(matches):  # Process in reverse to maintain positions
                    original_text = match.group(0)
                    converted_text = regex.sub(pattern['replacement'], original_text)
                    
                    # Calculate line number
                    line_number = modified_content[:match.start()].count('\n') + 1
                    
                    file_conversions.append(ConversionResult(
                        file_path=str(file_path),
                        pattern_name=pattern['name'],
                        original_text=original_text,
                        converted_text=converted_text,
                        line_number=line_number,
                        success=True
                    ))
                    
                    stats['patterns_found'] += 1
                    
                    if not self.dry_run:
                        # Apply the conversion
                        modified_content = (
                            modified_content[:match.start()] + 
                            converted_text + 
                            modified_content[match.end():]
                        )
                        stats['conversions_made'] += 1
            
            # Apply exception patterns (special handling)
            for pattern in self.exception_patterns:
                regex = re.compile(pattern['pattern'])
                matches = list(regex.finditer(modified_content))
                
                for match in reversed(matches):
                    original_text = match.group(0)
                    converted_text = regex.sub(pattern['replacement'], original_text)
                    
                    line_number = modified_content[:match.start()].count('\n') + 1
                    
                    file_conversions.append(ConversionResult(
                        file_path=str(file_path),
                        pattern_name=pattern['name'],
                        original_text=original_text,
                        converted_text=converted_text,
                        line_number=line_number,
                        success=True
                    ))
                    
                    stats['patterns_found'] += 1
                    
                    if not self.dry_run:
                        modified_content = (
                            modified_content[:match.start()] + 
                            converted_text + 
                            modified_content[match.end():]
                        )
                        stats['conversions_made'] += 1
            
            # Update using statements
            if stats['patterns_found'] > 0:
                modified_content = self._update_using_statements(modified_content)
            
            # Save changes if not dry run and changes were made
            if not self.dry_run and modified_content != original_content:
                # Create backup
                backup_path = file_path.with_suffix(file_path.suffix + '.bak')
                shutil.copy2(file_path, backup_path)
                
                # Write modified content
                file_path.write_text(modified_content, encoding='utf-8')
                
                # Track changes
                self.changes_made.append({
                    'file': str(file_path),
                    'conversions': len(file_conversions),
                    'backup': str(backup_path)
                })
            
            # Store all conversions
            self.conversions_made.extend(file_conversions)
            
        except Exception as e:
            print(f"Error processing {file_path}: {e}")
        
        return stats

    def _update_using_statements(self, content: str) -> str:
        """Update using statements: remove FluentAssertions, add Shouldly"""
        lines = content.splitlines()
        modified_lines = []
        shouldly_added = False
        
        for line in lines:
            # Skip FluentAssertions using statements
            if 'using FluentAssertions' in line:
                continue
            
            # Add Shouldly using if not already present
            if 'using Shouldly;' in line:
                shouldly_added = True
            
            # Add Shouldly after other using statements
            if line.strip() == '' and not shouldly_added and any('using ' in l for l in modified_lines):
                # Check if we're at the end of using statements
                if modified_lines and 'using ' in modified_lines[-1]:
                    modified_lines.append('using Shouldly;')
                    shouldly_added = True
            
            modified_lines.append(line)
        
        # If Shouldly wasn't added and there are using statements, add it
        if not shouldly_added:
            for i, line in enumerate(modified_lines):
                if 'using ' in line and 'namespace' not in line:
                    # Find the last using statement
                    j = i
                    while j < len(modified_lines) - 1 and 'using ' in modified_lines[j + 1]:
                        j += 1
                    modified_lines.insert(j + 1, 'using Shouldly;')
                    break
        
        return '\n'.join(modified_lines)

    def process_directory(self, directory: Path = None) -> Dict[str, int]:
        """Process all test files in the directory"""
        if directory is None:
            directory = self.root_dir
            
        stats = {'files_processed': 0, 'total_patterns_found': 0, 'total_conversions_made': 0, 'files_modified': 0}
        
        # Find all test files
        test_files = list(directory.rglob("*Tests.cs")) + list(directory.rglob("*Test.cs"))
        eligible_files = [f for f in test_files if self.should_process_file(f)]
        
        print(f"Found {len(eligible_files)} test files with FluentAssertions")
        
        for file_path in eligible_files:
            if self.max_fixes and stats['total_conversions_made'] >= self.max_fixes:
                print(f"Reached max fixes limit ({self.max_fixes}), stopping...")
                break
            
            file_stats = self.convert_file(file_path)
            
            stats['files_processed'] += 1
            stats['total_patterns_found'] += file_stats['patterns_found']
            stats['total_conversions_made'] += file_stats['conversions_made']
            
            if file_stats['conversions_made'] > 0:
                stats['files_modified'] += 1
            
            if file_stats['patterns_found'] > 0:
                print(f"  {file_path.name}: {file_stats['patterns_found']} patterns found, {file_stats['conversions_made']} converted")
        
        return stats

    def generate_report(self) -> str:
        """Generate a detailed conversion report"""
        report = []
        report.append("FluentAssertions to Shouldly Conversion Report")
        report.append("=" * 50)
        report.append(f"Timestamp: {datetime.now()}")
        report.append(f"Mode: {'DRY RUN' if self.dry_run else 'LIVE'}")
        report.append(f"Total conversions: {len(self.conversions_made)}")
        report.append("")
        
        # Group conversions by pattern
        pattern_counts = {}
        for conv in self.conversions_made:
            pattern_counts[conv.pattern_name] = pattern_counts.get(conv.pattern_name, 0) + 1
        
        report.append("Conversions by pattern:")
        for pattern, count in sorted(pattern_counts.items(), key=lambda x: x[1], reverse=True):
            report.append(f"  {pattern}: {count}")
        report.append("")
        
        # Show sample conversions
        if self.conversions_made:
            report.append("Sample conversions:")
            for conv in self.conversions_made[:5]:  # Show first 5
                report.append(f"  File: {Path(conv.file_path).name}")
                report.append(f"  Line {conv.line_number}:")
                report.append(f"    From: {conv.original_text}")
                report.append(f"    To:   {conv.converted_text}")
                report.append("")
        
        return "\n".join(report)

def main():
    """Main execution for testing"""
    import argparse
    
    parser = argparse.ArgumentParser(description='FluentAssertions to Shouldly Converter')
    parser.add_argument('--directory', default='code/test', 
                       help='Test directory to process')
    parser.add_argument('--apply', action='store_true', 
                       help='Apply changes (default is dry-run)')
    parser.add_argument('--max-fixes', type=int, 
                       help='Maximum number of fixes to apply (for scope limiting)')
    parser.add_argument('--project', 
                       help='Specific project to process (e.g., RefactorMCP.Core.Tests)')
    
    args = parser.parse_args()
    
    # Determine directory
    current_dir = Path.cwd()
    if args.project:
        test_dir = current_dir / args.directory / args.project
    else:
        test_dir = current_dir / args.directory
    
    if not test_dir.exists():
        print(f"Error: Directory not found: {test_dir}")
        return 1
    
    # Create converter
    converter = FluentAssertionsToShouldlyConverter(test_dir, dry_run=not args.apply)
    if args.max_fixes:
        converter.max_fixes = args.max_fixes
    
    print(f"Processing directory: {test_dir}")
    print(f"Mode: {'LIVE' if args.apply else 'DRY RUN'}")
    if args.max_fixes:
        print(f"Max fixes: {args.max_fixes}")
    print()
    
    # Process files
    stats = converter.process_directory()
    
    # Print results
    print(f"\nResults:")
    print(f"  Files processed: {stats['files_processed']}")
    print(f"  Patterns found: {stats['total_patterns_found']}")
    print(f"  Conversions made: {stats['total_conversions_made']}")
    print(f"  Files modified: {stats['files_modified']}")
    
    # Generate report
    report = converter.generate_report()
    print(f"\n{report}")
    
    # Save report to file
    report_path = test_dir / f"shouldly_conversion_report_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt"
    report_path.write_text(report)
    print(f"\nReport saved to: {report_path}")
    
    return 0

if __name__ == "__main__":
    exit(main())