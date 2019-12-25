using System;
using System.Linq;

namespace prb_dotnetcore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("prb_dotnetcore version 0.5 bate\n");
            if(args.Length == 0)
            {
                Console.WriteLine("エラー:引数が指定されていません。--helpを指定するとヘルプを表示できます。");
                Environment.Exit(1);
            }
            if(args.Any(a => a == "--help" || a == "-h"))
            {
                Console.WriteLine(
                    "prb_dotnetcore xmlfilepath [--notransfer] [--help]\n\n" +
                    "prb_donetcore はプロロボのプログラムのコンパイルと転送を行います。\n" +
                    "Options:\n" +
                    "xmlfilepath : プロロボ公式エディタで作成したプログラムのxmlファイルのパスを指定します。\n" +
                    "--notransfer -n : コンパイルのみを行い、転送を行いません。\n" +
                    "--help -h : このヘルプを表示します。より詳細な情報はhttps://github.com/comradeKamoKamo/prb_dotnetcore/を確認してください。"
                    ) ;
                Environment.Exit(0);
            }

            //xmlからバイナリコードを生成する。
            Xml2Bin xml2Bin = new Xml2Bin(args[0]);
            var bin = xml2Bin.GetBinaryCodes();
            Xml2Bin.SaveBinary(System.IO.Path.GetFileNameWithoutExtension(args[0]) + ".bin", bin);

            //notransfer
            bool notransfer = args.Any(a => a == "--notransfer" || a == "-n");
            if (notransfer) Environment.Exit(0);

            //転送を行う。
            PrbTransfer prbTransfer = new PrbTransfer(bin);
            prbTransfer.ConnectPrb();
            prbTransfer.WriteData();

            Environment.Exit(0);
        }
    }
}
