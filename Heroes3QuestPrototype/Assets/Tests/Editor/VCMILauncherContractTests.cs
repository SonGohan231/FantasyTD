using NUnit.Framework;

namespace Heroes3QuestPrototype.Tests
{
    public class VCMILauncherContractTests
    {
        [Test]
        public void PackageNameMatchesPublishedVCMIAndroidPackage()
        {
            Assert.AreEqual("is.xyz.vcmi", VCMIAndroidLauncher.AndroidPackageId);
        }
        [Test]
        public void EditorNeverClaimsToLaunchAnAndroidApp()
        {
#if UNITY_EDITOR
            StringAssert.Contains("Android/Quest", VCMIAndroidLauncher.TryLaunch());
#endif
        }
    }
}
