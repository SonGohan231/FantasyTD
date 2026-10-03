using UnityEngine;

namespace FantasyTD.MapA
{
    public static class CampaignProgress
    {
        private const string VictoryKeyPrefix = "FantasyTD.Campaign.v1.Victory.";

        public static bool HasVictory(string mapId) => PlayerPrefs.GetInt(VictoryKeyPrefix + mapId, 0) == 1;

        public static bool IsUnlocked(string mapId)
        {
            if (mapId == "MAP1") return true;
            if (mapId == "MAP2") return HasVictory("MAP1");
            return false;
        }

        public static void RecordVictory(string mapId)
        {
            CampaignMapDefinition definition = mapId == "MAP1" ? CampaignMapDefinition.Map1
                : (mapId == "MAP2" ? CampaignMapDefinition.Map2 : null);
            if (definition == null || !definition.IsValidatedPlayable) return;
            PlayerPrefs.SetInt(VictoryKeyPrefix + mapId, 1);
            PlayerPrefs.Save();
        }
    }
}
