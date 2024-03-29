# KosorenTool

このBeatSaberプラグインは、ScoreSaberなどのオンラインリーダーボードにスコアを送信せずにプレイをするためのツールです。

通常、このような場合はPRACTICE(練習)モードでプレイしますが、ローカルにもスコア記録が残らない問題があります。

このツールを使うと、スコア非送信(KOSOREN)モードでもスコアを記録して表示することができます。

また、スコア記録と同時にJump DistanceやReaction Timeも記録して表示するため、JDの調整にも役立つと思います。

他にも単ノーツスコアや精度が一定値以下の場合に一時停止させる機能(Notes Score,Accuracy Below Pause)があります。

KOSORENモードはSOLO(ソロ)プレイのときのみ有効になります。 マルチプレイやパーティモード、TournamentAssistantのプレイ時にはKOSORENモードの設定は無効になります。

Notes Score,Accuracy Below Pauseは、マルチプレイとTournamentAssistantのプレイ時には無効になります。

※これらのモードではKOSORENモードやNotes Score,Accuracy Below Pauseのスイッチがオンのときに警告表示は消えませんが、実際には無効になります。ただし、念のため無効にするようにしてください。

# インストール方法

1. 依存Modは`SiraUtil`と`BSML(BeatSaberMarkupLanguage)`と`BS Utils`です。基本Modなので入っていない人はいないと思います。
2. [リリースページか](https://github.com/rynan4818/KosorenTool/releases)らBeatSaberのバージョンにあったKosorenToolのリリースをダウンロードします。
3. ダウンロードしたzipファイルを`Beat Saber`フォルダに解凍して、`Plugin`フォルダに`KosorenTool.dll`ファイルをコピーします。

# 使い方

左のMODSタブにKOSORENTOOLが追加されます。

![image](https://github.com/rynan4818/KosorenTool/assets/14249877/16c6f34c-4203-4c49-82bb-70e5131395da)

* `KOSOREN` スイッチをオンにすると、スコア送信が無効になります。(ソロプレイ時のみ無効で、他のプレイモードはオンでも無効化されません)
* `Sort by Score`または`Sort by Date`で並べ替えがスコアが高い順か、日付の新しい順になります。`Memo`を選択するとメモファイルを表示します（下記参照）
* スコア表示は左から日付、スコア、ミス数、精度、Modifier (KOSORENモードは`KR`表示)、クリア有無(Fail時は残りノーツ数)、Jump Distance、Reaction Timeです。
* スコア表示は最大30件表示します。一番下までスクロールすると、追加設定があります。

![image](https://github.com/rynan4818/KosorenTool/assets/14249877/5c1c850e-8e13-4850-9b46-0ffc9991b898)

* `Notes Score Below Pause` スイッチをオンにすると、`Single Notes Score`で設定した点数以下のノーツ点数が出た場合に一時停止します。
* `Single Notes Score` 一時停止する基準スコアです。この点数以下でポーズします。
* `Accuracy Below Pause` スイッチをオンにすると、`Minimum Accuracy(%)`未満の精度になるとポーズします。
* `Minimum Accuracy(%)` 一時停止する基準精度です。この精度未満になるとポーズします。
* `Start Unchecked Time(%)` 精度チェックをしない曲の冒頭部分の割合(%)です。精度が安定する最初を除外するための設定になります。60秒の曲で10%の場合6秒まで精度をチェックしません。

![image](https://github.com/rynan4818/KosorenTool/assets/14249877/add84cc7-bf59-4d95-88c1-faffbfcefde7)

* `Show Failed` スイッチをオンにすると、Failしたスコアも表示します。
* `All Time Save` スイッチをオンにすると、KOSORENモード以外も全て記録します。オフはKOSORENモードのみです。
* `BeatSavior Targeted` スイッチをオンにすると、BeatSaviorの送信も無効化します。※BeatSaviorの送信管理はScoreSaberやBeatLeaderなどと別なのでこの設定がオンのときにKOSORENモードと連動します。

![image](https://github.com/rynan4818/KosorenTool/assets/14249877/359acbfe-6758-4ec9-8c93-6ad222e7abda)

`Notes Score Below Pause`で一時停止時には、その時のノーツのスコアが表示されます。

![image](https://github.com/rynan4818/KosorenTool/assets/14249877/06c37deb-b3a2-4bfd-88fd-459a23484c1e)

![image](https://github.com/rynan4818/KosorenTool/assets/14249877/f1e06956-d910-468b-9edd-e3ee8c9d937c)

![image](https://github.com/rynan4818/KosorenTool/assets/14249877/54ccde4c-0bb4-4b1f-aea3-cd0116352d6b)

* KOSORENモードや、Notes Score Below Pauseが有効のときは、正面上に上記の様に赤字で警告表示されます。

## メモファイルについて
スコア記録のソートで`Memo`を選択すると`UserData\KosorenToolMemo.txt`ファイルの中身を表示します。

<img src="https://github.com/rynan4818/KosorenTool/assets/14249877/28b444a9-562f-4d37-9713-ce7db678de6c" width="438" height="322">

一度BeatSaberを起動すると`UserData\KosorenToolMemo.txt`が作成されるので、そのファイルに書き込んで下さい。

最大３０行までにして下さい。横幅はフォントが等幅でないので、見ながら調整して下さい。

なお、[TextMeshProで表示しているのでタグが使えます](https://www.google.com/search?q=unity+TextMeshPro+%E3%82%BF%E3%82%B0)

日本語を使う場合は`UTF-8`の文字コードにして下さい。

<img src="https://github.com/rynan4818/KosorenTool/assets/14249877/950865a8-69f3-481c-a83b-36c340a89ec0" width="488" height="193">
