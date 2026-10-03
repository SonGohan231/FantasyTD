#!/usr/bin/env python3
import io
import json
import os
import re
import shutil
import sys
import tempfile
import urllib.request
import zipfile
from pathlib import Path

from PIL import Image, ImageDraw

API = "https://api.github.com"
WORKFLOW_NAME = "Higgsfield API Asset Generator"

MAPA_DERIVATIVES = {
    "stylelock-pedostrzal": ("Towers/Pedostrzal.png", (0, 0, 1000, 1152)),
    "stylelock-chochlik": ("Enemies/Chochlik.png", (0, 0, 900, 1152)),
    "mapa-essence-flower-unit": ("Towers/KwiatEsencji.png", None),
    "mapa-oak-guardian-unit": ("Towers/DebowyStraznik.png", None),
    "mapa-frost-bud-unit": ("Towers/MroznyPak.png", None),
    "mapa-wind-sprite-unit": ("Enemies/WichrowySkrzat.png", None),
    "mapa-forest-ogre-unit": ("Enemies/OgrZarosli.png", None),
    "mapa-iron-goblin-unit": ("Enemies/ZelaznyGoblin.png", None),
    "mapa-thorn-king-boss-unit": ("Enemies/KrolCierni.png", None),
    "mapa-spawn-portal-unit": ("Environment/SpawnPortal.png", None),
    "mapa-home-tree-base-unit": ("Environment/HomeTree.png", None),
}

def api_json(url, token):
    req = urllib.request.Request(url)
    req.add_header("Authorization", f"Bearer {token}")
    req.add_header("Accept", "application/vnd.github+json")
    req.add_header("X-GitHub-Api-Version", "2022-11-28")
    with urllib.request.urlopen(req, timeout=120) as response:
        return json.loads(response.read().decode("utf-8"))

def api_bytes(url, token):
    req = urllib.request.Request(url)
    req.add_header("Authorization", f"Bearer {token}")
    req.add_header("Accept", "application/vnd.github+json")
    req.add_header("X-GitHub-Api-Version", "2022-11-28")
    with urllib.request.urlopen(req, timeout=180) as response:
        return response.read()

def list_success_runs(repo, branch, token):
    runs = []
    page = 1
    while True:
        url = f"{API}/repos/{repo}/actions/runs?branch={branch}&per_page=100&page={page}"
        payload = api_json(url, token)
        batch = payload.get("workflow_runs", [])
        if not batch:
            break
        for run in batch:
            if run.get("name") == WORKFLOW_NAME and run.get("conclusion") == "success":
                runs.append(run)
        if len(batch) < 100:
            break
        page += 1
    runs.sort(key=lambda r: r.get("created_at", ""))
    return runs

def select_scope(generation_id, scope):
    gid = generation_id.lower()
    if scope == "map-a":
        return gid.startswith("stylelock-mapa") or gid.startswith("stylelock-pedostrzal") or gid.startswith("stylelock-chochlik") or gid.startswith("mapa-")
    if scope == "maps-2-6":
        return any(gid.startswith(f"map{i}-") for i in range(2, 7))
    if scope == "all":
        return (
            gid.startswith("stylelock-mapa")
            or gid.startswith("stylelock-pedostrzal")
            or gid.startswith("stylelock-chochlik")
            or gid.startswith("mapa-")
            or any(gid.startswith(f"map{i}-") for i in range(2, 7))
        )
    raise ValueError(f"Unsupported scope: {scope}")

def safe_slug(text):
    return re.sub(r"[^A-Za-z0-9_.-]+", "_", text).strip("_")

def make_transparent_cutout(source, crop=None):
    img = Image.open(source).convert("RGB")
    if crop is not None:
        img = img.crop(crop)

    marker = (1, 2, 3)
    work = img.copy()
    draw = ImageDraw.Draw(work)
    w, h = work.size
    seeds = [(0, 0), (w - 1, 0), (0, h - 1), (w - 1, h - 1)]
    for seed in seeds:
        try:
            ImageDraw.floodfill(work, seed, marker, thresh=52)
        except Exception:
            pass

    rgba = img.convert("RGBA")
    src = work.load()
    dst = rgba.load()
    for y in range(h):
        for x in range(w):
            if src[x, y] == marker:
                r, g, b, _ = dst[x, y]
                dst[x, y] = (r, g, b, 0)

    bbox = rgba.getbbox()
    if bbox:
        left, top, right, bottom = bbox
        pad = max(12, int(max(right-left, bottom-top) * 0.025))
        left = max(0, left-pad)
        top = max(0, top-pad)
        right = min(w, right+pad)
        bottom = min(h, bottom+pad)
        rgba = rgba.crop((left, top, right, bottom))
    return rgba

