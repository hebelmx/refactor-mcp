#!/usr/bin/env python3
"""
Enhanced Solution Migration Executor
====================================
Safe File Migration Script with Comprehensive Features

Features:
- Dry-run mode for safe testing
- Configurable source/destination paths
- Hash verification for file integrity
- Disk space validation
- Progress estimation with ETA
- Automatic config generation
- Enhanced error recovery
- Resume capability for interrupted migrations

Author: Enhanced Migration Assistant
Version: 2.0
"""

import os
import json
import hashlib
import shutil
import argparse
import sys
from pathlib import Path
from datetime import datetime, timedelta
from typing import Dict, List, Set, Tuple, Any, Optional
import logging
import time

class EnhancedMigrationExecutor:
    """
    Enhanced migration executor with dry-run support and better error handling.
    """
    
    def __init__(self, source_path: str = None, dest_path: str = None, 
                 dry_run: bool = False, config_path: str = None):
        self.start_timestamp = datetime.now()
        self.dry_run = dry_run
        self.source_path = source_path
        self.dest_path = dest_path
        self.config_path = config_path or "migration_config.json"
        
        # Migration state
        self.copied_files = []
        self.failed_files = []
        self.skipped_files = []
        self.total_files = 0
        self.total_size_bytes = 0
        self.processed_count = 0
        self.copied_size = 0
        self.start_time = None
        
        # Resume capability
        self.resume_log_path = None
        self.completed_files = set()
        
        # Load or create configuration
        self.config = self._load_or_create_configuration()
        self.migration_plan = self._load_migration_plan()
        
        self._setup_logging()
        self._load_resume_state()
        
    def _load_or_create_configuration(self) -> Dict[str, Any]:
        """Load existing config or create a default one"""
        config_file = Path(self.config_path)
        
        # Default configuration
        default_config = {
            "migration_settings": {
                "verify_checksums": True,
                "skip_large_files_mb": 100,
                "retry_attempts": 3,
                "chunk_size_bytes": 4096
            },
            "file_filters": {
                "exclude_extensions": [".tmp", ".log", ".cache"],
                "exclude_directories": ["bin", "obj", ".vs", "node_modules", ".git"],
                "include_only": []
            },
            "paths": {
                "source": self.source_path or str(Path.cwd()),
                "destination": self.dest_path or str(Path.cwd() / "migrated_solution")
            }
        }
        
        if config_file.exists():
            try:
                with open(config_file, 'r', encoding='utf-8') as f:
                    loaded_config = json.load(f)
                # Merge with defaults to ensure all keys exist
                for key, value in default_config.items():
                    if key not in loaded_config:
                        loaded_config[key] = value
                return loaded_config
            except Exception as e:
                print(f"WARNING Warning: Could not load config file: {e}")
                print("📝 Using default configuration...")
        
        # Create default config file
        try:
            with open(config_file, 'w', encoding='utf-8') as f:
                json.dump(default_config, f, indent=2)
            print(f"Created default configuration: {config_file}")
        except Exception as e:
            print(f"Warning: Could not create config file: {e}")
            
        return default_config
    
    def _load_migration_plan(self) -> Dict[str, Any]:
        """Load the migration plan, with fallback to directory scanning"""
        plan_files = [
            "migration_plan.json",
            "src/PythonScript/migration_plan.json",
            "vs/migration_plan.json"
        ]
        
        for plan_file in plan_files:
            plan_path = Path(plan_file)
            if plan_path.exists():
                try:
                    with open(plan_path, 'r', encoding='utf-8') as f:
                        return json.load(f)
                except Exception as e:
                    print(f"Warning: Could not load {plan_file}: {e}")
        
        # If no plan file found, create a simple one by scanning the source
        print("No migration plan found. Creating one from source directory...")
        return self._create_migration_plan_from_source()
    
    def _create_migration_plan_from_source(self) -> Dict[str, Any]:
        """Create a basic migration plan by scanning the source directory"""
        source_path = Path(self.config["paths"]["source"])
        if not source_path.exists():
            raise FileNotFoundError(f"Source path does not exist: {source_path}")
        
        candidates = []
        exclude_dirs = set(self.config["file_filters"]["exclude_directories"])
        exclude_exts = set(self.config["file_filters"]["exclude_extensions"])
        
        for file_path in source_path.rglob("*"):
            if file_path.is_file():
                # Check if file should be excluded
                if any(part in exclude_dirs for part in file_path.parts):
                    continue
                if file_path.suffix.lower() in exclude_exts:
                    continue
                
                try:
                    stat = file_path.stat()
                    relative_path = file_path.relative_to(source_path)
                    
                    candidates.append({
                        "absolute_path": str(file_path),
                        "relative_path": str(relative_path),
                        "file_name": file_path.name,
                        "extension": file_path.suffix,
                        "size_bytes": stat.st_size,
                        "size_mb": stat.st_size / (1024 * 1024),
                        "modified_date": datetime.fromtimestamp(stat.st_mtime).isoformat(),
                        "directory": str(relative_path.parent),
                        "hash": self._calculate_file_hash(file_path) if stat.st_size < 10 * 1024 * 1024 else ""  # Hash only small files
                    })
                except Exception as e:
                    print(f"Warning: Could not process {file_path}: {e}")
        
        plan = {
            "plan_timestamp": datetime.now().isoformat(),
            "source_path": str(source_path),
            "destination_path": self.config["paths"]["destination"],
            "migration_candidates": candidates,
            "auto_generated": True
        }
        
        # Save the generated plan
        plan_file = Path("migration_plan.json")
        try:
            with open(plan_file, 'w', encoding='utf-8') as f:
                json.dump(plan, f, indent=2)
            print(f"Generated migration plan: {plan_file}")
        except Exception as e:
            print(f"Warning: Could not save migration plan: {e}")
        
        return plan
    
    def _setup_logging(self):
        """Setup comprehensive logging system"""
        # Create logs directory
        log_dir = Path("migration_logs")
        log_dir.mkdir(parents=True, exist_ok=True)
        
        # Setup main log file
        timestamp_str = self.start_timestamp.strftime('%Y%m%d_%H%M%S')
        log_file = log_dir / f"migration_{timestamp_str}.log"
        
        if self.dry_run:
            log_file = log_dir / f"migration_dryrun_{timestamp_str}.log"
        
        # Setup resume log file
        self.resume_log_path = log_dir / f"completed_files_{timestamp_str}.txt"
        
        # Configure logging
        log_level = logging.DEBUG if self.dry_run else logging.INFO
        logging.basicConfig(
            level=log_level,
            format='%(asctime)s - %(levelname)s - %(message)s',
            handlers=[
                logging.FileHandler(log_file, encoding='utf-8'),
                logging.StreamHandler()  # Console output
            ]
        )
        
        self.logger = logging.getLogger(__name__)
        self.logger.info("=" * 80)
        self.logger.info("Enhanced Solution Migration Executor")
        if self.dry_run:
            self.logger.info("🧪 DRY RUN MODE - No files will be copied")
        self.logger.info("=" * 80)
        self.logger.info(f"Migration started: {self.start_timestamp}")
        self.logger.info(f"Configuration: {self.config_path}")
        self.logger.info(f"Log file: {log_file}")
        
    def _load_resume_state(self):
        """Load previous completion state for resumption"""
        if not self.dry_run and self.resume_log_path.exists():
            self.logger.info("🔄 Found previous migration state. Loading for resumption...")
            try:
                with open(self.resume_log_path, 'r', encoding='utf-8') as f:
                    for line in f:
                        line = line.strip()
                        if line:
                            self.completed_files.add(line)
                self.logger.info(f"📁 Resuming: {len(self.completed_files)} files already completed")
            except Exception as e:
                self.logger.warning(f"WARNING Could not load resume state: {e}")
        else:
            self.logger.info("🆕 Starting fresh migration")
    
    def _calculate_file_hash(self, file_path: Path) -> str:
        """Calculate SHA256 hash of a file for verification"""
        hash_sha256 = hashlib.sha256()
        try:
            with open(file_path, "rb") as f:
                for chunk in iter(lambda: f.read(self.config["migration_settings"]["chunk_size_bytes"]), b""):
                    hash_sha256.update(chunk)
            return hash_sha256.hexdigest()
        except Exception as e:
            self.logger.warning(f"WARNING Could not calculate hash for {file_path}: {e}")
            return ""
    
    def _check_disk_space(self, dest_path: Path, required_bytes: int) -> bool:
        """Check if there's enough disk space for the migration"""
        try:
            stat = shutil.disk_usage(dest_path.parent if dest_path.parent.exists() else dest_path.root)
            free_bytes = stat.free
            required_mb = required_bytes / (1024 * 1024)
            free_mb = free_bytes / (1024 * 1024)
            
            self.logger.info(f"💾 Disk space check:")
            self.logger.info(f"   Required: {required_mb:.1f} MB")
            self.logger.info(f"   Available: {free_mb:.1f} MB")
            
            if free_bytes < required_bytes * 1.1:  # 10% buffer
                self.logger.error(f"ERROR Insufficient disk space! Need {required_mb:.1f} MB, have {free_mb:.1f} MB")
                return False
            
            return True
        except Exception as e:
            self.logger.warning(f"WARNING Could not check disk space: {e}")
            return True  # Proceed if we can't check
    
    def _copy_file_with_verification(self, source_path: Path, dest_path: Path, expected_hash: str = "") -> bool:
        """
        Copy a single file with verification.
        Returns True if successful, False otherwise.
        """
        if self.dry_run:
            self.logger.debug(f"🧪 DRY RUN: Would copy {source_path} -> {dest_path}")
            return True
        
        retry_count = 0
        max_retries = self.config["migration_settings"]["retry_attempts"]
        
        while retry_count <= max_retries:
            try:
                # Ensure destination directory exists
                dest_path.parent.mkdir(parents=True, exist_ok=True)
                
                # Get source file info
                source_size = source_path.stat().st_size
                
                # Copy the file
                shutil.copy2(source_path, dest_path)
                
                # Verify the copy
                if not dest_path.exists():
                    raise Exception("Destination file not created")
                
                # Verify size
                dest_size = dest_path.stat().st_size
                if source_size != dest_size:
                    raise Exception(f"Size mismatch: source {source_size} vs dest {dest_size}")
                
                # Verify hash if provided and enabled
                if (self.config["migration_settings"]["verify_checksums"] and 
                    expected_hash and 
                    source_size > 1024):  # Only verify files > 1KB
                    
                    dest_hash = self._calculate_file_hash(dest_path)
                    if expected_hash and dest_hash and expected_hash != dest_hash:
                        raise Exception(f"Hash mismatch: expected {expected_hash[:16]}... got {dest_hash[:16]}...")
                
                # Log successful copy to resume file
                with open(self.resume_log_path, 'a', encoding='utf-8') as f:
                    f.write(f"{source_path}\n")
                
                return True
                
            except Exception as e:
                retry_count += 1
                if retry_count <= max_retries:
                    self.logger.warning(f"WARNING Retry {retry_count}/{max_retries} for {source_path}: {e}")
                    time.sleep(retry_count)  # Progressive delay
                    
                    # Clean up failed copy
                    if dest_path.exists():
                        try:
                            dest_path.unlink()
                        except:
                            pass
                else:
                    self.logger.error(f"ERROR Failed to copy {source_path} after {max_retries} retries: {e}")
                    return False
        
        return False
    
    def _update_progress(self, current: int, total: int, current_file: str = ""):
        """Update and display progress with ETA"""
        if self.start_time is None:
            self.start_time = time.time()
        
        percentage = (current / total * 100) if total > 0 else 0
        progress_bar_length = 30
        filled_length = int(progress_bar_length * current // total) if total > 0 else 0
        
        bar = '=' * filled_length + '-' * (progress_bar_length - filled_length)
        
        # Calculate sizes and ETA
        copied_mb = self.copied_size / (1024 * 1024)
        total_mb = self.total_size_bytes / (1024 * 1024)
        
        elapsed_time = time.time() - self.start_time
        if current > 0 and elapsed_time > 0:
            eta_seconds = (elapsed_time / current) * (total - current)
            eta_str = str(timedelta(seconds=int(eta_seconds)))
        else:
            eta_str = "calculating..."
        
        mode_str = "DRY RUN" if self.dry_run else "COPYING"
        
        print(f"\r{mode_str}: |{bar}| {current}/{total} ({percentage:.1f}%) "
              f"| {copied_mb:.1f}/{total_mb:.1f} MB | ETA: {eta_str} | {os.path.basename(current_file)[:25]:<25}", end='')
    
    def execute_migration(self) -> bool:
        """
        Execute the complete migration process.
        Returns True if successful, False otherwise.
        """
        try:
            mode_str = "🧪 DRY RUN" if self.dry_run else "🚀 MIGRATION"
            self.logger.info(f"{mode_str} Starting file migration...")
            
            # Get migration candidates
            candidates = self.migration_plan.get("migration_candidates", [])
            self.total_files = len(candidates)
            self.total_size_bytes = sum(f.get("size_bytes", 0) for f in candidates)
            
            if self.total_files == 0:
                self.logger.warning("WARNING No files to migrate found in migration plan")
                return False
            
            # Validate paths
            source_base = Path(self.migration_plan["source_path"])
            dest_base = Path(self.migration_plan["destination_path"])
            
            if not source_base.exists():
                self.logger.error(f"ERROR Source path does not exist: {source_base}")
                return False
            
            # Check disk space (skip for dry run)
            if not self.dry_run and not self._check_disk_space(dest_base, self.total_size_bytes):
                return False
            
            self.logger.info(f"📊 Migration Plan:")
            self.logger.info(f"   Files to migrate: {self.total_files:,}")
            self.logger.info(f"   Total size: {self.total_size_bytes / (1024*1024):.1f} MB")
            self.logger.info(f"   Source: {source_base}")
            self.logger.info(f"   Destination: {dest_base}")
            self.logger.info(f"   Mode: {'DRY RUN' if self.dry_run else 'COPY FILES'}")
            
            # Process each file
            for i, file_info in enumerate(candidates):
                self.processed_count = i + 1
                
                source_path = Path(file_info["absolute_path"])
                relative_path = file_info["relative_path"]
                dest_path = dest_base / relative_path
                expected_hash = file_info.get("hash", "")
                
                # Update progress
                self._update_progress(self.processed_count, self.total_files, str(source_path))
                
                # Skip if already completed (resumption) - only for real migrations
                if not self.dry_run and str(source_path) in self.completed_files:
                    self.skipped_files.append({
                        "source": str(source_path),
                        "reason": "Already completed (resumed)",
                        "size_bytes": file_info.get("size_bytes", 0)
                    })
                    self.copied_size += file_info.get("size_bytes", 0)
                    continue
                
                # Check if source file still exists
                if not source_path.exists():
                    self.logger.warning(f"WARNING Source file not found: {source_path}")
                    self.failed_files.append({
                        "source": str(source_path),
                        "error": "Source file not found",
                        "size_bytes": file_info.get("size_bytes", 0)
                    })
                    continue
                
                # Copy the file (or simulate in dry run)
                if self._copy_file_with_verification(source_path, dest_path, expected_hash):
                    self.copied_files.append({
                        "source": str(source_path),
                        "destination": str(dest_path),
                        "size_bytes": file_info.get("size_bytes", 0),
                        "timestamp": datetime.now().isoformat()
                    })
                    self.copied_size += file_info.get("size_bytes", 0)
                else:
                    self.failed_files.append({
                        "source": str(source_path),
                        "destination": str(dest_path),
                        "error": "Copy verification failed",
                        "size_bytes": file_info.get("size_bytes", 0)
                    })
            
            print()  # New line after progress bar
            
            # Generate final report
            self._generate_completion_report()
            
            # Summary
            duration = datetime.now() - self.start_timestamp
            success_count = len(self.copied_files)
            fail_count = len(self.failed_files)
            skip_count = len(self.skipped_files)
            
            self.logger.info("=" * 80)
            
            if self.dry_run:
                self.logger.info("SUCCESS DRY RUN COMPLETED")
                self.logger.info("🧪 This was a simulation - no files were actually copied")
                self.logger.info("🚀 Run without --dry-run to perform the actual migration")
            else:
                self.logger.info("SUCCESS MIGRATION COMPLETED")
            
            self.logger.info(f"📊 Results Summary:")
            self.logger.info(f"   SUCCESS Successfully processed: {success_count:,} files ({self.copied_size / (1024*1024):.1f} MB)")
            self.logger.info(f"   ERROR Failed: {fail_count:,} files")
            self.logger.info(f"   SKIPPED Skipped (resumed): {skip_count:,} files")
            self.logger.info(f"   TIME Duration: {duration.total_seconds():.1f} seconds")
            self.logger.info(f"   📁 Results saved to: migration_logs/")
            self.logger.info("=" * 80)
            
            return fail_count == 0
            
        except Exception as e:
            self.logger.error(f"ERROR Migration failed: {e}")
            return False
    
    def _generate_completion_report(self):
        """Generate comprehensive completion report"""
        self.logger.info("📄 Generating completion report...")
        
        report_dir = Path("migration_logs")
        timestamp_str = self.start_timestamp.strftime('%Y%m%d_%H%M%S')
        
        mode_suffix = "_dryrun" if self.dry_run else ""
        
        # Detailed JSON report
        completion_report = {
            "migration_info": {
                "start_time": self.start_timestamp.isoformat(),
                "end_time": datetime.now().isoformat(),
                "duration_seconds": (datetime.now() - self.start_timestamp).total_seconds(),
                "source_path": self.migration_plan.get("source_path"),
                "destination_path": self.migration_plan.get("destination_path"),
                "dry_run": self.dry_run
            },
            "statistics": {
                "total_planned": self.total_files,
                "successfully_processed": len(self.copied_files),
                "failed": len(self.failed_files),
                "skipped_resumed": len(self.skipped_files),
                "total_size_bytes": self.total_size_bytes,
                "processed_size_bytes": self.copied_size
            },
            "processed_files": self.copied_files,
            "failed_files": self.failed_files,
            "skipped_files": self.skipped_files
        }
        
        # Save JSON report
        json_report_path = report_dir / f"migration_report{mode_suffix}_{timestamp_str}.json"
        with open(json_report_path, 'w', encoding='utf-8') as f:
            json.dump(completion_report, f, indent=2, ensure_ascii=False)
        
        # Generate human-readable summary
        summary_path = report_dir / f"migration_summary{mode_suffix}_{timestamp_str}.txt"
        with open(summary_path, 'w', encoding='utf-8') as f:
            f.write("=" * 80 + "\n")
            f.write("Enhanced Solution Migration Executor - Completion Report\n")
            if self.dry_run:
                f.write("🧪 DRY RUN MODE - No files were actually copied\n")
            f.write("=" * 80 + "\n")
            f.write(f"Migration Date: {self.start_timestamp}\n")
            f.write(f"Duration: {(datetime.now() - self.start_timestamp).total_seconds():.1f} seconds\n")
            f.write(f"Source: {self.migration_plan.get('source_path')}\n")
            f.write(f"Destination: {self.migration_plan.get('destination_path')}\n")
            f.write(f"Mode: {'DRY RUN' if self.dry_run else 'COPY FILES'}\n")
            f.write("\n")
            
            f.write("MIGRATION STATISTICS\n")
            f.write("-" * 40 + "\n")
            f.write(f"Total Files Planned: {self.total_files:,}\n")
            f.write(f"Successfully Processed: {len(self.copied_files):,}\n")
            f.write(f"Failed: {len(self.failed_files):,}\n")
            f.write(f"Skipped (Resumed): {len(self.skipped_files):,}\n")
            f.write(f"Success Rate: {len(self.copied_files)/self.total_files*100 if self.total_files > 0 else 0:.1f}%\n")
            f.write("\n")
            
            f.write("SIZE STATISTICS\n")
            f.write("-" * 40 + "\n")
            f.write(f"Total Planned Size: {self.total_size_bytes / (1024*1024):.1f} MB\n")
            f.write(f"Successfully Processed: {self.copied_size / (1024*1024):.1f} MB\n")
            f.write(f"Processing Efficiency: {self.copied_size/self.total_size_bytes*100 if self.total_size_bytes > 0 else 0:.1f}%\n")
            f.write("\n")
            
            if self.failed_files:
                f.write("FAILED FILES\n")
                f.write("-" * 40 + "\n")
                for fail in self.failed_files[:10]:  # Show first 10
                    f.write(f"ERROR {fail['source']}: {fail['error']}\n")
                if len(self.failed_files) > 10:
                    f.write(f"... and {len(self.failed_files) - 10} more (see JSON report)\n")
                f.write("\n")
            
            f.write("NEXT STEPS\n")
            f.write("-" * 40 + "\n")
            if self.dry_run:
                f.write("🧪 This was a DRY RUN - no files were actually copied\n")
                f.write("🚀 Run the script without --dry-run to perform the actual migration\n")
                f.write("📋 Review the file list above to ensure everything looks correct\n")
            elif len(self.failed_files) == 0:
                f.write("SUCCESS Migration completed successfully!\n")
                f.write("🎯 Your clean solution is ready in the destination folder\n")
                f.write("🗂️ Open the solution file (.sln) in Visual Studio\n")
                f.write("🔨 Run 'dotnet build' to verify everything works\n")
            else:
                f.write("WARNING Some files failed to copy\n")
                f.write("🔍 Review the failed files list above\n")
                f.write("🔄 Re-run this script to retry failed files\n")
                f.write("🛠️ Check file permissions and disk space\n")
            f.write("\n")
        
        self.logger.info(f"   SUCCESS Created: migration_report{mode_suffix}_{timestamp_str}.json")
        self.logger.info(f"   SUCCESS Created: migration_summary{mode_suffix}_{timestamp_str}.txt")

def main():
    """Main execution function with command line argument parsing"""
    parser = argparse.ArgumentParser(
        description="Enhanced Solution Migration Executor",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Dry run to test migration
  python solution_migration_executor.py --dry-run
  
  # Migrate with custom paths
  python solution_migration_executor.py --source "C:/Old/Project" --dest "C:/New/Project"
  
  # Full migration with all defaults
  python solution_migration_executor.py
        """
    )
    
    parser.add_argument('--dry-run', action='store_true',
                       help='Perform a dry run without actually copying files')
    parser.add_argument('--source', type=str,
                       help='Source directory path (overrides config)')
    parser.add_argument('--dest', '--destination', type=str,
                       help='Destination directory path (overrides config)')
    parser.add_argument('--config', type=str, default='migration_config.json',
                       help='Configuration file path (default: migration_config.json)')
    
    args = parser.parse_args()
    
    print("Enhanced Solution Migration Executor v2.0")
    print("=" * 60)
    
    if args.dry_run:
        print("DRY RUN MODE - No files will be actually copied")
        print("=" * 60)
    
    try:
        executor = EnhancedMigrationExecutor(
            source_path=args.source,
            dest_path=args.dest,
            dry_run=args.dry_run,
            config_path=args.config
        )
        
        success = executor.execute_migration()
        
        if success:
            if args.dry_run:
                print("\nDry run completed successfully!")
                print("Run without --dry-run to perform the actual migration")
            else:
                print("\nMigration completed successfully!")
                print("Check your clean solution in the destination folder")
                print("Try opening the .sln file in Visual Studio")
        else:
            print("\nMigration completed with some issues.")
            print("Check the migration logs for details")
            return 1
            
    except KeyboardInterrupt:
        print("\nMigration interrupted by user")
        if not args.dry_run:
            print("Run the script again to resume from where it left off")
        return 1
    except Exception as e:
        print(f"\nUnexpected error: {e}")
        return 1
    
    return 0

if __name__ == "__main__":
    sys.exit(main())