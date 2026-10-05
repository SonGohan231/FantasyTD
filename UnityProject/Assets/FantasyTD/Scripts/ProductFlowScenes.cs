using UnityEngine.SceneManagement;

namespace FantasyTD.Product
{
    public static class ProductFlowScenes
    {
        public const string MainMenuSceneName = "MAIN_MENU";
        public const string CampaignSceneName = "CAMPAIGN_MAP_SELECT";
        public const string MapASceneName = "MAP_A_ANDROID_V2";
        public const string Map2SceneName = "MAP_2_POSTAPO";

        public static void OpenMainMenu() => SceneManager.LoadScene(MainMenuSceneName);
        public static void OpenCampaign() => SceneManager.LoadScene(CampaignSceneName);
        public static void OpenMap2() => SceneManager.LoadScene(Map2SceneName);
        public static void OpenMapA() => SceneManager.LoadScene(MapASceneName);
    }
}
