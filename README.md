# prb_dotnetcore プロロボ on Linux or Mac
　prb_dotnetcoreはヤマザキ教育システム社の
 「[制御学習プロロボUSBプラス(YJ-U5型)](http://www.yamazaki-kk.com/technique/detail.php?id=58&t=1&c=5)」を
 Linux,Mac OSを含めてあらゆるプラットフォームでプログラミングするための非公式ユーティリティです。  
　 [公式サイト](http://www.yamazaki-kk.com/t_materials/)からダウンロードできるヴィジュアルプログラミングエディタはWindows専用であり、それ以外のOSでは、
 WINEを用いてもプロロボにデータを転送することができません。prb_dotnetcoreは.NET Core 3.1のランタイムがインストールされている環境において、
 公式のエディタで作成されたプログラムを含むXMLファイルを、その内容をプロロボ用の16進コードに変換した上で、USB通信でプロロボに転送します。

# インストール
　現在のバージョンは0.6 betaです。
## 1 .NET Core 3.1 Runtimeのインストール
>Download .NET - Downloads for .NET Framework and .NET Core, including ASP.NET and ASP.NET Core
>https://dotnet.microsoft.com/download  

上記より .NET Core 3.1のランタイムをダウンロード＆インストールしてください。
**Runtimeのみ必要です。** SDKやASP.NET Coreは動作に必要ありません。

## 2.ダウンロードと展開
 ~~~
 $ wget https://github.com/comradeKamoKamo/prb_dotnetcore/raw/master/prb_dotnetcore0.6b.tgz
 $ tar xzvf prb_dotnetcore0.6b.tgz
 $ cd prb_dotnetcore0.6b
 $ dotnet prb_dotnetcore.dll --help
 ~~~
ヘルプが表示されれば成功です。上はubuntuでの例です。

# 使い方
プロロボを接続した状態で、
~~~
$ dotnet prb_dotnetcore.dll example.xml
~~~
を実行します。転送に成功したらプロロボを取り外してください。XMLファイルのプログラムが不正でないかどうかはチェックされません。   

**注意**:LinuxではプロロボのようなHIDにアクセスするには権限を適切に設定する必要があります。権限が不足している場合、
プロロボを開けなかった旨のエラーが起こります。最もかんたんな方法は```sudo dotnet prb_dotnetcore.dll example.xml```でroot権限で実行することです。
また、プロロボがOSに認識されているかどうかは```lsusb```コマンドでvnderID:0x2431のデバイスが存在するかを確認してください。  

また、途中で出力されるバイナリファイル*.binは公式エディタの「機械語コード転送」では利用できません。また、公式エディタにある補正と電圧確認には未対応です。

# 更新履歴
Version 0.6b - VenderIDのみでプロロボを識別していたミスをProductIDとともに識別するように修正しました。  
Version 0.5b - 初公開
# 参考URL
[PrbScript](https://ttnohp.web.fc2.com/prb/) 開発者は同じ人です。バイナリコードの仕様などはこちらで見れます。  
[Works on Linux, but only as root? (Page 1) — Discussion — Zer's Programming Page](https://forum.zer7.com/topic/10068/) LinuxでのHIDの権限不足について。
