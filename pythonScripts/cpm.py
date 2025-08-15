import xml.etree.ElementTree as ET
import io
from pathlib import Path
from collections import defaultdict
from packaging import version
import logging
import argparse
import sys

def detect_existing_package_versions(repo_path: Path):
    package_versions = defaultdict(set)
    for csproj in repo_path.rglob("*.csproj"):
        print(f"Scanning: {csproj}")
        try:
            tree = ET.parse(csproj)
            root = tree.getroot()
            for pkg_ref in root.findall(".//PackageReference"):
                pkg_name = pkg_ref.attrib.get("Include")
                pkg_version = pkg_ref.attrib.get("Version")
                if pkg_name and pkg_version:
                    package_versions[pkg_name].add(pkg_version)
        except ET.ParseError:
            logging.warning(f"Failed to parse {csproj}")
    return package_versions

def select_latest_versions(package_versions):
    latest_versions = {}
    for pkg, versions in package_versions.items():
        sorted_versions = sorted(versions, key=version.parse, reverse=True)
        latest_versions[pkg] = sorted_versions[0]
    return latest_versions

def generate_directory_packages_props(repo_path: Path, package_versions: dict, dry_run: bool):
    log_file = repo_path / "cpm_autogen.log"
    logging.basicConfig(
        level=logging.INFO,
        format='%(message)s',
        handlers=[
            logging.FileHandler(log_file, mode='w', encoding='utf-8'),
            logging.StreamHandler(sys.stdout)
        ]
    )

    logger = logging.getLogger()

    props_file = repo_path / "Directory.Packages.props"

    project = ET.Element("Project")
    property_group = ET.SubElement(project, "PropertyGroup")
    ET.SubElement(property_group, "ManagePackageVersionsCentrally").text = "true"

    if package_versions:
        item_group = ET.SubElement(project, "ItemGroup")
        for pkg, ver in sorted(package_versions.items()):
            ET.SubElement(item_group, "PackageVersion", Include=pkg, Version=ver)

    ET.indent(project, space="  ", level=0)
    preview_xml = ET.tostring(project, encoding='unicode')
    logger.info("=== Generated Directory.Packages.props ===\n" + preview_xml)

    if not dry_run:
        ET.ElementTree(project).write(props_file, encoding="utf-8", xml_declaration=True)
        logger.info(f"\n✅ Directory.Packages.props created at: {props_file}")
        remove_versions_from_projects(repo_path, logger)
    else:
        logger.info("\n⚠️ Dry run mode: No files were written.")

def remove_versions_from_projects(repo_path: Path, logger):
    for csproj in repo_path.rglob("*.csproj"):
        try:
            tree = ET.parse(csproj)
            root = tree.getroot()
            modified = False
            for pkg_ref in root.findall(".//PackageReference"):
                if "Version" in pkg_ref.attrib:
                    logger.info(f"Removing Version from {pkg_ref.attrib.get('Include')} in {csproj}")
                    del pkg_ref.attrib["Version"]
                    modified = True
            if modified:
                ET.indent(tree, space="  ", level=0)
                tree.write(csproj, encoding="utf-8", xml_declaration=True)
        except ET.ParseError:
            logger.warning(f"Failed to parse {csproj}")

def main():
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    default_repo = r"F:\Dynamic\Refactor\refactor-mcp"
    apply = False

    parser = argparse.ArgumentParser(description="Enable Central Package Management in a .NET repo")
    parser.add_argument("repo_path", nargs="?", default=default_repo, help="Path to the root of the .NET repository")
    parser.add_argument("--apply", action="store_true", default=apply, help="Apply changes instead of dry run")
    args = parser.parse_args()

    repo_path = Path(args.repo_path).resolve()
    if not repo_path.exists() or not repo_path.is_dir():
        raise ValueError(f"{repo_path} is not a valid directory")

    detected = detect_existing_package_versions(repo_path)
    latest_versions = select_latest_versions(detected)
    generate_directory_packages_props(repo_path, latest_versions, dry_run=not args.apply)

main()
