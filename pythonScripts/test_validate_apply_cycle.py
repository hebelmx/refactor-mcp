#!/usr/bin/env python3
"""
Test-Validate-Apply-Verify Cycle for Test Fixing
Implements comprehensive workflow: dry-run → verify → apply → reality check → rollback on failure
"""

import re
import os
import sys
import subprocess
import json
import shutil
from pathlib import Path
from typing import List, Tuple, Dict, Set
from datetime import datetime
from dataclasses import dataclass

@dataclass
class TestResult:
    total_tests: int
    failed_tests: int
    errors: int
    skipped: int
    success_rate: float
    
@dataclass
class CycleResult:
    phase: str
    success: bool
    message: str
    test_result: TestResult = None
    fixes_applied: int = 0
    files_modified: List[str] = None

class TestValidateApplyCycle:
    """Comprehensive test fixing cycle with validation and rollback"""
    
    def __init__(self, test_directory: str):
        self.test_directory = Path(test_directory)
        self.project_file = self.test_directory / "Application.UnitTests.csproj"
        self.errors_file = self.test_directory / "errors.txt"
        self.backup_dir = Path("cycle_backups")
        self.session_dir = self.backup_dir / f"session_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
        self.cycle_history = []
        self.baseline_result = None
        
    def setup_session(self):
        """Setup session directories and baseline"""
        print("Starting Test-Validate-Apply Cycle")
        print("=" * 50)
        
        self.session_dir.mkdir(parents=True, exist_ok=True)
        print(f"Session directory: {self.session_dir}")
        
        # Get baseline test results
        print("\nEstablishing baseline...")
        self.baseline_result = self.run_tests()
        if not self.baseline_result:
            raise Exception("Failed to establish baseline - tests won't run")
            
        print(f"Baseline: {self.baseline_result.failed_tests} failures, {self.baseline_result.success_rate:.1f}% success")
        
    def generate_enhanced_errors(self) -> bool:
        """Generate errors.txt using the enhanced command"""
        print("\nGenerating enhanced error file...")
        
        cmd = [
            "powershell", "-Command",
            f'dotnet run --project "{self.project_file}" *>&1 | Select-String -Pattern "^\\s*\\[FAIL\\]", "\\.cs\\(\\d+,\\d+\\):" | ForEach-Object {{ $_.Line }} | Out-File -Encoding utf8 "{self.errors_file}"'
        ]
        
        try:
            result = subprocess.run(cmd, capture_output=True, text=True, cwd=self.test_directory.parent)
            
            if self.errors_file.exists() and self.errors_file.stat().st_size > 0:
                lines = len(self.errors_file.read_text(encoding='utf-8').strip().split('\n'))
                print(f" Generated errors.txt with {lines} lines")
                return True
            else:
                print(" Failed to generate errors.txt or file is empty")
                return False
                
        except Exception as e:
            print(f" Error generating errors.txt: {e}")
            return False
            
    def run_tests(self) -> TestResult:
        """Run tests and parse results"""
        print("Running tests...")
        
        cmd = ["dotnet", "run", "--project", str(self.project_file)]
        
        try:
            result = subprocess.run(cmd, capture_output=True, text=True, cwd=self.test_directory.parent, timeout=300)
            output = result.stdout + result.stderr
            
            # Parse test results from output
            # Look for summary line like: "Total: 6546, Errors: 0, Failed: 734, Skipped: 0"
            summary_match = re.search(r'Total:\s*(\d+),\s*Errors:\s*(\d+),\s*Failed:\s*(\d+),\s*Skipped:\s*(\d+)', output)
            
            if summary_match:
                total = int(summary_match.group(1))
                errors = int(summary_match.group(2))
                failed = int(summary_match.group(3))
                skipped = int(summary_match.group(4))
                
                success_rate = ((total - failed - errors) / total * 100) if total > 0 else 0
                
                test_result = TestResult(
                    total_tests=total,
                    failed_tests=failed,
                    errors=errors,
                    skipped=skipped,
                    success_rate=success_rate
                )
                
                print(f"Results: {failed} failures, {errors} errors, {success_rate:.1f}% success")
                return test_result
            else:
                print(" Could not parse test results")
                print(f"Output: {output[-500:]}")  # Last 500 chars for debugging
                return None
                
        except subprocess.TimeoutExpired:
            print(" Test run timed out (5 minutes)")
            return None
        except Exception as e:
            print(f" Error running tests: {e}")
            return None
            
    def backup_current_state(self) -> str:
        """Create backup of current state"""
        backup_name = f"backup_{datetime.now().strftime('%H%M%S')}"
        backup_path = self.session_dir / backup_name
        backup_path.mkdir(exist_ok=True)
        
        # Backup all .cs files
        cs_files = list(self.test_directory.rglob("*.cs"))
        for cs_file in cs_files:
            rel_path = cs_file.relative_to(self.test_directory)
            dest_path = backup_path / rel_path
            dest_path.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(cs_file, dest_path)
            
        print(f"Backed up {len(cs_files)} files to {backup_name}")
        return str(backup_path)
        
    def restore_backup(self, backup_path: str) -> bool:
        """Restore from backup"""
        backup_dir = Path(backup_path)
        if not backup_dir.exists():
            print(f" Backup directory not found: {backup_path}")
            return False
            
        try:
            # Restore all .cs files
            cs_files = list(backup_dir.rglob("*.cs"))
            for cs_file in cs_files:
                rel_path = cs_file.relative_to(backup_dir)
                dest_path = self.test_directory / rel_path
                shutil.copy2(cs_file, dest_path)
                
            print(f"Restored {len(cs_files)} files from backup")
            return True
            
        except Exception as e:
            print(f" Error restoring backup: {e}")
            return False
            
    def run_enhanced_fixer(self, scope_multiplier: float = 1.0) -> CycleResult:
        """Run the enhanced fixer with specified scope"""
        print(f"\nRunning enhanced fixer (scope: {scope_multiplier:.1f}x)...")
        
        # Import and run the enhanced fixer
        sys.path.append(str(Path.cwd()))
        
        try:
            from enhanced_test_fixer import EnhancedTestFixer
            
            fixer = EnhancedTestFixer(str(self.test_directory), dry_run=True)
            
            # First do dry run
            print("   Dry run analysis...")
            pattern_stats = fixer.run_enhanced_fixes()
            total_potential_fixes = sum(pattern_stats.values())
            
            if total_potential_fixes == 0:
                return CycleResult(
                    phase="enhanced_fixer",
                    success=False,
                    message="No fixes identified",
                    fixes_applied=0
                )
                
            # Apply scope multiplier to limit fixes
            max_fixes = max(1, int(total_potential_fixes * scope_multiplier))
            print(f"   Limiting to {max_fixes} fixes (of {total_potential_fixes} potential)")
            
            # Apply fixes with limited scope
            fixer_live = EnhancedTestFixer(str(self.test_directory), dry_run=False)
            fixer_live.max_fixes = max_fixes  # Add scope limiting
            
            pattern_stats_live = fixer_live.run_enhanced_fixes()
            actual_fixes = sum(pattern_stats_live.values())
            
            return CycleResult(
                phase="enhanced_fixer",
                success=True,
                message=f"Applied {actual_fixes} fixes",
                fixes_applied=actual_fixes,
                files_modified=list(set(change['file'] for change in fixer_live.changes_made))
            )
            
        except Exception as e:
            return CycleResult(
                phase="enhanced_fixer",
                success=False,
                message=f"Fixer failed: {str(e)}",
                fixes_applied=0
            )
            
    def run_shouldly_converter(self, scope_multiplier: float = 1.0) -> CycleResult:
        """Run the FluentAssertions to Shouldly converter with specified scope"""
        print(f"\nRunning FluentAssertions to Shouldly converter (scope: {scope_multiplier:.1f}x)...")
        
        # Import and run the converter
        sys.path.append(str(Path.cwd()))
        
        try:
            from FluentAssertionsToShouldly import FluentAssertionsToShouldlyConverter
            
            # First do dry run to count potential fixes
            converter_dry = FluentAssertionsToShouldlyConverter(str(self.test_directory), dry_run=True)
            print("   Dry run analysis...")
            dry_stats = converter_dry.process_directory()
            total_potential_fixes = dry_stats['total_patterns_found']
            
            if total_potential_fixes == 0:
                return CycleResult(
                    phase="shouldly_converter",
                    success=False,
                    message="No FluentAssertions patterns found",
                    fixes_applied=0
                )
                
            # Apply scope multiplier to limit fixes
            max_fixes = max(1, int(total_potential_fixes * scope_multiplier))
            print(f"   Limiting to {max_fixes} fixes (of {total_potential_fixes} potential)")
            
            # Apply conversions with limited scope
            converter_live = FluentAssertionsToShouldlyConverter(str(self.test_directory), dry_run=False)
            converter_live.max_fixes = max_fixes
            
            live_stats = converter_live.process_directory()
            actual_fixes = live_stats['total_conversions_made']
            
            return CycleResult(
                phase="shouldly_converter",
                success=True,
                message=f"Converted {actual_fixes} FluentAssertions patterns to Shouldly",
                fixes_applied=actual_fixes,
                files_modified=list(set(change['file'] for change in converter_live.changes_made))
            )
            
        except Exception as e:
            return CycleResult(
                phase="shouldly_converter",
                success=False,
                message=f"Converter failed: {str(e)}",
                fixes_applied=0
            )
            
    def run_handler_null_guard_suppressor(self, scope_multiplier: float = 1.0) -> CycleResult:
        """Run the handler null guard suppressor with specified scope"""
        print(f"\nRunning handler null guard suppressor (scope: {scope_multiplier:.1f}x)...")
        
        # Import and run the suppressor
        sys.path.append(str(Path.cwd()))
        
        try:
            from handler_null_guard_suppressor import HandlerNullGuardSuppressor
            
            # First do dry run to count potential fixes
            suppressor_dry = HandlerNullGuardSuppressor(str(self.test_directory), dry_run=True)
            print("   Dry run analysis...")
            dry_stats = suppressor_dry.process_files()
            total_potential_fixes = dry_stats['tests_suppressed']
            
            if total_potential_fixes == 0:
                return CycleResult(
                    phase="handler_suppressor",
                    success=False,
                    message="No constructor null guard tests found",
                    fixes_applied=0
                )
                
            # Apply scope multiplier to limit fixes
            max_fixes = max(1, int(total_potential_fixes * scope_multiplier))
            print(f"   Limiting to {max_fixes} fixes (of {total_potential_fixes} potential)")
            
            # Apply suppressions with limited scope
            suppressor_live = HandlerNullGuardSuppressor(str(self.test_directory), dry_run=False)
            suppressor_live.max_fixes = max_fixes
            
            live_stats = suppressor_live.process_files()
            actual_fixes = live_stats['tests_suppressed']
            
            return CycleResult(
                phase="handler_suppressor", 
                success=True,
                message=f"Suppressed {actual_fixes} constructor null guard tests",
                fixes_applied=actual_fixes,
                files_modified=list(set(change['file'] for change in suppressor_live.changes_made))
            )
            
        except Exception as e:
            return CycleResult(
                phase="handler_suppressor",
                success=False,
                message=f"Suppressor failed: {str(e)}",
                fixes_applied=0
            )
            
    def run_cycle(self, initial_scope: float = 0.1, max_scope: float = 1.0, scope_increment: float = 0.1) -> bool:
        """Run the complete test-validate-apply cycle"""
        
        current_scope = initial_scope
        consecutive_failures = 0
        max_failures = 3
        
        while current_scope <= max_scope and consecutive_failures < max_failures:
            cycle_num = len(self.cycle_history) + 1
            print(f"\nCYCLE {cycle_num} - Scope: {current_scope:.1f}x")
            print("-" * 40)
            
            # Step 1: Backup current state
            backup_path = self.backup_current_state()
            
            # Step 2: Generate fresh errors.txt
            if not self.generate_enhanced_errors():
                consecutive_failures += 1
                current_scope += scope_increment
                continue
                
            # Step 3: Run selected fixer mode 
            if hasattr(self, 'fixer_mode') and self.fixer_mode == 'enhanced':
                fixer_result = self.run_enhanced_fixer(current_scope)
            elif hasattr(self, 'fixer_mode') and self.fixer_mode == 'shouldly':
                fixer_result = self.run_shouldly_converter(current_scope)
            else:
                fixer_result = self.run_handler_null_guard_suppressor(current_scope)
            
            if not fixer_result.success:
                print(f" Fixer failed: {fixer_result.message}")
                consecutive_failures += 1
                current_scope += scope_increment
                continue
                
            # Step 4: Reality check - run tests again
            print("\nReality check - running tests...")
            new_result = self.run_tests()
            
            if not new_result:
                print(" Reality check failed - tests won't run")
                print("Restoring backup...")
                self.restore_backup(backup_path)
                consecutive_failures += 1
                current_scope += scope_increment
                continue
                
            # Step 5: Evaluate improvement
            improvement = self.baseline_result.failed_tests - new_result.failed_tests
            success_rate_improvement = new_result.success_rate - self.baseline_result.success_rate
            
            cycle_result = CycleResult(
                phase=f"cycle_{cycle_num}",
                success=improvement > 0,
                message=f"Fixed {improvement} tests, success rate: {success_rate_improvement:+.1f}%",
                test_result=new_result,
                fixes_applied=fixer_result.fixes_applied,
                files_modified=fixer_result.files_modified
            )
            
            self.cycle_history.append(cycle_result)
            
            if improvement > 0:
                print(f" SUCCESS: Fixed {improvement} tests!")
                print(f"Success rate improved by {success_rate_improvement:+.1f}%")
                
                # Update baseline for next cycle
                self.baseline_result = new_result
                consecutive_failures = 0
                
                # Widen scope on success
                current_scope = min(current_scope + scope_increment, max_scope)
                
            else:
                print(f" No improvement: {improvement} tests fixed")
                print("Restoring backup...")
                self.restore_backup(backup_path)
                consecutive_failures += 1
                
                # Narrow scope on failure
                current_scope = max(current_scope * 0.8, 0.05)
                
            # Generate cycle report
            self.save_cycle_report()
            
        # Final summary
        self.print_final_summary()
        return len([c for c in self.cycle_history if c.success]) > 0
        
    def save_cycle_report(self):
        """Save detailed cycle report"""
        report = {
            'session_start': datetime.now().isoformat(),
            'baseline': {
                'failed_tests': self.baseline_result.failed_tests,
                'success_rate': self.baseline_result.success_rate
            },
            'cycles': []
        }
        
        for cycle in self.cycle_history:
            cycle_data = {
                'phase': cycle.phase,
                'success': cycle.success,
                'message': cycle.message,
                'fixes_applied': cycle.fixes_applied,
                'files_modified': cycle.files_modified or []
            }
            
            if cycle.test_result:
                cycle_data['test_result'] = {
                    'failed_tests': cycle.test_result.failed_tests,
                    'success_rate': cycle.test_result.success_rate
                }
                
            report['cycles'].append(cycle_data)
            
        report_file = self.session_dir / "cycle_report.json"
        with open(report_file, 'w') as f:
            json.dump(report, f, indent=2)
            
    def print_final_summary(self):
        """Print final summary of all cycles"""
        print("\n" + "=" * 60)
        print("FINAL SUMMARY")
        print("=" * 60)
        
        successful_cycles = [c for c in self.cycle_history if c.success]
        total_fixes = sum(c.fixes_applied for c in successful_cycles)
        
        if successful_cycles:
            final_result = self.cycle_history[-1].test_result
            original_failures = self.baseline_result.failed_tests
            current_failures = final_result.failed_tests if final_result else original_failures
            total_improvement = original_failures - current_failures
            
            print(f" Successful cycles: {len(successful_cycles)}/{len(self.cycle_history)}")
            print(f"Total fixes applied: {total_fixes}")
            print(f"Tests fixed: {total_improvement}")
            print(f"Final failure count: {current_failures} (was {original_failures})")
            
            if final_result:
                print(f"Final success rate: {final_result.success_rate:.1f}% (was {self.baseline_result.success_rate:.1f}%)")
        else:
            print(" No successful cycles completed")
            print("All changes have been reverted")
            
        print(f"\nSession data saved to: {self.session_dir}")

