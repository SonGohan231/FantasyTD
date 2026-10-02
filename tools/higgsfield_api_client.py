#!/usr/bin/env python3
import json
import mimetypes
import os
import pathlib
import sys
import time
import urllib.error
import urllib.parse
import urllib.request

API_BASE = "https://api.higgsfield.ai"
ALLOWED_MODELS = {
    "higgsfield-ai/soul/v2/standard": "image",
    "z-image/turbo": "image",
    "recraft/v4.1/text-to-image": "image",
    "bytedance/seedance-2.5/text-to-video": "video",
    "bytedance/seedance-2.5/image-to-video": "video",
}
TERMINAL = {"completed", "failed", "nsfw", "canceled"}

def die(message):
    print(f"ERROR: {message}", file=sys.stderr)
    raise SystemExit(1)

def http_json(method, url, api_key, body=None):
    headers = {"Authorization": f"Key {api_key}", "Content-Type": "application/json"}
    data = None if body is None else json.dumps(body).encode("utf-8")
    request = urllib.request.Request(url, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(request, timeout=60) as response:
            payload = response.read().decode("utf-8")
            return json.loads(payload)
    except urllib.error.HTTPError as exc:
        body_text = exc.read().decode("utf-8", errors="replace")[:2000]
        die(f"Higgsfield HTTP {exc.code}: {body_text}")
    except urllib.error.URLError as exc:
        die(f"Higgsfield network error: {exc.reason}")

def validate(cfg):
    if cfg.get("enabled") is not True:
        return False
    generation_id = cfg.get("generation_id")
    if not isinstance(generation_id, str) or len(generation_id.strip()) < 8:
        die("generation_id must be a unique string with at least 8 characters")
    model_path = cfg.get("model_path")
    if model_path not in ALLOWED_MODELS:
        die(f"model_path not allowed: {model_path}")
    payload = cfg.get("input")
    if not isinstance(payload, dict):
        die("input must be a JSON object")
    prompt = payload.get("prompt")
    if prompt is not None and (not isinstance(prompt, str) or not prompt.strip()):
        die("prompt must be a non-empty string when supplied")
    if model_path.endswith("text-to-video"):
        duration = int(payload.get("duration", 5))
        if duration < 4 or duration > 8:
            die("video duration is limited to 4-8 seconds in this bridge")
        if payload.get("resolution", "720p") not in {"480p", "720p"}:
            die("video resolution must be 480p or 720p")
        if payload.get("generate_audio", False) not in {False, 0}:
            die("generate_audio must be false for the game-asset bridge")
    if model_path.endswith("image-to-video"):
        image_url = payload.get("image_url")
        if not isinstance(image_url, str) or not image_url.startswith("https://"):
            die("image-to-video requires an https image_url")
        duration = int(payload.get("duration", 5))
        if duration < 4 or duration > 8:
            die("video duration is limited to 4-8 seconds in this bridge")
        if payload.get("resolution", "720p") not in {"480p", "720p"}:
            die("video resolution must be 480p or 720p")
        if payload.get("generate_audio", False) not in {False, 0}:
            die("generate_audio must be false for the game-asset bridge")
    if model_path == "higgsfield-ai/soul/v2/standard":
        if int(payload.get("batch_size", 1)) != 1:
            die("Soul 2 batch_size is limited to 1 in this bridge")
    return True

def media_urls(payload):
    urls = []
    def collect(obj):
        if isinstance(obj, dict):
            images = obj.get("images")
            if isinstance(images, list):
                for item in images:
                    if isinstance(item, dict) and isinstance(item.get("url"), str):
                        urls.append(item["url"])
            for key in ("video", "audio", "file"):
                item = obj.get(key)
                if isinstance(item, dict) and isinstance(item.get("url"), str):
                    urls.append(item["url"])
                elif isinstance(item, str) and item.startswith("http"):
                    urls.append(item)
            for key in ("result", "output", "data"):
                if key in obj:
                    collect(obj[key])
        elif isinstance(obj, list):
            for item in obj:
                collect(item)
    collect(payload)
    seen = set()
    return [u for u in urls if not (u in seen or seen.add(u))]

def download(url, target_dir, index):
    parsed = urllib.parse.urlparse(url)
    ext = pathlib.Path(parsed.path).suffix
    if not ext or len(ext) > 8:
        ext = ".bin"
    request = urllib.request.Request(url, method="GET")
    with urllib.request.urlopen(request, timeout=120) as response:
        ctype = response.headers.get_content_type()
        guessed = mimetypes.guess_extension(ctype) if ctype else None
        if ext == ".bin" and guessed:
            ext = guessed
        path = target_dir / f"asset_{index:02d}{ext}"
        with open(path, "wb") as f:
            while True:
                chunk = response.read(1024 * 1024)
                if not chunk:
                    break
                f.write(chunk)
    return path

def main():
    request_path = pathlib.Path(sys.argv[1] if len(sys.argv) > 1 else ".higgsfield/request.json")
    out_dir = pathlib.Path(os.environ.get("HF_OUTPUT_DIR", "higgsfield-output"))
    out_dir.mkdir(parents=True, exist_ok=True)

    cfg = json.loads(request_path.read_text(encoding="utf-8"))
    if not validate(cfg):
        print("Higgsfield request disabled; no paid API call was made.")
        (out_dir / "disabled.txt").write_text("enabled=false\n", encoding="utf-8")
        return

    api_key = os.environ.get("HF_API_KEY", "").strip()
    if not api_key:
        die("HF_API_KEY secret is not configured")

    model_path = cfg["model_path"]
    submit_url = f"{API_BASE}/{model_path}"
    print(f"Submitting generation_id={cfg['generation_id']} model={model_path}")
    submitted = http_json("POST", submit_url, api_key, cfg["input"])

    request_id = submitted.get("request_id")
    if not isinstance(request_id, str) or not request_id:
        die("Higgsfield response did not contain request_id")
    status_url = submitted.get("status_url")
    if not isinstance(status_url, str) or not status_url.startswith(API_BASE):
        status_url = f"{API_BASE}/requests/{request_id}/status"

    safe_submit = {k: v for k, v in submitted.items() if k not in {"api_key", "authorization"}}
    (out_dir / "submit.json").write_text(json.dumps(safe_submit, indent=2), encoding="utf-8")

    timeout = int(cfg.get("max_wait_seconds", 1200))
    timeout = max(60, min(timeout, 1800))
    deadline = time.time() + timeout
    final = None

    while time.time() < deadline:
        status_payload = http_json("GET", status_url, api_key)
        status = str(status_payload.get("status", "")).lower()
        print(f"request_id={request_id} status={status or 'unknown'}")
        if status in TERMINAL:
            final = status_payload
            break
        time.sleep(5)

    if final is None:
        die(f"Timed out after {timeout}s while waiting for request_id={request_id}")

    (out_dir / "final.json").write_text(json.dumps(final, indent=2), encoding="utf-8")
    status = str(final.get("status", "")).lower()
    if status != "completed":
        die(f"Generation finished with terminal status={status}")

    urls = media_urls(final)
    if not urls:
        die("Generation completed but no media URL was found in the status payload")

    downloaded = []
    for i, url in enumerate(urls, 1):
        path = download(url, out_dir, i)
        downloaded.append(path.name)
        print(f"Downloaded {path.name}")

    manifest = {
        "generation_id": cfg["generation_id"],
        "model_path": model_path,
        "request_id": request_id,
        "status": status,
        "files": downloaded,
    }
    (out_dir / "manifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")

if __name__ == "__main__":
    main()
