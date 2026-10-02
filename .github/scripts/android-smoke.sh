#!/usr/bin/env bash
set -uo pipefail

EXPECTED_SHA="48070beb0826ef5b7d72f16512266d4246a2a4ce341fc2acc4a01feb0c4a69e9"
PACKAGE="com.indie.fantasytd"
ARTIFACT_ROOT="${1:?artifact root required}"
OUT_DIR="${2:?output dir required}"
mkdir -p "$OUT_DIR/screenshots"
REPORT="$OUT_DIR/android-smoke.txt"
PROBE="$OUT_DIR/runtime-probe.txt"
INSTALL_LOG="$OUT_DIR/adb-install.txt"
LAUNCH_LOG="$OUT_DIR/launch.txt"
LOGCAT="$OUT_DIR/logcat.txt"
LOGCAT_FILTERED="$OUT_DIR/logcat-filtered.txt"
EMULATOR_LOG="$OUT_DIR/emulator.txt"
: > "$REPORT"
: > "$PROBE"
: > "$LOGCAT_FILTERED"

EMULATOR_PID=""
LOGCAT_PID=""
SERIAL=""
RUNTIME_TYPE=""
BLOCKED_REASON=""
FAIL_REASON=""

cleanup() {
  if [[ -n "$LOGCAT_PID" ]]; then
    kill "$LOGCAT_PID" >/dev/null 2>&1 || true
    wait "$LOGCAT_PID" >/dev/null 2>&1 || true
  fi
  if [[ -n "$EMULATOR_PID" ]]; then
    kill "$EMULATOR_PID" >/dev/null 2>&1 || true
    wait "$EMULATOR_PID" >/dev/null 2>&1 || true
  fi
}
trap cleanup EXIT

report() {
  printf '%s\n' "$*" >> "$REPORT"
}

block() {
  BLOCKED_REASON="$1"
  report "status=BLOCKED"
  report "blockedReason=$BLOCKED_REASON"
  exit 2
}

fail() {
  FAIL_REASON="$1"
  report "status=FAIL"
  report "failureReason=$FAIL_REASON"
  exit 1
}

APK="$(find "$ARTIFACT_ROOT" -type f -name 'FantasyTD_MAP_A_v2.apk' -print | head -n 1)"
if [[ -z "$APK" || ! -f "$APK" ]]; then
  report "expectedSha256=$EXPECTED_SHA"
  block "Exact T0008 APK was not found in downloaded artifact."
fi

ACTUAL_SHA="$(sha256sum "$APK" | awk '{print $1}')"
APK_BYTES="$(stat -c '%s' "$APK")"
report "sourceArtifactRun=37006999780"
report "sourceArtifactName=fantasy-td-android-37006999780"
report "apkResolvedPath=$APK"
report "expectedSha256=$EXPECTED_SHA"
report "actualSha256=$ACTUAL_SHA"
report "apkBytes=$APK_BYTES"
if [[ "$ACTUAL_SHA" != "$EXPECTED_SHA" ]]; then
  block "T0008 APK SHA-256 mismatch."
fi
report "apkIdentityVerified=True"

UNITY_BIN="$(find "$HOME" /opt -type f -path '*/6000.0.26f1/Editor/Unity' -perm -111 2>/dev/null | head -n 1)"
if [[ -n "$UNITY_BIN" ]]; then
  SDK_ROOT="$(dirname "$UNITY_BIN")/Data/PlaybackEngines/AndroidPlayer/SDK"
else
  SDK_ROOT=""
fi
ADB=""
if [[ -n "$SDK_ROOT" && -x "$SDK_ROOT/platform-tools/adb" ]]; then
  ADB="$SDK_ROOT/platform-tools/adb"
elif command -v adb >/dev/null 2>&1; then
  ADB="$(command -v adb)"
  SDK_ROOT="$(cd "$(dirname "$ADB")/.." && pwd)"
else
  block "ADB is unavailable on the self-hosted runner."
fi