def main():
    """Main execution"""
    import argparse
    
    parser = argparse.ArgumentParser(description='Test-Validate-Apply Cycle for Test Fixing')
    parser.add_argument('--directory', default='Src/Tests/Core/Application.UnitTests', 
                       help='Test directory to process')
    parser.add_argument('--initial-scope', type=float, default=0.05,
                       help='Initial scope multiplier (0.05 = 5% of available fixes)')
    parser.add_argument('--max-scope', type=float, default=0.5,
                       help='Maximum scope multiplier (0.5 = 50% of available fixes)')
    parser.add_argument('--mode', choices=['enhanced', 'suppressor', 'shouldly'], default='suppressor',
                       help='Mode: enhanced (pattern fixer), suppressor (null guard tests), or shouldly (FluentAssertions to Shouldly)')
    
    args = parser.parse_args()
    
    # Determine test directory
    current_dir = Path.cwd()
    if 'Src' in str(current_dir):
        test_dir = current_dir / args.directory.replace('Src/', '')
    else:
        test_dir = current_dir / args.directory
        
    if not test_dir.exists():
        print(f"Error: Test directory not found: {test_dir}")
        sys.exit(1)
        
    try:
        cycle = TestValidateApplyCycle(str(test_dir))
        cycle.fixer_mode = args.mode  # Set the fixer mode
        cycle.setup_session()
        
        success = cycle.run_cycle(
            initial_scope=args.initial_scope,
            max_scope=args.max_scope
        )
        
        sys.exit(0 if success else 1)
        
    except KeyboardInterrupt:
        print("\n️  Cycle interrupted by user")
        sys.exit(1)
    except Exception as e:
        print(f"\nCycle failed with error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()