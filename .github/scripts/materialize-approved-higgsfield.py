#!/usr/bin/env python3
"""Import existing approved media and explicitly unapproved recovered candidates, with verified byte identity."""
import argparse
import hashlib
import io
import json
import os
import subprocess
from pathlib import Path
import urllib.request
import zipfile

RUNTIME_COPIES = {
    11223032330: 'MAP7_RECOVERED_CONCEPT.png',
    11222684271: 'MAP8_RECOVERED_CONCEPT.png',
    11219593382: 'MAP1_ROAD_SOURCE.png',
    11233790855: 'MAP3_TERRAIN.png',
    11233805827: 'MAP3_GATE.png',
    11232874052: 'MAP4_TERRAIN.png',
    11234470479: 'MAP4_PROPS.png',
    11233644218: 'MAP4_SHRINE_CLOSED.png',
    11234691977: 'MAP5_TERRAIN.png',
    11234731952: 'MAP5_PROPS.png',
    11235542085: 'MAP6_TERRAIN.png',
    11235063190: 'MAP6_PROPS.png',
    11235466241: 'MAP5_LAB_CLOSED.png',
    11237770536: 'MAP5_MOTHER_DORMANT.png',
    11237423117: 'MAP5_MOTHER_ACTIVE.png',
    11235363386: 'MAP5_MUTATION_TOWER.png',
    11227178308: 'MAP2_TERRAIN.png',
    11227420838: 'MAP2_ROAD_KIT.png',
}


RUNTIME_VIDEO_FRAMES = {
    11234759621: 'MAP5_LAB_OPEN_FINAL.png',
    11234391273: 'MAP4_SHRINE_OPEN_FINAL.png',
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
    recovered = []
    for candidate_path in sorted((root / 'ArtSource/Higgsfield/RecoveredCandidates').glob('*.json')):
        candidate = json.loads(candidate_path.read_text())
        row = dict(candidate)
        row['members'] = [dict(m, name=m['path']) for m in candidate['members']]
        row['recoveredCandidate'] = True
        recovered.append(row)
    for row in manifest['artifacts'] + recovered:
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
                            if is_media and not row.get('recoveredCandidate') else f'ArtSource/Higgsfield/{artifact_id}/{name}')
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
                if name == 'asset_01.mp4' and artifact_id in RUNTIME_VIDEO_FRAMES:
                    frame_relative = 'UnityProject/Assets/FantasyTD/Resources/Higgsfield/Maps/' + RUNTIME_VIDEO_FRAMES[artifact_id]
                    frame_target = root / frame_relative
                    frame_target.parent.mkdir(parents=True, exist_ok=True)
                    subprocess.run(['ffmpeg', '-hide_banner', '-loglevel', 'error', '-y',
                        '-sseof', '-0.05', '-i', str(target), '-frames:v', '1', str(frame_target)], check=True)
                    frame_guid = hashlib.md5(('FantasyTD-approved-derived:' + frame_relative).encode()).hexdigest()
                    frame_target.with_name(frame_target.name + '.meta').write_text(
                        f'fileFormatVersion: 2\nguid: {frame_guid}\n')
                    provenance = root / 'ArtSource/Higgsfield' / str(artifact_id) / 'runtime-frame-derivation.json'
                    provenance.write_text(json.dumps({
                        'sourceArchiveSha256': row['archiveSha256'], 'sourceMemberSha256': member['sha256'],
                        'derivation': 'ffmpeg final frame; -sseof -0.05; no authored pixels generated',
                        'runtimePath': frame_relative, 'runtimeSha256': digest(frame_target.read_bytes()),
                        'frameCount': 1, 'isFullAnimation': False
                    }, indent=2) + '\n')
        print(f"Verified {'recovered candidate' if row.get('recoveredCandidate') else 'approved'} artifact {artifact_id}")
    print(f"Verified {len(manifest['artifacts'])} unique approved source archives; no generation performed")


if __name__ == '__main__':
    main()