EMULATOR="$SDK_ROOT/emulator/emulator"
SDKMANAGER="$(find "$SDK_ROOT" -type f -name sdkmanager -perm -111 2>/dev/null | head -n 1)"
AVDMANAGER="$(find "$SDK_ROOT" -type f -name avdmanager -perm -111 2>/dev/null | head -n 1)"
export ANDROID_SDK_ROOT="$SDK_ROOT"
export ANDROID_HOME="$SDK_ROOT"

{
  echo "sdkRoot=$SDK_ROOT"
  echo "adb=$ADB"
  "$ADB" version || true
  echo "emulator=$EMULATOR"
  [[ -x "$EMULATOR" ]] && "$EMULATOR" -version 2>&1 | head -n 8 || true
  echo "sdkmanager=$SDKMANAGER"
  echo "avdmanager=$AVDMANAGER"
  echo "kvmExists=$([[ -e /dev/kvm ]] && echo True || echo False)"
  echo "kvmWritable=$([[ -w /dev/kvm ]] && echo True || echo False)"
  echo "devicesBefore:"
  "$ADB" devices -l || true
  echo "avdsBefore:"
  [[ -x "$EMULATOR" ]] && "$EMULATOR" -list-avds || true
  echo "installedSystemImages:"
  find "$SDK_ROOT/system-images" -maxdepth 4 -type d -name 'x86_64' -print 2>/dev/null || true
} >> "$PROBE" 2>&1

"$ADB" start-server >/dev/null 2>&1 || true

existing_emulator="$("$ADB" devices | awk '$1 ~ /^emulator-/ && $2=="device" {print $1; exit}')"
if [[ -n "$existing_emulator" ]]; then
  SERIAL="$existing_emulator"
  RUNTIME_TYPE="emulator-existing"
fi

if [[ -z "$SERIAL" ]]; then
  if [[ ! -x "$EMULATOR" && -n "$SDKMANAGER" ]]; then
    {
      echo "Installing emulator package into Unity-reported SDK root..."
      yes | "$SDKMANAGER" --sdk_root="$SDK_ROOT" "emulator"
    } >> "$PROBE" 2>&1 || true
  fi

  SYSTEM_IMAGE=""
  installed_image="$(find "$SDK_ROOT/system-images" -maxdepth 4 -type d -path '*/google_apis/x86_64' -print 2>/dev/null | sort -V | tail -n 1)"
  if [[ -n "$installed_image" ]]; then
    rel="${installed_image#"$SDK_ROOT/system-images/"}"
    api="${rel%%/*}"
    SYSTEM_IMAGE="system-images;${api};google_apis;x86_64"
  elif [[ -n "$SDKMANAGER" ]]; then
    SYSTEM_IMAGE="system-images;android-35;google_apis;x86_64"
    {
      echo "Installing $SYSTEM_IMAGE into Unity-reported SDK root..."
      yes | "$SDKMANAGER" --sdk_root="$SDK_ROOT" "$SYSTEM_IMAGE"
    } >> "$PROBE" 2>&1 || SYSTEM_IMAGE=""
  fi

  if [[ -x "$EMULATOR" && -n "$AVDMANAGER" && -n "$SYSTEM_IMAGE" ]]; then
    AVD_NAME="fantasytd_t0009_${GITHUB_RUN_ID:-local}"
    {
      echo "Creating AVD $AVD_NAME using $SYSTEM_IMAGE"
      echo "no" | "$AVDMANAGER" create avd -n "$AVD_NAME" -k "$SYSTEM_IMAGE" --device "pixel_6" --force
    } >> "$PROBE" 2>&1 || true

    ACCEL_ARGS=()
    if [[ ! -w /dev/kvm ]]; then
      ACCEL_ARGS=(-accel off)
    fi

    "$EMULATOR" -avd "$AVD_NAME" -no-window -no-audio -no-boot-anim -no-snapshot -wipe-data       -gpu swiftshader_indirect "${ACCEL_ARGS[@]}" > "$EMULATOR_LOG" 2>&1 &
    EMULATOR_PID="$!"

    for _ in $(seq 1 180); do
      SERIAL="$("$ADB" devices | awk '$1 ~ /^emulator-/ && $2=="device" {print $1; exit}')"
      [[ -n "$SERIAL" ]] && break
      if ! kill -0 "$EMULATOR_PID" >/dev/null 2>&1; then
        SERIAL=""
        break
      fi
      sleep 5
    done

    if [[ -n "$SERIAL" ]]; then
      booted=""
      for _ in $(seq 1 180); do
        booted="$("$ADB" -s "$SERIAL" shell getprop sys.boot_completed 2>/dev/null | tr -d '\r')"
        [[ "$booted" == "1" ]] && break
        sleep 5
      done
      if [[ "$booted" == "1" ]]; then
        RUNTIME_TYPE="emulator-created"
      else
        SERIAL=""
      fi
    fi
  fi
