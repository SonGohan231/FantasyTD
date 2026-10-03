#!/usr/bin/env python3
"""Import only approved pre-existing media, with byte identity and stable Unity GUIDs."""
import argparse
import hashlib
import io
import json
import os
from pathlib import Path
import urllib.request
import zipfile

RUNTIME_COPIES = {
    11227178308: 'MAP2_TERRAIN.png',
    11227420838: 'MAP2_ROAD_KIT.png',
}


class SourceRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, request, fp, code, msg, headers, new_url):
        redirected = super().redirect_request(request, fp, code, msg, headers, new_url)
        # The GitHub API signs the download URL. Its bearer token is not Blob credentials.
        if redirected is not None:
            redirected.remove_header('Authorization')
        return redirected


def digest(data):
    return hashlib.sha256(data).hexdigest()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--local-index', help='Validate preserved ZIPs without network or writes')
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[2]
    manifest = json.loads((root / 'ArtSource/Higgsfield/approved-artifacts.json').read_text())
    local = None
    if args.local_index:
        local = {a['id']: a['archive'] for a in json.loads(Path(args.local_index).read_text())['artifacts']}
    for row in manifest['artifacts']:
        artifact_id = row['artifactId']
        if local:
            archive = Path(local[artifact_id]).read_bytes()
        else:
            token = os.environ['GH_TOKEN']
            repo = os.environ['GITHUB_REPOSITORY']
            request = urllib.request.Request(
                f'https://api.github.com/repos/{repo}/actions/artifacts/{artifact_id}/zip',
                headers={'Authorization': f'Bearer {token}', 'Accept': 'application/vnd.github+json'})
            with urllib.request.build_opener(SourceRedirect()).open(request, timeout=90) as response:
                archive = response.read()
        if digest(archive) != row['archiveSha256']:
            raise ValueError(f'Archive identity mismatch: {artifact_id}')
        with zipfile.ZipFile(io.BytesIO(archive)) as zipped:
            if zipped.testzip() is not None:
                raise ValueError(f'ZIP CRC failed: {artifact_id}')
            for member in row['members']:
                name = member['name']
                if Path(name).name != name:
                    raise ValueError(f'Unsafe source path: {name}')
                data = zipped.read(name)
                if len(data) != member['bytes'] or digest(data) != member['sha256']:
                    raise ValueError(f'Member identity mismatch: {artifact_id}/{name}')
                if local:
                    continue
                is_media = name.startswith('asset_') and Path(name).suffix in ('.png', '.mp4')
                relative = (f'UnityProject/Assets/FantasyTD/Art/ApprovedSources/{artifact_id}/{name}'
                            if is_media else f'ArtSource/Higgsfield/{artifact_id}/{name}')
                target = root / relative
                target.parent.mkdir(parents=True, exist_ok=True)
                target.write_bytes(data)
                if is_media:
                    # Stable GUID per approved source identity, independent of CI checkout/import order.
                    guid = hashlib.md5(('FantasyTD-approved:' + relative).encode()).hexdigest()
                    meta = target.with_name(target.name + '.meta')
                    if not meta.exists():
                        meta.write_text(f'fileFormatVersion: 2\nguid: {guid}\n')
                    if artifact_id in RUNTIME_COPIES and name == 'asset_01.png':
                        runtime_relative = 'UnityProject/Assets/FantasyTD/Resources/Higgsfield/Maps/' + RUNTIME_COPIES[artifact_id]
                        runtime_target = root / runtime_relative
                        runtime_target.parent.mkdir(parents=True, exist_ok=True)
                        runtime_target.write_bytes(data)
                        runtime_meta = runtime_target.with_name(runtime_target.name + '.meta')
                        if not runtime_meta.exists():
                            runtime_guid = hashlib.md5(('FantasyTD-approved:' + runtime_relative).encode()).hexdigest()
                            runtime_meta.write_text(f'fileFormatVersion: 2\nguid: {runtime_guid}\n')
        print(f'Verified approved artifact {artifact_id}')
    print(f"Verified {len(manifest['artifacts'])} unique approved source archives; no generation performed")


if __name__ == '__main__':
    main()
