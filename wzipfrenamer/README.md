# wzipfrenamer

IPFファイルの生成を監視して特定のフォルダに移動させます。

フォルダ構成の制限があります。

```
basedir -- 監視対象
└ addonName
├ addon_d.ipf
│ ├ xxx.lua
│ └ xxx.xml
└ xxx.ipf -- ipftool等で生成
...
└ addonName
```

## Usage

wzipfrenamer.exe <WatchDir>

## Settings

### AddonFolder

必須。

ipfファイルのコピー先を指定します。

### UnicodeChar

必須。デフォルト：📖

付与する絵文字を指定します。

### WaitSecToFileWritten

任意。デフォルト：3

ipfファイルの書き込み完了を待つ時間を秒数で指定します。

## parameters

### WatchDir

必須。

監視対象のフォルダを指定します。