fi

if [[ -z "$SERIAL" ]]; then
  physical="$("$ADB" devices | awk '$1 !~ /^emulator-/ && $2=="device" {print $1; exit}')"
  if [[ -n "$physical" ]]; then
    SERIAL="$physical"
    RUNTIME_TYPE="physical-device"
  fi
fi

if [[ -z "$SERIAL" ]]; then
  block "No bootable Android emulator and no authorized physical ADB device were available."
fi

ANDROID_RELEASE="$("$ADB" -s "$SERIAL" shell getprop ro.build.version.release | tr -d '\r')"
ANDROID_API="$("$ADB" -s "$SERIAL" shell getprop ro.build.version.sdk | tr -d '\r')"
ARCH="$("$ADB" -s "$SERIAL" shell getprop ro.product.cpu.abi | tr -d '\r')"
MODEL="$("$ADB" -s "$SERIAL" shell getprop ro.product.model | tr -d '\r')"
WM_SIZE="$("$ADB" -s "$SERIAL" shell wm size | tr '\n' ';' | tr -d '\r')"

report "runtimeType=$RUNTIME_TYPE"
report "adbSerial=$SERIAL"
report "androidVersion=$ANDROID_RELEASE"
report "androidApi=$ANDROID_API"
report "architecture=$ARCH"
report "model=$MODEL"
report "wmSize=$WM_SIZE"

"$ADB" -s "$SERIAL" shell settings put global window_animation_scale 0 >/dev/null 2>&1 || true
"$ADB" -s "$SERIAL" shell settings put global transition_animation_scale 0 >/dev/null 2>&1 || true
"$ADB" -s "$SERIAL" shell settings put global animator_duration_scale 0 >/dev/null 2>&1 || true

set +e
"$ADB" -s "$SERIAL" install -r "$APK" > "$INSTALL_LOG" 2>&1
INSTALL_RC=$?
set -e
report "adbInstallExitCode=$INSTALL_RC"
report "adbInstallResult=$(tr '\n' ' ' < "$INSTALL_LOG" | sed 's/[[:space:]]\+/ /g')"
if [[ $INSTALL_RC -ne 0 ]]; then
  fail "adb install failed."
fi

PACKAGE_PATH="$("$ADB" -s "$SERIAL" shell pm path "$PACKAGE" 2>/dev/null | tr -d '\r')"
report "packagePath=$PACKAGE_PATH"
if [[ "$PACKAGE_PATH" != package:* ]]; then
  fail "Package $PACKAGE is not installed after adb install."
fi
report "packageInstalled=True"

"$ADB" -s "$SERIAL" logcat -c || true
"$ADB" -s "$SERIAL" logcat -v threadtime > "$LOGCAT" 2>&1 &
LOGCAT_PID="$!"

