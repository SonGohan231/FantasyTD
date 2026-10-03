#if UNITY_EDITOR
using UnityEditor;

namespace FantasyTD.MapA.Editor
{
    public sealed class HiggsfieldTextureImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (string.IsNullOrEmpty(assetPath) ||
                !assetPath.Contains("/Resources/Higgsfield/"))
                return;

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.isReadable = true;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = UnityEngine.FilterMode.Bilinear;
            importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
            if (assetPath.Contains("/Higgsfield/Maps/"))
                importer.npotScale = TextureImporterNPOTScale.None;
        }
    }
}
#endif
