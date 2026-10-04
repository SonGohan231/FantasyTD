namespace FantasyTD.MapA
{
    public static class CampaignCatalog
    {
        public static CampaignMapDefinition Get(string id)
        {
            switch (id)
            {
                case "MAP1": return CampaignMapDefinition.Map1;
                case "MAP2": return CampaignMapDefinition.Map2;
                case "MAP3": return Map3SourceDefinition.Definition;
                case "MAP4": return Map4SourceDefinition.Definition;
                case "MAP5": return Map5SourceDefinition.Definition;
                case "MAP6": return Map6SourceDefinition.Definition;
                case "MAP8": return Map8SourceDefinition.Definition;
                default: return null;
            }
        }
    }
}

