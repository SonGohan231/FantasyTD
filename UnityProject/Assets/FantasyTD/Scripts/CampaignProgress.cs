using UnityEngine;

namespace FantasyTD.MapA
{
    public static class CampaignProgress
    {
        private const string VictoryKeyPrefix = "FantasyTD.Campaign.v1.Victory.";

        public static bool HasVictory(string mapId) => PlayerPrefs.GetInt(VictoryKeyPrefix + mapId, 0) == 1;

        public static bool IsUnlocked(string mapId)
        {
            CampaignMapDefinition definition = CampaignCatalog.Get(mapId);
            if (definition == null || !definition.IsValidatedPlayable) return false;
            if (mapId == "MAP1") return true;
            int number;
            return mapId.StartsWith("MAP") && int.TryParse(mapId.Substring(3), out number) &&
                number > 1 && number <= 8 && HasVictory("MAP" + (number - 1));
        }

        public static void RecordVictory(string mapId)
        {
            CampaignMapDefinition definition = CampaignCatalog.Get(mapId);
            if (definition == null || !definition.IsValidatedPlayable) return;
            PlayerPrefs.SetInt(VictoryKeyPrefix + mapId, 1);
            PlayerPrefs.Save();
        }
    }
}

