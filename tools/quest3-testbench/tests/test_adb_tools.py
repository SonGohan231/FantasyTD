"""Mock ADB regression checks; no headset needed. Hardware status remains unverified."""
import sys
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch
from subprocess import CompletedProcess, TimeoutExpired

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from adb_tools import QuestADB, QuestError, Device, parse_devices, check_package


def cp(argv, output="", error="", code=0):
    return CompletedProcess(argv, code, output, error)


class QuestToolsTests(unittest.TestCase):
    def test_parse_device_states_and_multiple_devices(self):
        data = ("List of devices attached\n"
                "1WMH1AB123 device product:eureka model:Quest_3\n"
                "R9DT12 unauthorized usb:2-3\n"
                "ZZ99 offline\n")
        self.assertEqual([d.state for d in parse_devices(data)],
                         ["device", "unauthorized", "offline"])
        self.assertEqual(parse_devices(data)[0].serial, "1WMH1AB123")

    @patch("adb_tools.adb_executable", return_value=None)
    def test_missing_adb(self, _):
        with self.assertRaisesRegex(QuestError, "Brak ADB"):
            QuestADB()

    @patch("adb_tools.subprocess.run")
    def test_unauthorized_blocks_install(self, run):
        run.return_value = cp([], "List of devices attached\nserial unauthorized\n")
        with self.assertRaisesRegex(QuestError, "USB debugging"):
            QuestADB("adb").install("serial", "game.apk")
        self.assertEqual(run.call_count, 1)

    @patch("adb_tools.subprocess.run")
    def test_offline_blocks_launch(self, run):
        run.return_value = cp([], "List of devices attached\nserial offline\n")
        with self.assertRaisesRegex(QuestError, "offline"):
            QuestADB("adb").launch("serial", "com.example.game")

    @patch("adb_tools.subprocess.run")
    def test_no_device_blocks_stop(self, run):
        run.return_value = cp([], "List of devices attached\n")
        with self.assertRaisesRegex(QuestError, "nie jest podłączone"):
            QuestADB("adb").stop("serial", "com.example.game")

    @patch("adb_tools.subprocess.run")
    def test_install_apk_uses_safe_argument_vector(self, run):
        run.side_effect = [
            cp([], "List of devices attached\nserial device\n"),
            cp([], "Success\n"),
        ]
        with tempfile.TemporaryDirectory() as tmp:
            apk = Path(tmp) / "test APK.apk"
            apk.write_bytes(b"APK placeholder")
            result = QuestADB("adb").install("serial", apk)
            self.assertIn("Success", result)
            argv = run.call_args.args[0]
            self.assertEqual(argv[:5], ["adb", "-s", "serial", "install", "-r"])
            self.assertEqual(argv[-1], str(apk))
            self.assertIs(run.call_args.kwargs["shell"], False)

    @patch("adb_tools.subprocess.run")
    def test_invalid_apk_rejected(self, run):
        run.return_value = cp([], "List of devices attached\nserial device\n")
        with self.assertRaisesRegex(QuestError, ".apk"):
            QuestADB("adb").install("serial", "/path/notfound.zip")

    def test_invalid_package_rejected(self):
        for package in ("", "my package", "x;rm -rf", "com.example;echo.pwned"):
            with self.subTest(package=package):
                with self.assertRaises(QuestError):
                    check_package(package)
        self.assertEqual(check_package("com.example.game"), "com.example.game")

    @patch("adb_tools.subprocess.run")
    def test_launch_and_stop_package(self, run):
        run.side_effect = [
            cp([], "List of devices attached\nserial device\n"),
            cp([], "Events injected: 1"),
            cp([], "List of devices attached\nserial device\n"),
            cp([], ""),
        ]
        tool = QuestADB("adb")
        tool.launch("serial", "com.example.game")
        tool.stop("serial", "com.example.game")
        self.assertIn("monkey", run.call_args_list[1].args[0])
        self.assertIn("force-stop", run.call_args_list[3].args[0])

    @patch("adb_tools.subprocess.run")
    def test_logcat_capture(self, run):
        run.side_effect = [
            cp([], "List of devices attached\nserial device\n"),
            cp([], "stacktrace test"),
        ]
        self.assertIn("stacktrace", QuestADB("adb").logcat("serial"))
        self.assertIn("logcat", run.call_args.args[0])

    @patch("adb_tools.subprocess.run")
    def test_screenshot_binary_png(self, run):
        run.side_effect = [
            cp([], "List of devices attached\nserial device\n"),
            cp([], b"\x89PNG\r\n\x1a\nsample", b""),
        ]
        with tempfile.TemporaryDirectory() as tmp:
            img = Path(tmp) / "capture.png"
            QuestADB("adb").screenshot("serial", img)
            self.assertTrue(img.read_bytes().startswith(b"\x89PNG"))

    @patch("adb_tools.subprocess.run")
    def test_screenshot_rejects_invalid_data(self, run):
        run.side_effect = [
            cp([], "List of devices attached\nserial device\n"),
            cp([], b"not a PNG", b""),
        ]
        with tempfile.TemporaryDirectory() as tmp:
            with self.assertRaisesRegex(QuestError, "PNG"):
                QuestADB("adb").screenshot("serial", Path(tmp) / "capture.png")

    @patch("adb_tools.subprocess.run", side_effect=TimeoutExpired(cmd=["adb"], timeout=2))
    def test_timeout_handled(self, run):
        with self.assertRaisesRegex(QuestError, "limit"):
            QuestADB("adb").devices()


if __name__ == "__main__":
    unittest.main()
