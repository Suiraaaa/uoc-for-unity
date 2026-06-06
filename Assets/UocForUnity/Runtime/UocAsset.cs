using System;
using UnityEngine;
using Uoc;
using Uoc.Parse;

namespace UocForUnity
{
    /// <summary>
    /// Unity へインポートされた UOC ファイルを表すアセット。
    ///
    /// UOC 文字列を保持し、必要に応じて
    /// UocString または UocObject へ変換します。
    /// </summary>
    public class UocAsset : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        private string value;

        /// <summary>
        /// 元のUOC文字列
        /// </summary>
        public string Value => value;

        /// <summary>
        /// UocStringからUnityアセットを生成します。
        /// </summary>
        public static UocAsset Create(UocString uocString)
        {
            if (uocString is null) throw new ArgumentNullException(nameof(uocString));

            var asset = CreateInstance<UocAsset>();
            asset.value = uocString.Value;

            return asset;
        }

        /// <summary>
        /// 保持している文字列からUocStringを生成します。
        /// </summary>
        public UocString ToUocString()
        {
            return new UocString(value);
        }

        /// <summary>
        /// 保持しているUOC文字列をパースし、UocObjectを生成します。
        /// </summary>
        public UocObject Parse()
        {
            var uocString = ToUocString();
            return UocParser.Parse(uocString);
        }
    }
}
