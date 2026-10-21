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
                importer.textureShape != TextureImporterShape.Texture2D ||
                importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.textureShape = TextureImporterShape.Texture2D;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.isReadable = artworkPath.Contains("/FT2/Units/");
                importer.SaveAndReimport();
            }

            // Diagnostic probe: compare importer data with on-disk PNG header
            // and actual AssetDatabase subassets, not only the importer type.
            string absolutePath = System.IO.Path.Combine(
                System.IO.Directory.GetParent(Application.dataPath).FullName,
                artworkPath);
            var raw = System.IO.File.ReadAllBytes(absolutePath);
            string signature = System.BitConverter.ToString(raw, 0,
                System.Math.Min(raw.Length, 24));
            var objects = AssetDatabase.LoadAllAssetsAtPath(artworkPath);
            string objectTypes = objects == null ? "null" :
                string.Join(", ", System.Array.ConvertAll(objects,
                    x => x == null ? "null" : x.GetType().Name));
            Debug.Log("FT2 asset source inspect: " + artworkPath +
                " fileBytes=" + raw.Length +
                " header=" + signature +
                " allAssets=[" + objectTypes + "]" +
                " mainType=" + AssetDatabase.GetMainAssetTypeAtPath(artworkPath) +
                " guid=" + AssetDatabase.AssetPathToGUID(artworkPath));

            Sprite art = AssetDatabase.LoadAssetAtPath<Sprite>(artworkPath);
            Texture2D original = AssetDatabase.LoadAssetAtPath<Texture2D>(artworkPath);
            Debug.Log("FT2 art inspect: " + artworkPath +
                " importer=" + importer.textureType +
                " shape=" + importer.textureShape +
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
            // Guard against importer metadata producing Cubemap main assets
            // even when TextureImporterType visually reads as Sprite.
            importer.textureShape = TextureImporterShape.Texture2D;
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
