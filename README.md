# KosorenTool

このBeatSaberプラグインは、ScoreSaberなどのオンラインリーダーボードにスコアを送信せずにプレイをするためのツールです。

通常、このような場合はPRACTICE(練習)モードでプレイしますが、ローカルにもスコア記録が残らない問題があります。

このツールを使うと、スコア非送信(KOSOREN)モードでもスコアを記録して表示することができます。

また、スコア記録と同時にJump DistanceやReaction Timeも記録して表示するため、JDの調整にも役立つと思います。

KOSORENモードはSOLO(ソロ)プレイのときのみ有効になります。 マルチプレイやパーティモード、TournamentAssistantのプレイ時には無効なるようにしてあります。

※これらのモードではKOSORENモードのスイッチがオンのときに警告表示は消えませんが、実際には無効になります。ただし、念のため無効にするようにしてください。

# インストール方法

1. 依存Modは`SiraUtil`と`BSML(BeatSaberMarkupLanguage)`と`BS Utils`です。基本Modなので入っていない人はいないと思います。
2. [リリースページか](https://github.com/rynan4818/KosorenTool/releases)らBeatSaberのバージョンにあったKosorenToolのリリースをダウンロードします。
3. ダウンロードしたzipファイルを`Beat Saber`フォルダに解凍して、`Plugin`フォルダに`PlayerInfoViewer.dll`ファイルをコピーします。

# 使い方

左のMODタブにKOSORENTOOLが追加されます。

![image](https://github.com/rynan4818/KosorenTool/assets/14249877/16c6f34c-4203-4c49-82bb-70e5131395da)

* `KOSOREN` スイッチをオンにすると、スコア送信が無効になります。(ソロプレイ時のみ無効で、他のプレイモードはオンでも無効化されません)
* `Sort by Score`または`Sort by Date`で並べ替えをスコアが高い順か、日付の新しい順にならびます。
* スコア表示は左から日付、スコア、ミス数、精度、Modifier (KOSORENモードは`KR`表示)、クリア有無(Fail時は残りノーツ数)、Jump Distance、Reaction Timeです。
* スコア表示は最大30件表示します。一番下までスクロールすると、追加設定があります。

![image](https://github.com/rynan4818/KosorenTool/assets/14249877/a4851094-69c8-47be-a9f4-8a9114ba132e)

* `Show Failed` スイッチをオンにすると、Failしたスコアも表示します。(デフォルトオン)
* `All Time Save` スイッチをオンにすると、KOSORENモード以外の全て記録します。オフはKOSORENモードのみです。
* `BeatSavior Targeted` スイッチをオンにすると、BeatSaviorの送信も無効化します。※BeatSaviorの送信管理はScoreSaberやBeatLeaderなどと別なのでこの設定がオンのときにKOSORENモードと連動します。

![image](https://github.com/rynan4818/KosorenTool/assets/14249877/06c37deb-b3a2-4bfd-88fd-459a23484c1e)

* KOSORENモードが有効のときは、正面上に`KOSOREN Enabled!`が赤字で表示されます。

