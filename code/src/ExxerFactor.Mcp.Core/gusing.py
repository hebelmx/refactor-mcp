import os
import re

ROOT_DIR = os.path.abspath(".")
FILE_EXTENSION = ".cs"
OUTPUT_FILE = "global_usings.cs"
SKIPPED_LOG = "skipped_files.log"

USING_PATTERN = re.compile(r'^\s*using\s+([\w\.]+)\s*;', re.MULTILINE)
NAMESPACE_PATTERN = re.compile(r'^\s*namespace\s+([\w\.]+)', re.MULTILINE)

def scan_cs_files(root_dir):
    using_set = set()
    namespace_set = set()
    skipped_files = []
    file_count = 0

    for dirpath, _, filenames in os.walk(root_dir):
        for filename in filenames:
            if filename.endswith(FILE_EXTENSION) and not filename.startswith("global_usings"):
                file_count += 1
                if file_count % 100 == 0:
                    print(f"Scanned {file_count} files...")

                full_path = os.path.join(dirpath, filename)
                try:
                    with open(full_path, 'r', encoding='utf-8', errors='ignore') as file:
                        content = file.read()
                        using_matches = USING_PATTERN.findall(content)
                        namespace_matches = NAMESPACE_PATTERN.findall(content)
                        using_set.update(using_matches)
                        namespace_set.update(namespace_matches)
                except Exception as ex:
                    skipped_files.append(f"{full_path} - {ex}")

    return sorted(using_set), sorted(namespace_set), skipped_files

def write_global_usings(using_list):
    with open(OUTPUT_FILE, 'w', encoding='utf-8') as output:
        output.write("// Auto-generated global usings file\n\n")
        for using in using_list:
            output.write(f"global using {using};\n")
    print(f"✔ Global usings written to {OUTPUT_FILE}")

def write_skipped_files(skipped_files):
    if skipped_files:
        with open(SKIPPED_LOG, 'w', encoding='utf-8') as log:
            log.write("Skipped files due to errors:\n\n")
            for entry in skipped_files:
                log.write(f"{entry}\n")
        print(f"⚠ Skipped {len(skipped_files)} files. Details logged in {SKIPPED_LOG}")
    else:
        print("✔ No files skipped.")

if __name__ == "__main__":
    print(f"🔍 Scanning directory: {ROOT_DIR}")
    usings, namespaces, skipped = scan_cs_files(ROOT_DIR)
    write_global_usings(usings)
    write_skipped_files(skipped)
