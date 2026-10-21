#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FantasyTD.V2.Editor
{
    /// <summary>
    /// Stable import rules shared by Editor smoke and Android player.
    /// Keeps legacy MAP-A art import settings entirely untouched.
    /// </summary>
    public sealed class Forest01ArtworkImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Resources/FT2/")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.isReadable = assetPath.StartsWith("Assets/Resources/FT2/Units/");
            importer.maxTextureSize = assetPath.StartsWith("Assets/Resources/FT2/Units/") ? 1024 : 2048;
        }
    }
}
#endif
