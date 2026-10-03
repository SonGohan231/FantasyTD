#!/usr/bin/env bash
set -euo pipefail

if [[ -z "${GH_TOKEN:-}" ]]; then
  echo "GH_TOKEN is required to materialize Higgsfield assets." >&2
  exit 1
fi

ROOT="${GITHUB_WORKSPACE:-$(pwd)}"
DEST="$ROOT/UnityProject/Assets/FantasyTD/Resources/Higgsfield"
mkdir -p "$DEST"

download_artifact() {
  local artifact_id="$1"
  local output_name="$2"
  local tmp
  tmp="$(mktemp --suffix=.zip)"

  echo "Materializing Higgsfield artifact $artifact_id -> $output_name"
  curl --fail --location --retry 3 --retry-all-errors \
    -H "Authorization: Bearer $GH_TOKEN" \
    -H "Accept: application/vnd.github+json" \
    -H "X-GitHub-Api-Version: 2022-11-28" \
    "https://api.github.com/repos/$GITHUB_REPOSITORY/actions/artifacts/$artifact_id/zip" \
    -o "$tmp"

  unzip -p "$tmp" asset_01.png > "$DEST/$output_name"
  rm -f "$tmp"

  if [[ ! -s "$DEST/$output_name" ]]; then
    echo "Materialized file is empty: $DEST/$output_name" >&2
    exit 1
  fi
}

download_cdn() {
  local url="$1"
  local output_name="$2"
  echo "Materializing connected Higgsfield generation -> $output_name"
  curl --fail --location --retry 3 --retry-all-errors "$url" -o "$DEST/$output_name"
  if [[ ! -s "$DEST/$output_name" ]]; then
    echo "Materialized file is empty: $DEST/$output_name" >&2
    exit 1
  fi
}

# Original MAP-A style lock and first two unit style locks.
download_artifact 11219593382 "MAP_A.png"
download_artifact 11219567888 "SEED_SHOOTER_SHEET.png"
download_artifact 11219935723 "WANDERER_SHEET.png"

# T0012 MAP-A unit production assets generated through Higgsfield.
download_artifact 11269897822 "ESSENCE_FLOWER.png"
download_artifact 11269588052 "OAK_GUARDIAN.png"
download_artifact 11269862777 "RUNNER.png"
download_artifact 11269493528 "BRUTE.png"
download_artifact 11270112107 "BOSS.png"
download_artifact 11268964482 "PORTAL.png"

# Connected Higgsfield generations used after the separate prepaid bridge exhausted credits.
download_cdn "https://d8j0ntlcm91z4.cloudfront.net/user_3JGQx06i0dP8dCxeDB39wmIRZBq/hf_20261003_085840_f98766f9-714c-4f0f-9fe2-e47e89c98cf4.png" "FROST_BUD.png"
download_cdn "https://d8j0ntlcm91z4.cloudfront.net/user_3JGQx06i0dP8dCxeDB39wmIRZBq/hf_20261003_085931_adc061ad-21c5-44e1-82f8-3adcdde5f97d.png" "ARMORED.png"
download_cdn "https://d8j0ntlcm91z4.cloudfront.net/user_3JGQx06i0dP8dCxeDB39wmIRZBq/hf_20261003_090139_e02ac308-d8b7-4a9a-b3fa-5dea6292362f.png" "HOME_TREE.png"

echo "Higgsfield production assets ready:"
ls -lh "$DEST"
