using System;
using System.IO;
using System.Text;
using UnityEditor.AssetImporters;
using UnityEngine;
using Uoc;
using Uoc.Parse;

namespace UocForUnity.Editor
{
    /// <summary>
    /// .uocファイルをUocTextAssetとしてインポートします。
    /// </summary>
    [ScriptedImporter(1, "uoc")]
    public sealed class UocImporter : ScriptedImporter
    {
        private const string MainAssetIdentifier = "UocAsset";
        private const string IconResourcePath = "UocForUnity/UocFileIcon";

        public override void OnImportAsset(AssetImportContext context)
        {
            try
            {
                var sourceText = File.ReadAllText(context.assetPath, Encoding.UTF8);
                var uocString = new UocString(sourceText);

                // インポート時点で不正な UOC ファイルを検出
                // ただしパース結果は保持しない
                _ = UocParser.Parse(uocString);

                var asset = UocAsset.Create(uocString);
                asset.name = Path.GetFileNameWithoutExtension(context.assetPath);

                var icon = LoadIcon(context);
                if (icon == null)
                {
                    context.AddObjectToAsset(MainAssetIdentifier, asset);
                }
                else
                {
                    context.AddObjectToAsset(MainAssetIdentifier, asset, icon);
                }

                context.SetMainObject(asset);
            }
            catch (Exception exception)
            {
                context.LogImportError($"UOCファイルのインポートに失敗しました: {context.assetPath}\n{exception}");
            }
        }

        private static Texture2D LoadIcon(AssetImportContext context)
        {
            var icon = Resources.Load<Texture2D>(IconResourcePath);
            if (icon == null)
            {
                context.LogImportWarning($"UOCアセット用アイコンを読み込めません: " + $"{IconResourcePath}");
            }
            return icon;
        }
    }
}
