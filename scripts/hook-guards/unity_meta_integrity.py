#!/usr/bin/env python3
"""Forest01 Unity .meta integrity. Read-only, stdlib, no Unity required."""
import argparse
import re
import subprocess
from pathlib import Path

GUID = re.compile(r"(?m)^guid:\s*([0-9a-fA-F]{32})\s*$")
ASSET_PREFIX = "UnityProject/Assets/"
SUFFIXES = {".cs", ".png", ".jpg", ".jpeg", ".tga", ".psd", ".prefab", ".unity", ".asset", ".mat", ".shader", ".compute", ".json", ".anim", ".controller", ".uxml", ".uss", ".asmdef", ".spriteatlas"}

def tracked_files(root):
    data = subprocess.check_output(["git", "-C", str(root), "ls-files", "-z", "--", ASSET_PREFIX])
    return {item.decode("utf-8", "surrogateescape") for item in data.split(b"\0") if item}

def changed_files(root, base):
    if not base:
        return set()
    data = subprocess.check_output(["git", "-C", str(root), "diff", "--name-only", "-z", "--diff-filter=ACMR", base, "HEAD", "--", ASSET_PREFIX])
    return {item.decode("utf-8", "surrogateescape") for item in data.split(b"\0") if item}

def check(root, files, changed):
    issues = []
    guids = {}
    for file in sorted(f for f in files if f.startswith(ASSET_PREFIX) and f.endswith(".meta")):
        path = root / file
        if not path.is_file():
            issues.append((file, "tracked metadata file missing"))
            continue
        content = path.read_text(encoding="utf-8", errors="replace")
        found = GUID.search(content)
        if not found:
            issues.append((file, "invalid or absent guid"))
            continue
        guid = found.group(1).lower()
        if guid in guids:
            issues.append((file, "duplicate GUID (also in " + guids[guid] + ")"))
        guids[guid] = file
        # Check any changed metadata referencing neither file nor directory.
        if file in changed:
            asset = file[:-5]
            if not (root / asset).exists():
                issues.append((file, "orphan metadata; corresponding asset is absent"))
    # Only newly/modified tracked assets: avoid retroactively failing unrelated historical content.
    for file in sorted(changed):
        if not file.startswith(ASSET_PREFIX) or file.endswith(".meta") or file not in files:
            continue
        if Path(file).suffix.lower() in SUFFIXES and file + ".meta" not in files:
            issues.append((file, "changed Unity asset lacks a tracked .meta companion"))
    return issues

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", default=".")
    parser.add_argument("--base", help="PR base commit SHA (optional; whole-repository duplicate scan is always run)")
    args = parser.parse_args()
    root = Path(args.root).resolve()
    try:
        files = tracked_files(root)
        changed = changed_files(root, args.base)
        issues = check(root, files, changed)
    except (OSError, subprocess.CalledProcessError) as exc:
        print("UNITY_META_NOT_VERIFIED: unable to read Git checkout")
        return 2
    if issues:
        for path, problem in issues[:60]:
            print(f"UNITY_META_FAIL {path}: {problem}")
        print(f"UNITY_META_FAIL_TOTAL={len(issues)}")
        return 1
    print(f"UNITY_META_PASS: verified {sum(p.endswith('.meta') for p in files)} tracked .meta files, {len(changed)} changed asset paths")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
