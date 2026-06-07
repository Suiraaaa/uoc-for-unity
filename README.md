# uoc-for-unity

[![License](https://img.shields.io/github/license/Suiraaaa/uoc-for-unity)](./LICENSE)
[![GitHub Release](https://img.shields.io/github/v/release/Suiraaaa/uoc-for-unity?include_prereleases=true\&sort=semver)](https://github.com/Suiraaaa/uoc-for-unity/releases)
![Unity](https://img.shields.io/badge/Unity-2022.3%2B-000000?logo=unity)

UOC（Universal Otoge Chart）形式の譜面ファイルを、Unityプロジェクト内で扱うためのパッケージです。

Unityプロジェクトの `Assets` ディレクトリ以下へ `.uoc` ファイルを配置すると、Unity上で参照可能な `UocAsset` として自動的にインポートされます。

このパッケージは、UOCファイルの解析処理を独自に再実装するものではありません。
内部では [`uoc-for-c-sharp`](https://github.com/Suiraaaa/uoc-for-c-sharp) を使用し、UnityとUOCライブラリの間を接続するための機能を提供します。

> [!WARNING]
> 現在は開発中の初期バージョンです。
> 公開API、インポート時の挙動、対応環境は、今後のリリースで変更される可能性があります。

## UOCフォーマットについて

UOC（Universal Otoge Chart）は、音楽ゲームの譜面データを表現するためのフォーマットです。

フォーマットの詳細については、以下の仕様書を参照してください。

* [UOCフォーマット仕様](https://gist.github.com/Suiraaaa/188f4ec0639fde9834d7cb7ef057bf2c)

## 依存ライブラリ

このパッケージは、以下のC#ライブラリに依存しています。

* [`uoc-for-c-sharp`](https://github.com/Suiraaaa/uoc-for-c-sharp)

`uoc-for-c-sharp` は、UOC文字列のパース、譜面情報の管理、イベント情報の取得、譜面再生向けデータの解析などを行います。

`uoc-for-unity` には、`uoc-for-c-sharp` のビルド成果物である `Uoc.dll` が同梱されています。
利用者が `Uoc.dll` を別途導入する必要はありません。

詳細なAPI仕様については、以下のドキュメントを参照してください。

* [`uoc-for-c-sharp` API仕様書](https://github.com/Suiraaaa/uoc-for-c-sharp/blob/0.1.0-alpha.1/SPEC.md)
* [UOCフォーマット仕様](https://gist.github.com/Suiraaaa/188f4ec0639fde9834d7cb7ef057bf2c)

## 主な機能

このパッケージは、Unity向けに以下の機能を提供します。

* `.uoc` ファイルの自動インポート
* インポートされた譜面の `UocAsset` としての参照
* `UocAsset.Value` による元のUOC文字列の取得
* `UocAsset.Parse()` による `UocObject` の生成
* `.uoc` アセット専用アイコンの表示

UOCの具体的な解析処理は、内部で `uoc-for-c-sharp` に委譲されます。

## 対象環境

* Unity 2022.3 以降

## 導入方法

### Package Managerから導入

```text
https://github.com/Suiraaaa/uoc-for-unity.git?path=/Packages/UocForUnity#v0.1.0
```

### `manifest.json` へ追加

```json
{
  "dependencies": {
    "com.suiraaaa.uoc": "https://github.com/Suiraaaa/uoc-for-unity.git?path=/Packages/UocForUnity#v0.1.0"
  }
}
```

## 基本的な使い方

### 1. `.uoc` ファイルをインポートする

Unityプロジェクトの `Assets` ディレクトリ以下へ、任意の `.uoc` ファイルを配置します。

```text
Assets/
└── Charts/
    └── sample.uoc
```

配置したファイルは、自動的に `UocAsset` としてインポートされます。

`UocAsset` は `ScriptableObject` です。通常のUnityアセットと同様に、`MonoBehaviour` のフィールド等から参照できます。

### 2. `UocObject` を取得する

次の例では、インポート済みの `UocAsset` をInspectorから割り当て、実行時に `UocObject` を取得します。

```csharp
using Uoc;
using UocForUnity;
using UnityEngine;

public sealed class UocAssetSample : MonoBehaviour
{
    [SerializeField]
    private UocAsset chart;

    private void Start()
    {
        UocObject uocObject = chart.Parse();

        var gameId = uocObject.ChartPropertyGroup.GetGameId();
        var ticksPerBeat = uocObject.ChartPropertyGroup.GetTpb();

        Debug.Log($"Game ID: {gameId}");
        Debug.Log($"Ticks Per Beat: {ticksPerBeat.Value}");
    }
}
```

`UocAsset.Parse()` は、保持しているUOC文字列を `uoc-for-c-sharp` の `UocParser` へ渡し、パース済みの `UocObject` を返します。

取得した `UocObject` から、譜面プロパティ、ノート、ノートグループ、イベント情報などへアクセスできます。

より詳しい操作方法については、[`uoc-for-c-sharp` のドキュメント](https://github.com/Suiraaaa/uoc-for-c-sharp) を参照してください。

### 3. 元のUOC文字列を取得する

インポート時の元文字列が必要な場合は、`UocAsset.Value` を使用します。

```csharp
using UocForUnity;
using UnityEngine;

public sealed class UocTextSample : MonoBehaviour
{
    [SerializeField]
    private UocAsset chart;

    private void Start()
    {
        string sourceText = chart.Value;

        Debug.Log(sourceText);
    }
}
```

## 譜面再生向けデータを生成する

`uoc-for-c-sharp` では、`UocObject` から譜面再生向けの解析済みデータを生成できます。

次の例では、`UocAsset` から `UocObject` を取得し、`AnalysisSetting` を指定して `ChartPlaybackData` を生成します。

```csharp
using Uoc;
using Uoc.Analyze;
using Uoc.Analyze.Playback;
using Uoc.Analyze.Speed;
using UocForUnity;
using UnityEngine;

public sealed class UocPlaybackDataSample : MonoBehaviour
{
    [SerializeField]
    private UocAsset chart;

    [SerializeField]
    private float noteMoveDurationMs = 1000f;

    private void Start()
    {
        UocObject uocObject = chart.Parse();

        var analysisSetting = new AnalysisSetting(
            basicSpeed: new BasicSpeed(noteMoveDurationMs),
            minimumTiming: -3000,
            ignoreSpeedChangesAfterJudgeLine: false,
            notesInstantiationInterval: 1000);

        ChartPlaybackData playbackData = uocObject.CreateChartPlaybackData(analysisSetting);

        // 生成されたplaybackDataを、
        // ゲーム固有の再生処理へ渡します。
    }
}
```

`AnalysisSetting` の値は、ゲームの仕様やユーザー設定に応じて決定してください。

たとえば、ノートスピードをゲーム内の設定画面から変更できるようにする場合は、`BasicSpeed` に渡す値をユーザー設定から取得します。

UOCの解析結果を実際にどのように利用するかは、ゲームごとに異なります。
このパッケージはゲーム固有の描画、判定、ノート生成処理を提供しません。

`ChartPlaybackData` や関連APIの詳細については、[`uoc-for-c-sharp` API仕様書](https://github.com/Suiraaaa/uoc-for-c-sharp/blob/0.1.0-alpha.1/SPEC.md) を参照してください。

## `UocAsset` について

`UocAsset` は、UnityへインポートされたUOCファイルを表す `ScriptableObject` です。

主に以下のメンバを提供します。

| メンバ              | 内容                                          |
| ------------------- | --------------------------------------------- |
| `Value`             | インポート元のUOC文字列を取得します           |
| `Parse()`           | UOC文字列をパースし、`UocObject` を生成します |

`.uoc` ファイルを `Assets` ディレクトリ以下へ配置すると、インポーターが自動的に `UocAsset` を生成します。

## 更新方法

新しいバージョンへ更新する場合は、Package Managerから新しいタグを指定したURLを再入力してください。

例:

```text
https://github.com/Suiraaaa/uoc-for-unity.git?path=/Packages/UocForUnity#v1.2.0
```

Gitタグを指定することで、プロジェクトで使用するパッケージのバージョンを指定できます。

## ドキュメント

* [`uoc-for-c-sharp`](https://github.com/Suiraaaa/uoc-for-c-sharp)
* [`uoc-for-c-sharp` API仕様書](https://github.com/Suiraaaa/uoc-for-c-sharp/blob/0.1.0-alpha.1/SPEC.md)
* [UOCフォーマット仕様](https://gist.github.com/Suiraaaa/188f4ec0639fde9834d7cb7ef057bf2c)
* [Releases](https://github.com/Suiraaaa/uoc-for-unity/releases)

## ライセンス

[MIT License](./LICENSE)
