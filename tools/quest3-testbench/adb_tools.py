"""Bezpieczna warstwa ADB dla testów natywnych aplikacji Quest 3."""
from __future__ import annotations

import os
import re
import shutil
import subprocess
from dataclasses import dataclass
from pathlib import Path


PACKAGE_RE = re.compile(r"^[a-zA-Z_][\w]*(?:\.[a-zA-Z_][\w]*)+$")


class QuestError(RuntimeError):
    """Błąd narzędzia lub połączenia z urządzeniem."""


@dataclass(frozen=True)
class Device:
    serial: str
    state: str
    details: str = ""


def parse_devices(output: str) -> list[Device]:
    result = []
    for line in output.splitlines():
        line = line.strip()
        if not line or line.startswith("List of devices") or line.startswith("*"):
            continue
        parts = line.split(maxsplit=2)
        if len(parts) >= 2:
            result.append(Device(parts[0], parts[1], parts[2] if len(parts) > 2 else ""))
    return result


def adb_executable() -> str | None:
    found = shutil.which("adb")
    if found:
        return found
    for key in ("ANDROID_SDK_ROOT", "ANDROID_HOME"):
        root = os.environ.get(key)
        if root:
            candidate = Path(root) / "platform-tools" / ("adb.exe" if os.name == "nt" else "adb")
            if candidate.is_file():
                return str(candidate)
    return None


def check_package(package: str) -> str:
    if not PACKAGE_RE.fullmatch(package):
        raise QuestError("Wprowadź poprawny identyfikator pakietu, np. com.firma.gra")
    return package


class QuestADB:
    def __init__(self, executable: str | None = None):
        self.executable = executable or adb_executable()
        if not self.executable:
            raise QuestError("Brak ADB. Zainstaluj Android SDK Platform Tools i dodaj adb do PATH.")

    def run(self, *args: str, timeout: int = 90, binary: bool = False):
        try:
            result = subprocess.run(
                [self.executable, *args],
                capture_output=True,
                text=not binary,
                timeout=timeout,
                check=False,
                shell=False,
            )
        except subprocess.TimeoutExpired as exc:
            raise QuestError(f"ADB: przekroczono limit {timeout}s") from exc
        except OSError as exc:
            raise QuestError(f"Nie można uruchomić ADB: {exc}") from exc
        if result.returncode:
            error = result.stderr
            if isinstance(error, bytes):
                error = error.decode("utf-8", errors="replace")
            raise QuestError((error or "Nieznany błąd ADB").strip())
        return result.stdout

    def devices(self) -> list[Device]:
        return parse_devices(self.run("devices", "-l", timeout=15))

    def require_device(self, serial: str) -> Device:
        matches = [d for d in self.devices() if d.serial == serial]
        if not matches:
            raise QuestError("Wybrane urządzenie nie jest podłączone.")
        device = matches[0]
        if device.state == "unauthorized":
            raise QuestError("Załóż gogle i zaakceptuj komunikat USB debugging.")
        if device.state != "device":
            raise QuestError(f"Quest nie jest gotowy (stan: {device.state}).")
        return device

    def install(self, serial: str, apk: str | Path) -> str:
        self.require_device(serial)
        apk = Path(apk)
        if not apk.is_file() or apk.suffix.lower() != ".apk":
            raise QuestError("Wskaż istniejący plik .apk")
        return self.run("-s", serial, "install", "-r", str(apk), timeout=300)

    def launch(self, serial: str, package: str) -> str:
        self.require_device(serial)
        return self.run("-s", serial, "shell", "monkey", "-p", check_package(package),
                        "-c", "android.intent.category.LAUNCHER", "1")

    def stop(self, serial: str, package: str) -> str:
        self.require_device(serial)
        return self.run("-s", serial, "shell", "am", "force-stop", check_package(package))

    def logcat(self, serial: str, limit: int = 1000) -> str:
        self.require_device(serial)
        n = max(100, min(int(limit), 5000))
        return self.run("-s", serial, "logcat", "-d", "-t", str(n), timeout=35)

    def screenshot(self, serial: str, destination: str | Path) -> Path:
        self.require_device(serial)
        destination = Path(destination)
        destination.parent.mkdir(parents=True, exist_ok=True)
        data = self.run("-s", serial, "exec-out", "screencap", "-p",
                        timeout=25, binary=True)
        if not data.startswith(b"\x89PNG"):
            raise QuestError("ADB nie zwróciło poprawnego zrzutu PNG.")
        destination.write_bytes(data)
        return destination

    def record(self, serial: str, destination: str | Path, seconds: int = 15) -> Path:
        self.require_device(serial)
        seconds = max(1, min(int(seconds), 30))
        remote = "/sdcard/quest3-testbench-record.mp4"
        destination = Path(destination)
        destination.parent.mkdir(parents=True, exist_ok=True)
        self.run("-s", serial, "shell", "screenrecord", "--time-limit", str(seconds),
                 remote, timeout=seconds + 20)
        try:
            self.run("-s", serial, "pull", remote, str(destination), timeout=90)
        finally:
            try:
                self.run("-s", serial, "shell", "rm", remote)
            except QuestError:
                pass
        if not destination.is_file():
            raise QuestError("Nie pobrano nagrania.")
        return destination


def pcvr_diagnostics() -> dict[str, str]:
    """Wykrywanie runtime OpenXR na Windows; nie udaje połączenia sprzętowego."""
    result = {"platform": os.name, "openxr_runtime": "niedostępny"}
    if os.name != "nt":
        result["note"] = "Meta Horizon Link do Unity Play Mode jest dostępny na Windows."
        return result
    try:
        import winreg
        with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\Khronos\OpenXR\1") as key:
            result["openxr_runtime"] = str(winreg.QueryValueEx(key, "ActiveRuntime")[0])
    except (FileNotFoundError, OSError):
        result["note"] = "Sprawdź Meta Horizon Link > Settings > General > OpenXR Runtime."
    return result
