#!/usr/bin/env python3
"""
ExxerAI Migration Executor - Phase 2
====================================
Safe File Migration Script

Purpose: Execute the migration plan created by Phase 1 scanner:
- Copy files with checksum verification
- Create detailed operation logs
- Support resumption of interrupted migrations
- Generate comprehensive completion reports
- NEVER delete original files (safe operation)

Author: ExxerAI Migration Assistant
Version: 1.0
"""

import os
import json
import hashlib
import shutil
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Set, Tuple, Any, Optional
import logging
import time

class ExxerMigrationExecutor:
	"""
	Executes the migration plan created by Phase 1 scanner.
	Performs safe file copying with comprehensive verification and logging.
	"""
	
	def __init__(self, config_path: str = None):
		self.start_timestamp = datetime.now()
		self.config_path = config_path or "vs/config_template.json"
		
		# Load configuration and migration plan
		self.config = self._load_configuration()
		self.migration_plan = self._load_migration_plan()
		
		# Migration state
		self.copied_files = []
		self.failed_files = []
		self.skipped_files = []
		self.total_files = 0
		self.total_size_bytes = 0
		self.processed_count = 0
		self.copied_size = 0
		
		# Resume capability
		self.resume_log_path = None
		self.completed_files = set()
		
		self._setup_logging()
		self._load_resume_state()
		
	def _load_configuration(self) -> Dict[str, Any]:
		"""Load the configuration from Phase 1"""
		config_file = Path(self.config_path)
		if not config_file.exists():
			raise FileNotFoundError(f"Configuration file not found: {config_file}")
			
		with open(config_file, 'r', encoding='utf-8') as f:
			return json.load(f)
	
	def _load_migration_plan(self) -> Dict[str, Any]:
		"""Load the migration plan from Phase 1"""
		plan_file = Path("vs/migration_plan.json")
		if not plan_file.exists():
			raise FileNotFoundError(f"Migration plan not found: {plan_file}. Run Phase 1 scanner first.")
			
		with open(plan_file, 'r', encoding='utf-8') as f:
			return json.load(f)
	
	def _setup_logging(self):
		"""Setup comprehensive logging system"""
		# Create logs directory
		log_dir = Path("vs/migration_logs")
		log_dir.mkdir(parents=True, exist_ok=True)
		
		# Setup main log file
		timestamp_str = self.start_timestamp.strftime('%Y%m%d_%H%M%S')
		log_file = log_dir / f"migration_{timestamp_str}.log"
		
		# Setup resume log file
		self.resume_log_path = log_dir / f"completed_files_{timestamp_str}.txt"
		
		# Configure logging
		logging.basicConfig(
			level=logging.INFO,
			format='%(asctime)s - %(levelname)s - %(message)s',
			handlers=[
				logging.FileHandler(log_file, encoding='utf-8'),
				logging.StreamHandler()  # Console output
			]
		)
		
		self.logger = logging.getLogger(__name__)
		self.logger.info("=" * 80)
		self.logger.info("ExxerAI Migration Executor - Phase 2")
		self.logger.info("=" * 80)
		self.logger.info(f"Migration started: {self.start_timestamp}")
		self.logger.info(f"Configuration: {self.config_path}")
		self.logger.info(f"Resume log: {self.resume_log_path}")
		
	def _load_resume_state(self):
		"""Load previous completion state for resumption"""
		if self.resume_log_path.exists():
			self.logger.info("🔄 Found previous migration state. Loading for resumption...")
			with open(self.resume_log_path, 'r', encoding='utf-8') as f:
				for line in f:
					line = line.strip()
					if line:
						self.completed_files.add(line)
			self.logger.info(f"📁 Resuming: {len(self.completed_files)} files already completed")
		else:
			self.logger.info("🆕 Starting fresh migration")
	
	def _calculate_file_hash(self, file_path: Path) -> str:
		"""Calculate MD5 hash of a file for verification"""
		hash_md5 = hashlib.md5()
		try:
			with open(file_path, "rb") as f:
				for chunk in iter(lambda: f.read(4096), b""):
					hash_md5.update(chunk)
			return hash_md5.hexdigest()
		except Exception as e:
			self.logger.warning(f"⚠️ Could not calculate hash for {file_path}: {e}")
			return ""
	
	def _copy_file_with_verification(self, source_path: Path, dest_path: Path) -> bool:
		"""
		Copy a single file with verification.
		Returns True if successful, False otherwise.
		"""
		try:
			# Ensure destination directory exists
			dest_path.parent.mkdir(parents=True, exist_ok=True)
			
			# Get source file info
			source_size = source_path.stat().st_size
			
			# Copy the file
			shutil.copy2(source_path, dest_path)
			
			# Verify the copy
			if not dest_path.exists():
				self.logger.error(f"❌ Copy failed - destination file not created: {dest_path}")
				return False
			
			# Verify size
			dest_size = dest_path.stat().st_size
			if source_size != dest_size:
				self.logger.error(f"❌ Size mismatch: {source_path} ({source_size}) vs {dest_path} ({dest_size})")
				dest_path.unlink()  # Remove corrupted copy
				return False
			
			# Verify checksum for files > 1KB (performance optimization)
			if source_size > 1024:
				source_hash = self._calculate_file_hash(source_path)
				dest_hash = self._calculate_file_hash(dest_path)
				
				if source_hash and dest_hash and source_hash != dest_hash:
					self.logger.error(f"❌ Checksum mismatch: {source_path}")
					dest_path.unlink()  # Remove corrupted copy
					return False
			
			# Log successful copy to resume file
			with open(self.resume_log_path, 'a', encoding='utf-8') as f:
				f.write(f"{source_path}\n")
			
			return True
			
		except Exception as e:
			self.logger.error(f"❌ Failed to copy {source_path}: {e}")
			return False
	
	def _update_progress(self, current: int, total: int, current_file: str = ""):
		"""Update and display progress"""
		percentage = (current / total * 100) if total > 0 else 0
		progress_bar_length = 30
		filled_length = int(progress_bar_length * current // total) if total > 0 else 0
		
		bar = '█' * filled_length + '-' * (progress_bar_length - filled_length)
		
		# Calculate sizes
		copied_mb = self.copied_size / (1024 * 1024)
		total_mb = self.total_size_bytes / (1024 * 1024)
		
		print(f"\r📁 Progress: |{bar}| {current}/{total} ({percentage:.1f}%) "
			  f"| {copied_mb:.1f}/{total_mb:.1f} MB | {os.path.basename(current_file)[:30]:<30}", end='')
	
	def execute_migration(self) -> bool:
		"""
		Execute the complete migration process.
		Returns True if successful, False otherwise.
		"""
		try:
			self.logger.info("🚀 Starting file migration...")
			
			# Get migration candidates
			candidates = self.migration_plan.get("migration_candidates", [])
			self.total_files = len(candidates)
			self.total_size_bytes = sum(f.get("size_bytes", 0) for f in candidates)
			
			if self.total_files == 0:
				self.logger.warning("⚠️ No files to migrate found in migration plan")
				return False
			
			self.logger.info(f"📊 Migration Plan:")
			self.logger.info(f"   Files to migrate: {self.total_files:,}")
			self.logger.info(f"   Total size: {self.total_size_bytes / (1024*1024):.1f} MB")
			self.logger.info(f"   Source: {self.migration_plan.get('source_path', 'Unknown')}")
			self.logger.info(f"   Destination: {self.migration_plan.get('destination_path', 'Unknown')}")
			
			# Process each file
			source_base = Path(self.migration_plan["source_path"])
			dest_base = Path(self.migration_plan["destination_path"])
			
			for i, file_info in enumerate(candidates):
				self.processed_count = i + 1
				
				source_path = Path(file_info["absolute_path"])
				relative_path = file_info["relative_path"]
				dest_path = dest_base / relative_path
				
				# Update progress
				self._update_progress(self.processed_count, self.total_files, str(source_path))
				
				# Skip if already completed (resumption)
				if str(source_path) in self.completed_files:
					self.skipped_files.append({
						"source": str(source_path),
						"reason": "Already completed (resumed)",
						"size_bytes": file_info.get("size_bytes", 0)
					})
					self.copied_size += file_info.get("size_bytes", 0)
					continue
				
				# Check if source file still exists
				if not source_path.exists():
					self.logger.warning(f"⚠️ Source file not found: {source_path}")
					self.failed_files.append({
						"source": str(source_path),
						"error": "Source file not found",
						"size_bytes": file_info.get("size_bytes", 0)
					})
					continue
				
				# Copy the file
				if self._copy_file_with_verification(source_path, dest_path):
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
			self.logger.info("✅ MIGRATION COMPLETED")
			self.logger.info(f"📊 Results Summary:")
			self.logger.info(f"   ✅ Successfully copied: {success_count:,} files ({self.copied_size / (1024*1024):.1f} MB)")
			self.logger.info(f"   ❌ Failed: {fail_count:,} files")
			self.logger.info(f"   ⏭️ Skipped (resumed): {skip_count:,} files")
			self.logger.info(f"   ⏱️ Duration: {duration.total_seconds():.1f} seconds")
			self.logger.info(f"   📁 Results saved to: vs/migration_logs/")
			self.logger.info("=" * 80)
			
			return fail_count == 0
			
		except Exception as e:
			self.logger.error(f"❌ Migration failed: {e}")
			return False
	
	def _generate_completion_report(self):
		"""Generate comprehensive completion report"""
		self.logger.info("📄 Generating completion report...")
		
		report_dir = Path("vs/migration_logs")
		timestamp_str = self.start_timestamp.strftime('%Y%m%d_%H%M%S')
		
		# Detailed JSON report
		completion_report = {
			"migration_info": {
				"start_time": self.start_timestamp.isoformat(),
				"end_time": datetime.now().isoformat(),
				"duration_seconds": (datetime.now() - self.start_timestamp).total_seconds(),
				"source_path": self.migration_plan.get("source_path"),
				"destination_path": self.migration_plan.get("destination_path")
			},
			"statistics": {
				"total_planned": self.total_files,
				"successfully_copied": len(self.copied_files),
				"failed": len(self.failed_files),
				"skipped_resumed": len(self.skipped_files),
				"total_size_bytes": self.total_size_bytes,
				"copied_size_bytes": self.copied_size
			},
			"copied_files": self.copied_files,
			"failed_files": self.failed_files,
			"skipped_files": self.skipped_files
		}
		
		# Save JSON report
		json_report_path = report_dir / f"migration_report_{timestamp_str}.json"
		with open(json_report_path, 'w', encoding='utf-8') as f:
			json.dump(completion_report, f, indent=2, ensure_ascii=False)
		
		# Generate human-readable summary
		summary_path = report_dir / f"migration_summary_{timestamp_str}.txt"
		with open(summary_path, 'w', encoding='utf-8') as f:
			f.write("=" * 80 + "\n")
			f.write("ExxerAI Migration Executor - Completion Report\n")
			f.write("=" * 80 + "\n")
			f.write(f"Migration Date: {self.start_timestamp}\n")
			f.write(f"Duration: {(datetime.now() - self.start_timestamp).total_seconds():.1f} seconds\n")
			f.write(f"Source: {self.migration_plan.get('source_path')}\n")
			f.write(f"Destination: {self.migration_plan.get('destination_path')}\n")
			f.write("\n")
			
			f.write("MIGRATION STATISTICS\n")
			f.write("-" * 40 + "\n")
			f.write(f"Total Files Planned: {self.total_files:,}\n")
			f.write(f"Successfully Copied: {len(self.copied_files):,}\n")
			f.write(f"Failed: {len(self.failed_files):,}\n")
			f.write(f"Skipped (Resumed): {len(self.skipped_files):,}\n")
			f.write(f"Success Rate: {len(self.copied_files)/self.total_files*100 if self.total_files > 0 else 0:.1f}%\n")
			f.write("\n")
			
			f.write("SIZE STATISTICS\n")
			f.write("-" * 40 + "\n")
			f.write(f"Total Planned Size: {self.total_size_bytes / (1024*1024):.1f} MB\n")
			f.write(f"Successfully Copied: {self.copied_size / (1024*1024):.1f} MB\n")
			f.write(f"Copy Efficiency: {self.copied_size/self.total_size_bytes*100 if self.total_size_bytes > 0 else 0:.1f}%\n")
			f.write("\n")
			
			if self.failed_files:
				f.write("FAILED FILES\n")
				f.write("-" * 40 + "\n")
				for fail in self.failed_files[:10]:  # Show first 10
					f.write(f"❌ {fail['source']}: {fail['error']}\n")
				if len(self.failed_files) > 10:
					f.write(f"... and {len(self.failed_files) - 10} more (see JSON report)\n")
				f.write("\n")
			
			f.write("NEXT STEPS\n")
			f.write("-" * 40 + "\n")
			if len(self.failed_files) == 0:
				f.write("✅ Migration completed successfully!\n")
				f.write("🎯 Your clean solution is ready in the destination folder\n")
				f.write("🗂️ Open the solution file (.sln) in Visual Studio\n")
				f.write("🔨 Run 'dotnet build' to verify everything works\n")
			else:
				f.write("⚠️ Some files failed to copy\n")
				f.write("🔍 Review the failed files list above\n")
				f.write("🔄 Re-run this script to retry failed files\n")
				f.write("🛠️ Check file permissions and disk space\n")
			f.write("\n")
		
		self.logger.info(f"   ✅ Created: migration_report_{timestamp_str}.json")
		self.logger.info(f"   ✅ Created: migration_summary_{timestamp_str}.txt")

def main():
	"""Main execution function"""
	print("🔧 ExxerAI Migration Executor - Phase 2")
	print("=" * 50)
	
	# Check for required files
	required_files = [
		"vs/config_template.json",
		"vs/migration_plan.json"
	]
	
	missing_files = [f for f in required_files if not Path(f).exists()]
	if missing_files:
		print("❌ Missing required files:")
		for f in missing_files:
			print(f"   - {f}")
		print("\n💡 Run Phase 1 scanner (solution_scanner_analyzer.py) first!")
		return 1
	
	print("📋 Configuration found, starting migration...")
	print("⚠️  WARNING: This will copy files to the destination.")
	print("✅ Original files will NOT be deleted (safe operation)")
	print()
	
	try:
		executor = ExxerMigrationExecutor()
		success = executor.execute_migration()
		
		if success:
			print("\n🎉 Migration completed successfully!")
			print("📁 Check your clean solution in: vs/")
			print("🔨 Try opening the .sln file in Visual Studio")
		else:
			print("\n⚠️ Migration completed with some issues.")
			print("📋 Check the migration logs for details")
			return 1
			
	except KeyboardInterrupt:
		print("\n⚠️ Migration interrupted by user")
		print("🔄 Run the script again to resume from where it left off")
		return 1
	except Exception as e:
		print(f"\n❌ Unexpected error: {e}")
		return 1
	
	return 0

if __name__ == "__main__":
	exit(main()) 