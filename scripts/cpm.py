from pathlib import Path
import xml.etree.ElementTree as ET
import logging
from packaging import version

def enable_cpm_with_dry_run(repo_path: str, package_versions: dict, dry_run: bool = True):
    repo_root = Path(repo_path).resolve()
    if not repo_root.exists() or not repo_root.is_dir():
        raise ValueError(f"{repo_path} is not a valid directory")

    log_file = repo_root / "cpm_dry_run.log"
    logging.basicConfig(level=logging.INFO, format='%(message)s', handlers=[
        logging.FileHandler(log_file, mode='w'),
        logging.StreamHandler()
    ])
    logger = logging.getLogger()

    props_file = repo_root / "Directory.Packages.props"

    # Build props file XML content
    project = ET.Element("Project")
    property_group = ET.SubElement(project, "PropertyGroup")
    ET.SubElement(property_group, "ManagePackageVersionsCentrally").text = "true"

    if package_versions:
        item_group = ET.SubElement(project, "ItemGroup")
        for pkg, ver in package_versions.items():
            ET.SubElement(item_group, "PackageVersion", Include=pkg, Version=ver)

    # Convert XML to string for dry-run logging
    props_preview = ET.tostring(project, encoding='unicode')
    ET.indent(project, space="  ", level=0)

    logger.info("=== Directory.Packages.props Preview ===")
    logger.info(props_preview)

    # Check and log changes for csproj files
    for csproj in repo_root.rglob("*.csproj"):
        tree = ET.parse(csproj)
        root = tree.getroot()
        changes = []
        for pkg_ref in root.findall(".//PackageReference"):
            if "Version" in pkg_ref.attrib:
                changes.append(f"Remove Version from {pkg_ref.attrib['Include']} in {csproj.name}")
        if changes:
            logger.info(f"\n--- Changes for {csproj.name} ---")
            for change in changes:
                logger.info(change)
            if not dry_run:
                for pkg_ref in root.findall(".//PackageReference"):
                    if "Version" in pkg_ref.attrib:
                        del pkg_ref.attrib["Version"]
                ET.indent(tree, space="  ", level=0)
                tree.write(csproj, encoding="utf-8", xml_declaration=True)

    # Apply props file if not in dry-run
    if not dry_run:
        tree = ET.ElementTree(project)
        tree.write(props_file, encoding="utf-8", xml_declaration=True)
        logger.info(f"\n✅ Changes applied. Created: {props_file}")
    else:
        logger.info("\n⚠️ Dry run completed. No files were modified.")

# Example dry-run usage
enable_cpm_with_dry_run(
    repo_path="/mnt/data/example-dotnet-solution",
    package_versions={
        "Newtonsoft.Json": "13.0.3",
        "Microsoft.EntityFrameworkCore": "7.0.10"
    },
    dry_run=True
)




def select_latest_versions(package_versions):
    latest_versions = {}
    for pkg, versions in package_versions.items():
        sorted_versions = sorted(versions, key=version.parse, reverse=True)
        latest_versions[pkg] = sorted_versions[0]
    return latest_versions

def generate_directory_packages_props(repo_path: str, package_versions: dict, dry_run: bool = True):
    repo_root = Path(repo_path).resolve()
    if not repo_root.exists() or not repo_root.is_dir():
        raise ValueError(f"{repo_path} is not a valid directory")

    log_file = repo_root / "cpm_autogen.log"
    logging.basicConfig(level=logging.INFO, format='%(message)s', handlers=[
        logging.FileHandler(log_file, mode='w'),
        logging.StreamHandler()
    ])
    logger = logging.getLogger()

    props_file = repo_root / "Directory.Packages.props"

    project = ET.Element("Project")
    property_group = ET.SubElement(project, "PropertyGroup")
    ET.SubElement(property_group, "ManagePackageVersionsCentrally").text = "true"

    if package_versions:
        item_group = ET.SubElement(project, "ItemGroup")
        for pkg, ver in package_versions.items():
            ET.SubElement(item_group, "PackageVersion", Include=pkg, Version=ver)

    # Convert XML to string for logging
    props_preview = ET.tostring(project, encoding='unicode')
    ET.indent(project, space="  ", level=0)
    logger.info("=== Generated Directory.Packages.props ===")
    logger.info(props_preview)

    if not dry_run:
        tree = ET.ElementTree(project)
        tree.write(props_file, encoding="utf-8", xml_declaration=True)
        logger.info(f"\n✅ Directory.Packages.props created at: {props_file}")
    else:
        logger.info("\n⚠️ Dry run mode: No files were written.")

# Detect -> Pick latest -> Generate props
detected_versions = detect_existing_package_versions("/mnt/data/example-dotnet-solution")
latest_versions = select_latest_versions(detected_versions)
generate_directory_packages_props("/mnt/data/example-dotnet-solution", latest_versions, dry_run=True)
