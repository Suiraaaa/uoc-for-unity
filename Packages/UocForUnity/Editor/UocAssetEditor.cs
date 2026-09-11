using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Uoc;
using Uoc.Chart.Notes.Definition;

namespace UocForUnity.Editor
{
    [CustomEditor(typeof(UocAsset))]
    internal sealed class UocAssetEditor : UnityEditor.Editor
    {
        private readonly List<KeyValuePair<string, string>> basicInformation = new();
        private readonly List<KeyValuePair<string, string>> properties = new();
        private readonly List<KeyValuePair<string, string>> chartInformation = new();
        private readonly List<string> warnings = new();
        private string source;
        private string parseError;
        private Vector2 sourceScroll;
        private GUIStyle sourceStyle;

        private void OnEnable()
        {
            var asset = (UocAsset)target;
            var icon = Resources.Load<Texture2D>("UocForUnity/UocFileIcon");
            var script = MonoScript.FromScriptableObject(asset);
            if (icon != null && script != null)
            {
                EditorGUIUtility.SetIconForObject(script, icon);
            }
            RefreshInformation(asset);
        }

        private void OnDisable()
        {
            source = null;
            parseError = null;
            sourceStyle = null;
            basicInformation.Clear();
            properties.Clear();
            chartInformation.Clear();
            warnings.Clear();
        }

        public override void OnInspectorGUI()
        {
            var asset = (UocAsset)target;
            if (asset == null)
            {
                return;
            }

            // 再インポートで文字列が変わった場合だけ表示情報を再取得する
            if (source != asset.Value) RefreshInformation(asset);

            if (parseError != null)
            {
                EditorGUILayout.HelpBox(parseError, MessageType.Error);
            }
            else
            {
                DrawSection("基本情報", basicInformation);
                DrawSection("譜面プロパティ", properties);
                DrawSection("譜面情報", chartInformation);
                foreach (var warning in warnings)
                {
                    EditorGUILayout.HelpBox(warning, MessageType.Warning);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("UOC文字列", EditorStyles.boldLabel);
            sourceStyle ??= new GUIStyle(EditorStyles.textArea) { wordWrap = false };
            var text = source ?? string.Empty;
            var size = sourceStyle.CalcSize(new GUIContent(text));
            sourceScroll = EditorGUILayout.BeginScrollView(sourceScroll, GUILayout.Height(250f));
            EditorGUILayout.SelectableLabel(text, sourceStyle, GUILayout.MinWidth(Mathf.Max(100f, size.x)), GUILayout.Height(Mathf.Max(200f, size.y)));
            EditorGUILayout.EndScrollView();
        }

        private void RefreshInformation(UocAsset asset)
        {
            source = asset.Value;
            sourceScroll = Vector2.zero;
            parseError = null;
            basicInformation.Clear();
            properties.Clear();
            chartInformation.Clear();
            warnings.Clear();

            UocObject chart;
            try
            {
                chart = asset.Parse();
            }
            catch (Exception exception)
            {
                parseError = $"UOC の読み取りに失敗しました。\n{exception.Message}";
                return;
            }

            var chartProperties = chart.ChartPropertyGroup;
            AddInformation(basicInformation, "GameID", () => chartProperties.GetGameId());
            AddInformation(basicInformation, "TicksPerBeat", () => chartProperties.GetTpb().Value.ToString());

            for (int i = 0; i < chartProperties.Count; i++)
            {
                var property = chartProperties[i];
                var key = property.Key.Value;
                if (key == "GameID" || key == "TicksPerBeat")
                {
                    continue;
                }

                properties.Add(new KeyValuePair<string, string>(key, property.Value.HasValue() ? property.Value.AsString() : "（値なし）"));
            }

            // グループの構成ノートも個別に数え、共通イベントは除外する
            AddInformation(chartInformation, "総ノーツ数", () => chart.NoteProfileCollection.NoteProfiles.Count(note => !NoteDef.CommonNoteDefs.Any(def => def.NoteId == note.NoteId)).ToString());
            AddInformation(chartInformation, "総ノートグループ数", () => chart.NoteGroupProfileCollection.NoteGroupProfiles.Count.ToString());
            AddInformation(chartInformation, "BPM", () => GetBpmRange(chart));
        }

        private static string GetBpmRange(UocObject chart)
        {
            var notes = chart.NoteProfileCollection;
            var provider = notes.CreateBpmProvider(notes.CreateMeasureLengthProvider(), chart.ChartPropertyGroup.GetTpb());
            var minimum = provider.GetMeasureStartBpm(0).Value;
            var maximum = minimum;

            // BPM イベントのある小節だけ調べる
            var measures = notes.NoteProfiles.Where(note => note.NoteId == NoteDef.BpmChange.NoteId).Select(note => note.Position.MeasureIndex.Value).Distinct();
            foreach (var measure in measures)
            {
                var startBpm = provider.GetMeasureStartBpm(measure).Value;
                minimum = Math.Min(minimum, startBpm);
                maximum = Math.Max(maximum, startBpm);
                foreach (var change in provider.GetBpmChangeEventsAt(measure))
                {
                    minimum = Math.Min(minimum, change.Bpm.Value);
                    maximum = Math.Max(maximum, change.Bpm.Value);
                }
            }
            return minimum == maximum ? minimum.ToString() : $"{minimum}~{maximum}";
        }

        private void AddInformation(List<KeyValuePair<string, string>> destination, string label, Func<string> readValue)
        {
            string value;
            try
            {
                value = readValue();
            }
            catch (Exception exception)
            {
                value = "取得不可";
                warnings.Add($"{label}: {exception.Message}");
            }
            destination.Add(new KeyValuePair<string, string>(label, value));
        }

        private static void DrawSection(string title, List<KeyValuePair<string, string>> items)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            if (items.Count == 0)
            {
                EditorGUILayout.LabelField("（なし）");
            }

            foreach (var item in items)
            {
                DrawValue(item.Key, item.Value);
            }

            EditorGUILayout.Space();
        }

        private static void DrawValue(string label, string value)
        {
            var width = Mathf.Max(80f, EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 40f);
            var height = EditorStyles.wordWrappedLabel.CalcHeight(new GUIContent(value), width);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(new GUIContent(label, label));
                EditorGUILayout.SelectableLabel(value, EditorStyles.wordWrappedLabel, GUILayout.Height(Mathf.Max(EditorGUIUtility.singleLineHeight, height)));
            }
        }
    }
}
