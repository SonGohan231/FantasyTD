#!/usr/bin/env bash
set -euo pipefail

if [[ -z "${GH_TOKEN:-}" ]]; then
  echo "GH_TOKEN is required to materialize Higgsfield assets." >&2
  exit 1
fi

ROOT="${GITHUB_WORKSPACE:-$(pwd)}"
DEST="$ROOT/UnityProject/Assets/FantasyTD/Resources/Higgsfield"
mkdir -p "$DEST"

download_asset() {
  local artifact_id="$1"
  local output_name="$2"
  local tmp
  tmp="$(mktemp --suffix=.zip)"

  echo "Materializing Higgsfield artifact $artifact_id -> $output_name"
  curl --fail --location --retry 3 --retry-all-errors     -H "Authorization: Bearer $GH_TOKEN"     -H "Accept: application/vnd.github+json"     -H "X-GitHub-Api-Version: 2022-11-28"     "https://api.github.com/repos/$GITHUB_REPOSITORY/actions/artifacts/$artifact_id/zip"     -o "$tmp"

  unzip -p "$tmp" asset_01.png > "$DEST/$output_name"
  rm -f "$tmp"

  if [[ ! -s "$DEST/$output_name" ]]; then
    echo "Materialized file is empty: $DEST/$output_name" >&2
    exit 1
  fi
}

# Existing successful prepaid Higgsfield generations.
download_asset 11219593382 "MAP_A.png"
download_asset 11219567888 "SEED_SHOOTER_SHEET.png"
download_asset 11219935723 "WANDERER_SHEET.png"

echo "Higgsfield assets ready:"
ls -lh "$DEST"
