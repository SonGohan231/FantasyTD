#!/usr/bin/env bash
set -euo pipefail

APK_ROOT="${1:?downloaded APK artifact directory required}"
OUT="${2:?evidence directory required}"
EXPECTED_SHA="49dc6b891baed50d8e91a39e23404b3dbf93c396f50c32cd88bc6706d9ebd55e"
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
report "sourceRun=37124504578"
report "sourceArtifact=fantasy-td-android-37124504578"
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
sleep 15
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

capture() {
  local step="$1"
  local file="$OUT/screenshots/$step.png"
  "${ADB[@]}" exec-out screencap -p > "$file"
  python3 - "$file" <<'PY'
import struct,sys
with open(sys.argv[1],"rb") as f:
    data=f.read(24)
assert data[:8] == b"\x89PNG\r\n\x1a\n" and data[12:16] == b"IHDR", "Invalid screenshot"
w,h=struct.unpack(">II",data[16:24])
assert w>h, "App did not render in landscape"
print(w,h)
PY
  report "screenshot=$step.png"
}
read W H < <(capture main-menu)
report "resolution=${W}x${H}"
tap() {
  local x="$1" y="$2"
  "${ADB[@]}" shell input tap "$((W*x/1000))" "$((H*y/1000))"
}
alive() {
  local pid
  pid="$("${ADB[@]}" shell pidof "$PACKAGE" | tr -d '\r')"
  [[ -n "$pid" ]] || fail "App process exited after $1"
  report "aliveAfter$1=True"
}
alive MainMenu
MENU_SHA="$(sha256sum "$OUT/screenshots/main-menu.png" | awk '{print $1}')"
tap 500 700
sleep 5
capture campaign
CAMPAIGN_SHA="$(sha256sum "$OUT/screenshots/campaign.png" | awk '{print $1}')"
[[ "$CAMPAIGN_SHA" != "$MENU_SHA" ]] || fail "Menu tap did not change the rendered screen"
alive Campaign
tap 170 360
sleep 8
capture map1
MAP_SHA="$(sha256sum "$OUT/screenshots/map1.png" | awk '{print $1}')"
[[ "$MAP_SHA" != "$CAMPAIGN_SHA" ]] || fail "MAP1 selection did not change the rendered screen"
alive Map1
tap 500 500
sleep 2
capture planting-menu
tap 435 500
sleep 3
capture planted
tap 100 920
sleep 8
capture wave
alive Wave
"${ADB[@]}" logcat -d -v threadtime > "$LOG"
grep -Ei "FATAL EXCEPTION|ANR in $PACKAGE|am_crash.*$PACKAGE|Process: $PACKAGE|Unity.*(Exception|Error)" "$LOG" > "$OUT/logcat-filtered.txt" || true
if [[ -s "$OUT/logcat-filtered.txt" ]]; then
  fail "Fatal exception, ANR, or Unity error found in logcat; inspect filtered log"
fi
report "status=AUTOMATION_COMPLETE_PENDING_VISUAL_REVIEW"