"$ADB" -s "$SERIAL" shell am force-stop "$PACKAGE" >/dev/null 2>&1 || true
set +e
"$ADB" -s "$SERIAL" shell monkey -p "$PACKAGE" -c android.intent.category.LAUNCHER 1 > "$LAUNCH_LOG" 2>&1
LAUNCH_RC=$?
set -e
report "launchExitCode=$LAUNCH_RC"
report "launchCommandResult=$(tr '\n' ' ' < "$LAUNCH_LOG" | sed 's/[[:space:]]\+/ /g')"
if [[ $LAUNCH_RC -ne 0 ]]; then
  fail "Launcher command failed."
fi

sleep 10
PID_START="$("$ADB" -s "$SERIAL" shell pidof "$PACKAGE" 2>/dev/null | tr -d '\r')"
FOREGROUND_START="$("$ADB" -s "$SERIAL" shell dumpsys activity activities 2>/dev/null | grep -m1 'mResumedActivity' | tr -d '\r')"
report "pidAfterLaunch=$PID_START"
report "foregroundAfterLaunch=$FOREGROUND_START"
if [[ -z "$PID_START" ]]; then
  fail "Package process is not alive after startup."
fi
if [[ "$FOREGROUND_START" != *"$PACKAGE"* ]]; then
  fail "Package is not the resumed foreground activity after launch."
fi

screenshot() {
  local name="$1"
  "$ADB" -s "$SERIAL" exec-out screencap -p > "$OUT_DIR/screenshots/$name"
  python3 - "$OUT_DIR/screenshots/$name" <<'PY'
import struct, sys
p=sys.argv[1]
with open(p,'rb') as f:
    sig=f.read(8)
    if sig != b'\x89PNG\r\n\x1a\n':
        raise SystemExit("not png")
    _length=f.read(4)
    typ=f.read(4)
    if typ != b'IHDR':
        raise SystemExit("missing IHDR")
    w,h=struct.unpack(">II", f.read(8))
print(f"{w}x{h}")
PY
}

START_DIMS="$(screenshot startup.png)"
report "startupScreenshot=startup.png"
report "startupScreenshotDimensions=$START_DIMS"

W="${START_DIMS%x*}"
H="${START_DIMS#*x}"
if (( W <= H )); then
  sleep 5
  START_DIMS="$(screenshot startup-landscape-retry.png)"
  W="${START_DIMS%x*}"
  H="${START_DIMS#*x}"
fi
report "runtimeResolution=${W}x${H}"
if (( W <= H )); then
  fail "App did not present a landscape framebuffer."
fi
report "landscapeEvidence=True"

read CENTER_X CENTER_Y SEED_X SEED_Y WAVE_X WAVE_Y A2_X A2_Y < <(python3 - "$W" "$H" <<'PY'
import math,sys
w,h=map(float,sys.argv[1:3])
scale=math.sqrt((w/1920.0)*(h/1080.0))
cx=w/2.0
cy=h/2.0
seedx=cx-125.0*scale
seedy=cy-125.0*scale
wavex=(34.0+150.0)*scale
wavey=h-(34.0+63.0)*scale
aspect=w/h
half_h=7.55
half_w=half_h*aspect
left=12.0-half_w
bottom=7.0-half_h
def world_to_adb(x,y):
    sx=(x-left)/(2*half_w)*w
    sy=(y-bottom)/(2*half_h)*h
    return sx,h-sy
a2x,a2y=world_to_adb(11.0,9.0)
print(*(int(round(v)) for v in (cx,cy,seedx,seedy,wavex,wavey,a2x,a2y)))
PY
)
report "tapPlantingSpotA3=${CENTER_X},${CENTER_Y}"
report "tapSeedShooterOption=${SEED_X},${SEED_Y}"
report "tapWaveButton=${WAVE_X},${WAVE_Y}"
report "tapWave2ResponsivenessSpotA2=${A2_X},${A2_Y}"

"$ADB" -s "$SERIAL" shell input tap "$CENTER_X" "$CENTER_Y"
sleep 2
RADIAL_DIMS="$(screenshot radial-menu.png)"
report "radialScreenshot=radial-menu.png"
report "radialScreenshotDimensions=$RADIAL_DIMS"

