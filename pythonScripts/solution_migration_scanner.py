#!/usr/bin/env python3
"""
Enhanced Solution Migration Scanner
==================================
Phase 1: Discovery and Analysis with Advanced Features

Features:
- Configurable source and destination paths
- Smart exclusion filters with exact matching
- File hash calculation for integrity verification
- Duplicate detection and resolution
- Detailed size and structure analysis
- JSON plan generation for safe migration
- Enhanced error handling and logging

Author: Enhanced Migration Assistant
Version: 2.0
"""

import os
import json
import hashlib
import argparse
import sys
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Set, Tuple, Any, Optional
import logging
import time

class EnhancedMigrationScanner:
    """
    Enhanced migration scanner with configurable filters and improved analysis.
    """
    
    def __init__(self, source_path: str, dest_path: str = None, config_path: str = None):
        self.start_timestamp = datetime.now()
        self.source_path = Path(source_path).resolve()
        self.dest_path = Path(dest_path or str(self.source_path.parent / f"{self.source_path.name}_migrated")).resolve()
        self.config_path = config_path or "scanner_config.json"
        
        # Scanning state
        self.scanned_files = []
        self.excluded_files = []
        self.duplicate_files = []
        self.error_files = []
        self.total_size_bytes = 0
        self.file_count = 0
        
        # Load or create configuration
        self.config = self._load_or_create_configuration()
        
        self._setup_logging()
        
    def _load_or_create_configuration(self) -> Dict[str, Any]:
        """Load existing config or create a default one optimized for .NET solutions"""
        config_file = Path(self.config_path)
        
        # Enhanced default configuration for .NET projects
        default_config = {
            "scan_settings": {
                "calculate_hashes": True,
                "detect_duplicates": True,
                "follow_symlinks": False,
                "max_file_size_mb": 500,  # Skip very large files
                "chunk_size_bytes": 8192
            },
            "file_filters": {
                "exclude_extensions": [
                    # Build artifacts
                    ".tmp", ".temp", ".cache", ".log", ".user", 
                    # Binary outputs
                    ".exe", ".dll", ".pdb", ".lib", ".exp",
                    # IDE files
                    ".suo", ".sdf", ".opensdf", ".ncb", ".aps",
                    # Package files
                    ".nupkg", ".zip", ".7z", ".rar",
                    # OS files
                    ".DS_Store", "Thumbs.db", "desktop.ini"
                ],
                "exclude_directories": [
                    # Build outputs (exact name matching)
                    "bin", "obj", "Debug", "Release", "x64", "x86", "AnyCPU",
                    # IDE and tools
                    ".vs", ".vscode", ".idea", ".git", ".svn", ".hg",
                    # Package managers
                    "node_modules", "packages", ".nuget",
                    # Test results
                    "TestResults", "test-results", "coverage",
                    # Documentation builds
                    "_site", "docs/_build"
                ],
                "exclude_patterns": [
                    # Glob patterns for more complex exclusions
                    "**/bin/**", "**/obj/**", "**/.vs/**",
                    "**/*.log", "**/*.tmp", "**/packages/**"
                ],
                "include_only_extensions": [],  # If specified, only these extensions will be included
                "force_include_files": [
                    # Always include these important files
                    "*.sln", "*.csproj", "*.props", "*.targets", "*.config",
                    "*.cs", "*.razor", "*.cshtml", "*.js", "*.css", "*.json",
                    "*.md", "*.txt", "*.yml", "*.yaml", "*.xml"
                ]
            },
            "analysis_settings": {
                "group_by_extension": True,
                "analyze_project_structure": True,
                "detect_solution_files": True,
                "calculate_statistics": True
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
                    elif isinstance(value, dict):
                        for subkey, subvalue in value.items():
                            if subkey not in loaded_config[key]:
                                loaded_config[key][subkey] = subvalue
                return loaded_config
            except Exception as e:
                print(f"Warning: Could not load config file: {e}")
                print("Using default configuration...")
        
        # Create default config file
        try:
            with open(config_file, 'w', encoding='utf-8') as f:
                json.dump(default_config, f, indent=2)
            print(f"Created default scanner configuration: {config_file}")
        except Exception as e:
            print(f"Warning: Could not create config file: {e}")
            
        return default_config
    
    def _setup_logging(self):
        """Setup comprehensive logging system"""
        # Create logs directory
        log_dir = Path("scanner_logs")
        log_dir.mkdir(parents=True, exist_ok=True)
        
        # Setup main log file
        timestamp_str = self.start_timestamp.strftime('%Y%m%d_%H%M%S')
        log_file = log_dir / f"scanner_{timestamp_str}.log"
        
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
        self.logger.info("Enhanced Solution Migration Scanner")
        self.logger.info("=" * 80)
        self.logger.info(f"Scan started: {self.start_timestamp}")
        self.logger.info(f"Source path: {self.source_path}")
        self.logger.info(f"Destination path: {self.dest_path}")
        self.logger.info(f"Configuration: {self.config_path}")
        self.logger.info(f"Log file: {log_file}")
    
    def _calculate_file_hash(self, file_path: Path) -> str:
        """Calculate SHA256 hash of a file for verification"""
        hash_sha256 = hashlib.sha256()
        try:
            chunk_size = self.config["scan_settings"]["chunk_size_bytes"]
            with open(file_path, "rb") as f:
                for chunk in iter(lambda: f.read(chunk_size), b""):
                    hash_sha256.update(chunk)
            return hash_sha256.hexdigest()
        except Exception as e:
            self.logger.warning(f"⚠️ Could not calculate hash for {file_path}: {e}")
            return ""
    
    def _should_exclude_file(self, file_path: Path) -> Tuple[bool, str]:
        """
        Determine if a file should be excluded based on filters.
        Returns (should_exclude, reason)
        """
        # Get relative path for pattern matching
        try:
            relative_path = file_path.relative_to(self.source_path)
        except ValueError:
            return True, "Outside source path"
        
        # Check if any parent directory is in exclude list (exact name matching)
        exclude_dirs = set(self.config["file_filters"]["exclude_directories"])
        for part in relative_path.parts[:-1]:  # Exclude filename
            if part in exclude_dirs:
                return True, f"In excluded directory: {part}"
        
        # Check file extension
        file_ext = file_path.suffix.lower()
        exclude_exts = set(ext.lower() for ext in self.config["file_filters"]["exclude_extensions"])
        if file_ext in exclude_exts:
            return True, f"Excluded extension: {file_ext}"
        
        # Check file size
        try:
            max_size = self.config["scan_settings"]["max_file_size_mb"] * 1024 * 1024
            if file_path.stat().st_size > max_size:
                return True, f"File too large: {file_path.stat().st_size / (1024*1024):.1f} MB"
        except Exception:
            pass
        
        # Check force exclude patterns (these override everything)
        force_exclude = self.config["file_filters"].get("force_exclude_files", [])
        for pattern in force_exclude:
            if file_path.match(pattern) or file_path.name == pattern:
                return True, f"Force excluded: {pattern}"
        
        # Check force include patterns (these override exclusions)
        force_include = self.config["file_filters"]["force_include_files"]
        for pattern in force_include:
            if file_path.match(pattern):
                return False, "Force included"
        
        # Check include-only extensions (if specified)
        include_only = self.config["file_filters"]["include_only_extensions"]
        if include_only and file_ext not in [ext.lower() for ext in include_only]:
            return True, f"Not in include-only list: {file_ext}"
        
        return False, "Included"
    
    def _scan_directory(self) -> bool:
        """
        Scan the source directory and collect file information.
        Returns True if successful, False otherwise.
        """
        try:
            self.logger.info(f"🔍 Starting directory scan...")
            
            if not self.source_path.exists():
                self.logger.error(f"❌ Source path does not exist: {self.source_path}")
                return False
            
            if not self.source_path.is_dir():
                self.logger.error(f"❌ Source path is not a directory: {self.source_path}")
                return False
            
            # Collect all files
            all_files = []
            follow_symlinks = self.config["scan_settings"]["follow_symlinks"]
            
            self.logger.info(f"📁 Discovering files...")
            start_time = time.time()
            
            for file_path in self.source_path.rglob("*"):
                if file_path.is_file():
                    # Handle symlinks
                    if file_path.is_symlink() and not follow_symlinks:
                        continue
                    all_files.append(file_path)
            
            discovery_time = time.time() - start_time
            self.logger.info(f"📊 Discovered {len(all_files):,} files in {discovery_time:.1f} seconds")
            
            # Process each file
            hash_map = {}  # For duplicate detection
            processed = 0
            
            for file_path in all_files:
                processed += 1
                if processed % 1000 == 0:
                    self.logger.info(f"📋 Processed {processed:,}/{len(all_files):,} files...")
                
                try:
                    # Check if file should be excluded
                    should_exclude, reason = self._should_exclude_file(file_path)
                    
                    if should_exclude:
                        self.excluded_files.append({
                            "absolute_path": str(file_path),
                            "relative_path": str(file_path.relative_to(self.source_path)),
                            "reason": reason
                        })
                        continue
                    
                    # Get file information
                    stat = file_path.stat()
                    relative_path = file_path.relative_to(self.source_path)
                    
                    file_info = {
                        "absolute_path": str(file_path),
                        "relative_path": str(relative_path),
                        "file_name": file_path.name,
                        "extension": file_path.suffix,
                        "size_bytes": stat.st_size,
                        "size_mb": stat.st_size / (1024 * 1024),
                        "modified_date": datetime.fromtimestamp(stat.st_mtime).isoformat(),
                        "directory": str(relative_path.parent),
                        "hash": ""
                    }
                    
                    # Calculate hash if enabled
                    if self.config["scan_settings"]["calculate_hashes"]:
                        file_hash = self._calculate_file_hash(file_path)
                        file_info["hash"] = file_hash
                        
                        # Check for duplicates
                        if self.config["scan_settings"]["detect_duplicates"] and file_hash:
                            if file_hash in hash_map:
                                self.duplicate_files.append({
                                    "hash": file_hash,
                                    "original": hash_map[file_hash]["relative_path"],
                                    "duplicate": str(relative_path),
                                    "size_bytes": stat.st_size
                                })
                            else:
                                hash_map[file_hash] = file_info
                    
                    self.scanned_files.append(file_info)
                    self.total_size_bytes += stat.st_size
                    self.file_count += 1
                    
                except Exception as e:
                    self.logger.warning(f"⚠️ Error processing {file_path}: {e}")
                    self.error_files.append({
                        "absolute_path": str(file_path),
                        "error": str(e)
                    })
            
            self.logger.info(f"✅ Scan completed: {self.file_count:,} files, {self.total_size_bytes / (1024*1024):.1f} MB")
            return True
            
        except Exception as e:
            self.logger.error(f"❌ Directory scan failed: {e}")
            return False
    
    def _analyze_project_structure(self) -> Dict[str, Any]:
        """Analyze the project structure and provide insights"""
        analysis = {
            "solution_files": [],
            "project_files": [],
            "extension_stats": {},
            "directory_stats": {},
            "large_files": [],
            "project_structure": {}
        }
        
        try:
            # Find solution and project files
            for file_info in self.scanned_files:
                file_path = Path(file_info["absolute_path"])
                ext = file_info["extension"].lower()
                
                if ext == ".sln":
                    analysis["solution_files"].append(file_info["relative_path"])
                elif ext in [".csproj", ".vbproj", ".fsproj", ".vcxproj"]:
                    analysis["project_files"].append(file_info["relative_path"])
                
                # Extension statistics
                if ext not in analysis["extension_stats"]:
                    analysis["extension_stats"][ext] = {"count": 0, "total_size_mb": 0}
                analysis["extension_stats"][ext]["count"] += 1
                analysis["extension_stats"][ext]["total_size_mb"] += file_info["size_mb"]
                
                # Directory statistics
                dir_name = file_info["directory"]
                if dir_name not in analysis["directory_stats"]:
                    analysis["directory_stats"][dir_name] = {"count": 0, "total_size_mb": 0}
                analysis["directory_stats"][dir_name]["count"] += 1
                analysis["directory_stats"][dir_name]["total_size_mb"] += file_info["size_mb"]
                
                # Large files (> 10MB)
                if file_info["size_mb"] > 10:
                    analysis["large_files"].append({
                        "path": file_info["relative_path"],
                        "size_mb": file_info["size_mb"]
                    })
            
            # Sort statistics by count/size
            analysis["extension_stats"] = dict(sorted(
                analysis["extension_stats"].items(),
                key=lambda x: x[1]["count"], reverse=True
            ))
            
            analysis["directory_stats"] = dict(sorted(
                analysis["directory_stats"].items(),
                key=lambda x: x[1]["total_size_mb"], reverse=True
            ))
            
            analysis["large_files"].sort(key=lambda x: x["size_mb"], reverse=True)
            
        except Exception as e:
            self.logger.warning(f"⚠️ Error during project analysis: {e}")
        
        return analysis
    
    def _generate_migration_plan(self) -> bool:
        """Generate the migration plan JSON file"""
        try:
            self.logger.info("📄 Generating migration plan...")
            
            # Analyze project structure
            analysis = self._analyze_project_structure()
            
            # Create migration plan
            migration_plan = {
                "plan_timestamp": self.start_timestamp.isoformat(),
                "scanner_version": "2.0",
                "source_path": str(self.source_path),
                "destination_path": str(self.dest_path),
                "scan_duration_seconds": (datetime.now() - self.start_timestamp).total_seconds(),
                "configuration": self.config,
                "statistics": {
                    "total_files_discovered": len(self.scanned_files) + len(self.excluded_files),
                    "files_to_migrate": len(self.scanned_files),
                    "files_excluded": len(self.excluded_files),
                    "files_with_errors": len(self.error_files),
                    "duplicate_files": len(self.duplicate_files),
                    "total_size_bytes": self.total_size_bytes,
                    "total_size_mb": self.total_size_bytes / (1024 * 1024)
                },
                "project_analysis": analysis,
                "migration_candidates": self.scanned_files,
                "excluded_files": self.excluded_files[:100],  # Limit to first 100 for size
                "duplicate_files": self.duplicate_files,
                "error_files": self.error_files
            }
            
            # Save migration plan
            plan_file = Path("migration_plan.json")
            with open(plan_file, 'w', encoding='utf-8') as f:
                json.dump(migration_plan, f, indent=2, ensure_ascii=False)
            
            self.logger.info(f"✅ Migration plan saved: {plan_file}")
            
            # Generate human-readable summary
            self._generate_scan_summary(migration_plan)
            
            return True
            
        except Exception as e:
            self.logger.error(f"❌ Failed to generate migration plan: {e}")
            return False
    
    def _generate_scan_summary(self, migration_plan: Dict[str, Any]):
        """Generate human-readable scan summary"""
        try:
            timestamp_str = self.start_timestamp.strftime('%Y%m%d_%H%M%S')
            summary_file = Path("scanner_logs") / f"scan_summary_{timestamp_str}.txt"
            
            with open(summary_file, 'w', encoding='utf-8') as f:
                f.write("=" * 80 + "\n")
                f.write("Enhanced Solution Migration Scanner - Scan Summary\n")
                f.write("=" * 80 + "\n")
                f.write(f"Scan Date: {self.start_timestamp}\n")
                f.write(f"Source: {self.source_path}\n")
                f.write(f"Destination: {self.dest_path}\n")
                f.write(f"Duration: {migration_plan['scan_duration_seconds']:.1f} seconds\n")
                f.write("\n")
                
                stats = migration_plan["statistics"]
                f.write("SCAN STATISTICS\n")
                f.write("-" * 40 + "\n")
                f.write(f"Total Files Discovered: {stats['total_files_discovered']:,}\n")
                f.write(f"Files to Migrate: {stats['files_to_migrate']:,}\n")
                f.write(f"Files Excluded: {stats['files_excluded']:,}\n")
                f.write(f"Duplicate Files Found: {stats['duplicate_files']:,}\n")
                f.write(f"Files with Errors: {stats['files_with_errors']:,}\n")
                f.write(f"Total Size: {stats['total_size_mb']:.1f} MB\n")
                f.write("\n")
                
                analysis = migration_plan["project_analysis"]
                
                # Solution structure
                f.write("PROJECT STRUCTURE\n")
                f.write("-" * 40 + "\n")
                f.write(f"Solution Files: {len(analysis['solution_files'])}\n")
                for sln in analysis['solution_files']:
                    f.write(f"  📁 {sln}\n")
                f.write(f"Project Files: {len(analysis['project_files'])}\n")
                for proj in analysis['project_files'][:10]:  # Show first 10
                    f.write(f"  🔗 {proj}\n")
                if len(analysis['project_files']) > 10:
                    f.write(f"  ... and {len(analysis['project_files']) - 10} more\n")
                f.write("\n")
                
                # Top file types
                f.write("TOP FILE TYPES\n")
                f.write("-" * 40 + "\n")
                for ext, data in list(analysis["extension_stats"].items())[:10]:
                    f.write(f"{ext or 'no extension':<15} {data['count']:>6,} files  {data['total_size_mb']:>8.1f} MB\n")
                f.write("\n")
                
                # Large files
                if analysis["large_files"]:
                    f.write("LARGE FILES (>10MB)\n")
                    f.write("-" * 40 + "\n")
                    for large_file in analysis["large_files"][:10]:
                        f.write(f"📦 {large_file['path']} ({large_file['size_mb']:.1f} MB)\n")
                    f.write("\n")
                
                # Duplicates
                if migration_plan["duplicate_files"]:
                    f.write("DUPLICATE FILES\n")
                    f.write("-" * 40 + "\n")
                    for dup in migration_plan["duplicate_files"][:10]:
                        f.write(f"🔗 {dup['original']} == {dup['duplicate']} ({dup['size_bytes']} bytes)\n")
                    f.write("\n")
                
                f.write("NEXT STEPS\n")
                f.write("-" * 40 + "\n")
                f.write("✅ Scan completed successfully!\n")
                f.write("📋 Migration plan generated: migration_plan.json\n")
                f.write("🚀 Run solution_migration_executor.py --dry-run to test the migration\n")
                f.write("🔧 Edit migration_config.json to adjust copy settings if needed\n")
                f.write("💾 Review the file list and exclusions above\n")
                f.write("\n")
            
            self.logger.info(f"✅ Scan summary saved: {summary_file}")
            
        except Exception as e:
            self.logger.warning(f"⚠️ Could not generate scan summary: {e}")
    
    def execute_scan(self) -> bool:
        """
        Execute the complete scanning process.
        Returns True if successful, False otherwise.
        """
        try:
            self.logger.info("🔍 Starting enhanced solution scan...")
            
            # Validate source path
            if not self.source_path.exists():
                self.logger.error(f"❌ Source path does not exist: {self.source_path}")
                return False
            
            # Scan directory
            if not self._scan_directory():
                return False
            
            # Generate migration plan
            if not self._generate_migration_plan():
                return False
            
            # Summary
            duration = datetime.now() - self.start_timestamp
            
            self.logger.info("=" * 80)
            self.logger.info("✅ SCAN COMPLETED SUCCESSFULLY")
            self.logger.info(f"📊 Results Summary:")
            self.logger.info(f"   📁 Files to migrate: {self.file_count:,}")
            self.logger.info(f"   📦 Total size: {self.total_size_bytes / (1024*1024):.1f} MB")
            self.logger.info(f"   ⚡ Excluded files: {len(self.excluded_files):,}")
            self.logger.info(f"   🔗 Duplicates found: {len(self.duplicate_files):,}")
            self.logger.info(f"   ⏱️ Duration: {duration.total_seconds():.1f} seconds")
            self.logger.info(f"   📋 Migration plan: migration_plan.json")
            self.logger.info("=" * 80)
            
            return True
            
        except Exception as e:
            self.logger.error(f"❌ Scan failed: {e}")
            return False

def main():
    """Main execution function with command line argument parsing"""
    parser = argparse.ArgumentParser(
        description="Enhanced Solution Migration Scanner",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Scan current directory
  python solution_migration_scanner.py
  
  # Scan specific source to specific destination
  python solution_migration_scanner.py --source "C:/MyProject" --dest "C:/CleanProject"
  
  # Use custom config
  python solution_migration_scanner.py --config "my_scanner_config.json"
        """
    )
    
    parser.add_argument('--source', type=str, default='.',
                       help='Source directory path to scan (default: current directory)')
    parser.add_argument('--dest', '--destination', type=str,
                       help='Destination directory path (default: source_migrated)')
    parser.add_argument('--config', type=str, default='scanner_config.json',
                       help='Scanner configuration file path (default: scanner_config.json)')
    
    args = parser.parse_args()
    
    print("Enhanced Solution Migration Scanner v2.0")
    print("=" * 60)
    
    try:
        scanner = EnhancedMigrationScanner(
            source_path=args.source,
            dest_path=args.dest,
            config_path=args.config
        )
        
        success = scanner.execute_scan()
        
        if success:
            print("\nScan completed successfully!")
            print("Check migration_plan.json for the complete file list")
            print("Run solution_migration_executor.py --dry-run to test the migration")
        else:
            print("\nScan completed with errors.")
            print("Check the scanner logs for details")
            return 1
            
    except KeyboardInterrupt:
        print("\nScan interrupted by user")
        return 1
    except Exception as e:
        print(f"\nUnexpected error: {e}")
        return 1
    
    return 0

if __name__ == "__main__":
    sys.exit(main())