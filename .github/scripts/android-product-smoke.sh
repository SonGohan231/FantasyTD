#!/usr/bin/env bash
set -euo pipefail

APK_ROOT="${1:?downloaded APK artifact directory required}"
OUT="${2:?evidence directory required}"
EXPECTED_SHA="de77755b516d493dd036259902a2c7913213eaad72064962c3c2e208669dc06c"
PACKAGE="com.indie.fantasytd"
mkdir -p "$OUT/screenshots"
REPORT="$OUT/android-product-smoke.txt"
LOG="$OUT/logcat.txt"
: > "$REPORT"
report() { printf '%s\n' "$*" >> "$REPORT"; }
fail() { report "status=FAIL"; report "reason=$1"; exit 1; }

APK="$(find "$APK_ROOT" -type f -name 'FantasyTD_MAP_A_v2.apk' -print -quit)"
[[ -n "$APK" ]] || fail "APK missing from candidate artifact"
ACTUAL_SHA="$(sha256sum "$APK" | awk '{print $1}')"
report "sourceRun=37196919539"
report "sourceArtifact=fantasy-td-android-37196919539"
report "expectedSha256=$EXPECTED_SHA"
report "actualSha256=$ACTUAL_SHA"
[[ "$ACTUAL_SHA" == "$EXPECTED_SHA" ]] || fail "APK identity mismatch"
report "apkIdentityVerified=True"

SERIAL="$(adb devices | awk '$1 ~ /^emulator-/ && $2=="device" {print $1; exit}')"
[[ -n "$SERIAL" ]] || fail "No booted emulator"
ADB=(adb -s "$SERIAL")
report "serial=$SERIAL"
report "androidApi=$("${ADB[@]}" shell getprop ro.build.version.sdk | tr -d '\r')"
report "abi=$("${ADB[@]}" shell getprop ro.product.cpu.abi | tr -d '\r')"
report "model=$("${ADB[@]}" shell getprop ro.product.model | tr -d '\r')"
"${ADB[@]}" shell wm size > "$OUT/display.txt"
"${ADB[@]}" install -r "$APK" > "$OUT/adb-install.txt" 2>&1 || fail "adb install failed"
"${ADB[@]}" shell pm path "$PACKAGE" | grep -q '^package:' || fail "Package not installed"
report "packageInstalled=True"
"${ADB[@]}" shell settings put secure immersive_mode_confirmations confirmed || true
"${ADB[@]}" logcat -c
"${ADB[@]}" shell am force-stop "$PACKAGE" || true
"${ADB[@]}" shell monkey -p "$PACKAGE" -c android.intent.category.LAUNCHER 1 > "$OUT/launch.txt" 2>&1 || fail "Launcher failed"
sleep 5
# Fresh emulator profiles show an Android immersive-mode education overlay.
# Dismiss only when that system dialog is present; otherwise leave the game untouched.
"${ADB[@]}" shell uiautomator dump /sdcard/fantasytd-window.xml >/dev/null 2>&1 || true
"${ADB[@]}" shell cat /sdcard/fantasytd-window.xml > "$OUT/window-before.xml" 2>/dev/null || true
if grep -q 'Viewing full screen' "$OUT/window-before.xml"; then
  read TARGET_X TARGET_Y < <(python3 - "$OUT/window-before.xml" <<'PY'
import re,sys,xml.etree.ElementTree as ET
root=ET.parse(sys.argv[1]).getroot()
for n in root.iter():
    if n.attrib.get("text") == "Got it":
        b=list(map(int,re.findall(r"\d+",n.attrib["bounds"])))
        print((b[0]+b[2])//2,(b[1]+b[3])//2)
        break
PY
)
  if [[ -n "${TARGET_X:-}" ]]; then
    "${ADB[@]}" shell input tap "$TARGET_X" "$TARGET_Y"
    sleep 5
    report "immersiveOverlayDismissed=True"
  fi
fi
"${ADB[@]}" shell uiautomator dump /sdcard/fantasytd-window-after.xml >/dev/null 2>&1 || true
"${ADB[@]}" shell cat /sdcard/fantasytd-window-after.xml > "$OUT/window-after.xml" 2>/dev/null || true
if grep -q 'Viewing full screen' "$OUT/window-after.xml"; then
  fail "Android immersive-mode system dialog still blocks the game"
fi

capture_raw() {
  local file="$1" dims
  for _ in $(seq 1 10); do
    if "${ADB[@]}" exec-out screencap -p > "$file" 2>/dev/null \
      && dims="$(python3 - "$file" 2>/dev/null <<'PY'
import struct,sys
with open(sys.argv[1],"rb") as f:
    data=f.read(24)
assert data[:8] == b"\\x89PNG\\r\\n\\x1a\\n" and data[12:16] == b"IHDR", "Invalid screenshot"
w,h=struct.unpack(">II",data[16:24])
assert w>h, "App did not render in landscape"
print(w,h)
PY
)"; then
      printf '%s\n' "$dims"
      return 0
    fi
    sleep 1
  done
  fail "Could not capture a valid landscape PNG from the emulator"
}
capture() {
  local step="$1"
  local file="$OUT/screenshots/$step.png"
  capture_raw "$file"
  report "screenshot=$step.png"
}
wait_for_stable_change() {
  local step="$1" baseline_sha="$2" description="$3"
  local target="$OUT/screenshots/$step.png"
  local candidate="$OUT/screenshots/.$step-candidate.png"
  local last_sha="" stable_count=0 current_sha
  for _ in $(seq 1 30); do
    sleep 2
    capture_raw "$candidate" >/dev/null
    current_sha="$(sha256sum "$candidate" | awk '{print $1}')"
    if [[ "$current_sha" != "$baseline_sha" ]]; then
      if [[ "$current_sha" == "$last_sha" ]]; then
        stable_count=$((stable_count + 1))
      else
        stable_count=0
      fi
      if (( stable_count >= 1 )); then
        mv "$candidate" "$target"
        report "screenshot=$step.png"
        report "stableAfter=$description"
        return 0
      fi
    fi
    last_sha="$current_sha"
  done
  rm -f "$candidate"
  fail "$description did not reach a changed stable frame within 60 seconds"
}
read W H < <(capture main-menu)
report "resolution=${W}x${H}"
# Unity can accept input before the first fully rendered frame on a cold
# emulator. Never start the flow from the initial flat/black surface.
if (( $(stat -c%s "$OUT/screenshots/main-menu.png") < 100000 )); then
  BOOT_SHA="$(sha256sum "$OUT/screenshots/main-menu.png" | awk '{print $1}')"
  wait_for_stable_change main-menu "$BOOT_SHA" "Main menu render"
