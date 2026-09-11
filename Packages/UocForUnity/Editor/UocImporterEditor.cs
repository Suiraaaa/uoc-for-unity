using UnityEditor;
using UnityEditor.AssetImporters;

namespace UocForUnity.Editor
{
    [CustomEditor(typeof(UocImporter))]
    internal sealed class UocImporterEditor : ScriptedImporterEditor
    {
        private UnityEditor.Editor assetEditor;

        protected override bool needsApplyRevert => false;

        public override bool showImportedObject => false;

        protected override void OnHeaderGUI()
        {
            if (PrepareAssetEditor())
            {
                assetEditor.DrawHeader();
            }
            else
            {
                base.OnHeaderGUI();
            }
        }

        public override void OnInspectorGUI()
        {
            if (!PrepareAssetEditor())
            {
                EditorGUILayout.HelpBox("UOC アセットを読み込めません。インポートエラーを確認してください。", MessageType.Error);
                return;
            }


            // Imported Object の無効化された表示枠を通さず、表示専用 UI を描画する
            assetEditor.OnInspectorGUI();
        }

        public override void OnDisable()
        {
            if (assetEditor != null)
            {
                DestroyImmediate(assetEditor);
            }

            assetEditor = null;
            base.OnDisable();
        }

        private bool PrepareAssetEditor()
        {
            if (target == null)
            {
                return false;
            }

            var importer = (UocImporter)target;
            var asset = AssetDatabase.LoadAssetAtPath<UocAsset>(importer.assetPath);
            if (asset == null)
            {
                if (assetEditor != null)
                {
                    DestroyImmediate(assetEditor);
                }

                assetEditor = null;
                return false;
            }

            // 再インポートでアセットが置き換わった場合は Editor も更新する
            CreateCachedEditor(asset, typeof(UocAssetEditor), ref assetEditor);
            return assetEditor != null;
        }
    }
}