def derivative_key(generation_id):
    gid = generation_id.lower()
    for prefix, config in MAPA_DERIVATIVES.items():
        if gid.startswith(prefix):
            return config
    return None

def main():
    if len(sys.argv) != 2:
        raise SystemExit("usage: higgsfield_importer.py <import-request.json>")

    request = json.loads(Path(sys.argv[1]).read_text(encoding="utf-8"))
    repo = os.environ["GITHUB_REPOSITORY"]
    token = os.environ["GITHUB_TOKEN"]
    source_branch = request.get("source_branch", "visual/higgsfield-api-bridge")
    scope = request.get("scope", "map-a")

    root = Path(request.get("unity_art_root", "UnityProject/Assets/FantasyTD/Art/Higgsfield"))
    source_root = root / "Source"
    resources_root = Path("UnityProject/Assets/FantasyTD/Resources/Higgsfield")
    map_a_root = resources_root / "MAP_A"
    source_root.mkdir(parents=True, exist_ok=True)
    map_a_root.mkdir(parents=True, exist_ok=True)

    runs = list_success_runs(repo, source_branch, token)
    imported = []
    latest_topology = None

    for run in runs:
        artifacts = api_json(f"{API}/repos/{repo}/actions/runs/{run['id']}/artifacts?per_page=100", token).get("artifacts", [])
        artifacts = [a for a in artifacts if not a.get("expired")]
        if not artifacts:
            continue

        artifact = artifacts[0]
        blob = api_bytes(artifact["archive_download_url"], token)

        with zipfile.ZipFile(io.BytesIO(blob)) as zf:
            names = zf.namelist()
            if "manifest.json" not in names:
                continue
            manifest = json.loads(zf.read("manifest.json").decode("utf-8"))
            generation_id = manifest.get("generation_id", "")
            if not generation_id or not select_scope(generation_id, scope):
                continue

            gid_dir = source_root / safe_slug(generation_id)
            gid_dir.mkdir(parents=True, exist_ok=True)

            files = manifest.get("files", [])
            copied = []
            for name in files:
                if name not in names:
                    continue
                target = gid_dir / Path(name).name
                with zf.open(name) as src, open(target, "wb") as dst:
                    shutil.copyfileobj(src, dst)
                copied.append(str(target))

            for metadata_name in ("manifest.json", "final.json", "submit.json"):
                if metadata_name in names:
                    (gid_dir / metadata_name).write_bytes(zf.read(metadata_name))

            resource_path = None
            image_file = next((gid_dir / Path(n).name for n in files if n.lower().endswith(".png")), None)
            gid_lower = generation_id.lower()

            if image_file and image_file.exists():
                if gid_lower.startswith("mapa-topology-locked-production"):
                    topology_candidate = (run.get("created_at", ""), image_file, generation_id)
                    if latest_topology is None or topology_candidate[0] > latest_topology[0]:
                        latest_topology = topology_candidate
                else:
                    deriv = derivative_key(generation_id)
                    if deriv:
                        relative, crop = deriv
                        target = map_a_root / relative
                        target.parent.mkdir(parents=True, exist_ok=True)
                        cutout = make_transparent_cutout(image_file, crop)
                        cutout.save(target)
                        resource_path = "Higgsfield/MAP_A/" + str(Path(relative).with_suffix("")).replace("\\", "/")

            imported.append({
                "generationId": generation_id,
                "runId": run["id"],
                "artifactId": artifact["id"],
                "createdAt": run.get("created_at"),
                "modelPath": manifest.get("model_path"),
                "files": copied,
                "resourcePath": resource_path,
            })

    if latest_topology:
        _, image_file, generation_id = latest_topology
        map_target = map_a_root / "MapBackground.png"
        shutil.copy2(image_file, map_target)
        for entry in imported:
            if entry["generationId"] == generation_id:
                entry["resourcePath"] = "Higgsfield/MAP_A/MapBackground"

    catalog = {
        "sourceBranch": source_branch,
        "scope": scope,
        "count": len(imported),
        "entries": imported,
    }
    resources_root.mkdir(parents=True, exist_ok=True)
    (resources_root / "catalog.json").write_text(json.dumps(catalog, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    print(json.dumps({
        "scope": scope,
        "imported": len(imported),
        "topologyMap": latest_topology[2] if latest_topology else None,
        "mapAResources": sorted(str(p.relative_to(resources_root)) for p in map_a_root.rglob("*") if p.is_file()),
    }, indent=2))

if __name__ == "__main__":
    main()