"$ADB" -s "$SERIAL" shell input tap "$SEED_X" "$SEED_Y"
sleep 3
PLANT_DIMS="$(screenshot planted-seedshooter.png)"
report "plantedScreenshot=planted-seedshooter.png"
report "plantedScreenshotDimensions=$PLANT_DIMS"

"$ADB" -s "$SERIAL" shell input tap "$WAVE_X" "$WAVE_Y"
sleep 4
WAVE1_DIMS="$(screenshot wave1-active.png)"
report "wave1Screenshot=wave1-active.png"
report "wave1ScreenshotDimensions=$WAVE1_DIMS"

sleep 75
"$ADB" -s "$SERIAL" shell input tap "$WAVE_X" "$WAVE_Y"
sleep 4
WAVE2_DIMS="$(screenshot wave2-active.png)"
report "wave2Screenshot=wave2-active.png"
report "wave2ScreenshotDimensions=$WAVE2_DIMS"

"$ADB" -s "$SERIAL" shell input tap "$A2_X" "$A2_Y"
sleep 2
RESP_DIMS="$(screenshot wave2-touch-responsive.png)"
report "responsivenessScreenshot=wave2-touch-responsive.png"
report "responsivenessScreenshotDimensions=$RESP_DIMS"

PID_END="$("$ADB" -s "$SERIAL" shell pidof "$PACKAGE" 2>/dev/null | tr -d '\r')"
FOREGROUND_END="$("$ADB" -s "$SERIAL" shell dumpsys activity activities 2>/dev/null | grep -m1 'mResumedActivity' | tr -d '\r')"
ROTATION_INFO="$("$ADB" -s "$SERIAL" shell dumpsys window displays 2>/dev/null | grep -E -m2 'mCurrentFocus|rotation|Rotation' | tr '\n' ';' | tr -d '\r')"
report "pidAfterScenario=$PID_END"
report "foregroundAfterScenario=$FOREGROUND_END"
report "rotationInfo=$ROTATION_INFO"
if [[ -z "$PID_END" || "$FOREGROUND_END" != *"$PACKAGE"* ]]; then
  fail "Package is no longer alive/foreground after smoke interactions."
fi

sleep 2
if [[ -n "$LOGCAT_PID" ]]; then
  kill "$LOGCAT_PID" >/dev/null 2>&1 || true
  wait "$LOGCAT_PID" >/dev/null 2>&1 || true
  LOGCAT_PID=""
fi

grep -Ei "FATAL EXCEPTION|ANR in $PACKAGE|am_crash.*$PACKAGE|Process: $PACKAGE|AndroidRuntime.*$PACKAGE|Unity.*(Exception|Error)|Exception.*Unity" "$LOGCAT" > "$LOGCAT_FILTERED" || true
FATAL_COUNT="$(grep -Eic "FATAL EXCEPTION|ANR in $PACKAGE|am_crash.*$PACKAGE|Process: $PACKAGE" "$LOGCAT" || true)"
ANDROID_RUNTIME_COUNT="$(grep -Eic "AndroidRuntime.*$PACKAGE" "$LOGCAT" || true)"
UNITY_ERROR_COUNT="$(grep -Eic "Unity.*(Exception|Error)|Exception.*Unity" "$LOGCAT" || true)"
report "fatalExceptionOrCrashMatches=$FATAL_COUNT"
report "androidRuntimePackageMatches=$ANDROID_RUNTIME_COUNT"
report "unityErrorExceptionMatches=$UNITY_ERROR_COUNT"
report "logcatFile=logcat.txt"
report "filteredLogcatFile=logcat-filtered.txt"
report "smokeAutomationCompleted=True"

if (( FATAL_COUNT > 0 )); then
  fail "Package-specific fatal/crash/ANR evidence was found in logcat."
fi

report "status=AUTOMATION_COMPLETE_PENDING_VISUAL_REVIEW"
exit 0
