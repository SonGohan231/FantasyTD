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
        public static void EnsureSpriteImported(string artworkPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(artworkPath)
                as TextureImporter;
            if (importer == null)
                throw new System.InvalidOperationException(
                    "FT2 artwork is missing a TextureImporter: " + artworkPath);
            if (importer.textureType != TextureImporterType.Sprite ||
                importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.isReadable = artworkPath.Contains("/FT2/Units/");
                importer.SaveAndReimport();
            }

            Sprite art = AssetDatabase.LoadAssetAtPath<Sprite>(artworkPath);
            Texture2D original = AssetDatabase.LoadAssetAtPath<Texture2D>(artworkPath);
            Debug.Log("FT2 art inspect: " + artworkPath +
                " importer=" + importer.textureType +
                " Sprite=" + (art == null ? "null" : art.name) +
                " Texture2D=" + (original == null ? "null" : original.name));
            if (art == null)
            {
                // A tracked legacy 2-line .meta can retain a stale texture
                // import artifact on the self-hosted Unity runner.
                AssetDatabase.ImportAsset(artworkPath,
                    ImportAssetOptions.ForceSynchronousImport |
                    ImportAssetOptions.ForceUpdate);
                art = AssetDatabase.LoadAssetAtPath<Sprite>(artworkPath);
            }
            if (art == null || art.texture == null)
                throw new System.InvalidOperationException(
                    "FT2 artwork cannot be loaded as Sprite: " + artworkPath);
        }

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