fi
tap() {
  local x="$1" y="$2"
  "${ADB[@]}" shell input tap "$((W*x/1000))" "$((H*y/1000))"
}
alive() {
  local pid
  pid="$("${ADB[@]}" shell pidof "$PACKAGE" | tr -d '\\r')"
  [[ -n "$pid" ]] || fail "App process exited after $1"
  report "aliveAfter$1=True"
}
alive MainMenu
MENU_SHA="$(sha256sum "$OUT/screenshots/main-menu.png" | awk '{print $1}')"
tap 500 700
wait_for_stable_change campaign "$MENU_SHA" "Campaign navigation"
CAMPAIGN_SHA="$(sha256sum "$OUT/screenshots/campaign.png" | awk '{print $1}')"
alive Campaign
tap 170 360
wait_for_stable_change map1 "$CAMPAIGN_SHA" "MAP1 selection"
MAP_SHA="$(sha256sum "$OUT/screenshots/map1.png" | awk '{print $1}')"
alive Map1
# Use authored MAP1 pad A3. On the current full-bleed/HUD-safe camera this projects to roughly 48.7%, 24.4% of the Pixel 7 Pro landscape framebuffer.
tap 487 244
wait_for_stable_change planting-menu "$MAP_SHA" "Planting spot selection"
MENU_PLANT_SHA="$(sha256sum "$OUT/screenshots/planting-menu.png" | awk '{print $1}')"
# Seed Shooter radial option is 125 reference UI units left of A3 (about 5.9% of this framebuffer width).
tap 428 244
wait_for_stable_change planted "$MENU_PLANT_SHA" "Tower placement"
PLANTED_SHA="$(sha256sum "$OUT/screenshots/planted.png" | awk '{print $1}')"
tap 100 920
wait_for_stable_change wave "$PLANTED_SHA" "Wave start"
WAVE_SHA="$(sha256sum "$OUT/screenshots/wave.png" | awk '{print $1}')"
alive Wave
"${ADB[@]}" logcat -d -v threadtime > "$LOG"
grep -Ei "FATAL EXCEPTION|ANR in $PACKAGE|am_crash.*$PACKAGE|Process: $PACKAGE|Unity.*(Exception|Error)" "$LOG" > "$OUT/logcat-filtered.txt" || true
if [[ -s "$OUT/logcat-filtered.txt" ]]; then
  fail "Fatal exception, ANR, or Unity error found in logcat; inspect filtered log"
fi
report "status=AUTOMATION_COMPLETE_PENDING_VISUAL_REVIEW"
