using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;

namespace prb_dotnetcore
{
    class Xml2Bin
    {

        XmlDocument xml;

        public Xml2Bin(string xmlpath)
        {
            xml = new XmlDocument();
            try
            {
                xml.Load(xmlpath);
            }
            catch
            {
                Console.WriteLine("エラー:XMLファイルの読み込みに失敗しました。");
                Environment.Exit(1);
            }
        }


        public List<byte[]> GetBinaryCodes()
        {
            var bin = new List<byte[]>();
            //初期化
            for (int i = 0; i <= 507; i++)
            {
                bin.Add(new byte[4] { 0xF0, 0xF0, 0xF0, 0xF0 });
            }
            for (int i = 507; i < 512; i++)
            {
                bin.Add(new byte[4] { 0, 0, 0, 0 });
            }
            try
            {
                var fobjs = xml.SelectNodes("mfx/chart/fobj");
                foreach (XmlNode fobj in fobjs)
                {
                    var ins = new byte[4] { 0, 0, 0, 0 };
                    //子を取得
                    XmlNode eri8obj = null;
                    XmlNode next = null;
                    XmlNode branch = null;
                    foreach (XmlNode child in fobj.ChildNodes)
                    {
                        if (child.Name == "eri8obj") eri8obj = child;
                        else if (child.Name == "next") next = child;
                        else if (child.Name == "branch") branch = child;
                    }
                    //命令名を取得。
                    string instype = eri8obj.Attributes["name"].Value;

                    //nextが指定されていない場合end_flow以外は無視する。
                    if (next is null && instype != "end_flow") continue;

                    //<eri8obj>のargo属性によって動作命令か制御命令か判断する。
                    if (eri8obj.Attributes["argo"].Value == "4")
                    {
                        //動作命令
                        //<eri8obj>の中身情報を切り取る
                        byte[] vals = (from v in eri8obj.InnerText.Split(',') select Hex2byte(v)).ToArray();

                        //ins[0] = (命令固有値 + 速度固有値) 速度固有値はvals[0] * 0x10
                        byte insUnique = 0;
                        if (instype == "op_fwd") insUnique = 0x01;
                        else if (instype == "op_back") insUnique = 0x02;
                        else if (instype == "op_rturn") insUnique = 0x03;
                        else if (instype == "op_lturn") insUnique = 0x04;
                        else if (instype == "op_rtwist") insUnique = 0x05;
                        else if (instype == "op_ltwist") insUnique = 0x06;
                        else if (instype == "op_stop") insUnique = 0x07;
                        else if (instype == "op_brturn") insUnique = 0x0B;
                        else if (instype == "op_blturn") insUnique = 0x0C;
                        else
                        {

                            Console.WriteLine("エラー:無効な動作命令です。");
                            Environment.Exit(1);
                        }
                        ins[0] = (byte)(insUnique + vals[0] * 0x10);

                        //次命令行番号は共通
                        ins[1] = Id2insLineNumber(next.Attributes["id"].Value);

                        //実行時間も共通
                        ins[2] = vals[1];
                        ins[3] = vals[2];
                    }
                    else if (eri8obj.Attributes["argo"].Value == "2")
                    {
                        //制御命令
                        if (instype == "start_flow")
                        {
                            ins[0] = 0xC1;
                            ins[1] = 0x00;
                            ins[2] = 0x02;
                            ins[3] = Id2insLineNumber(next.Attributes["id"].Value);
                            if (ins[3] == 1)
                            {
                                Console.WriteLine("エラー:開始命令が終了命令と直結しています。");
                                Environment.Exit(1);
                            }
                        }
                        else if (instype == "end_flow")
                        {
                            ins[0] = 0xC2;
                            ins[1] = 0x00;
                            ins[2] = 0x01;
                            ins[3] = 0x01;
                        }
                        else if (instype == "loop_begin")
                        {
                            ins[0] = 0xD1;
                            ins[1] = Id2insLineNumber(next.Attributes["id"].Value);
                            ins[2] = 0x01;
                            ins[3] = Hex2byte(eri8obj.InnerText);
                        }
                        else if (instype == "loop_end")
                        {
                            ins[0] = 0xD2;
                            ins[1] = Id2insLineNumber(next.Attributes["id"].Value);
                            ins[2] = 0x01;
                            ins[3] = 0xF4;
                        }
                        else if (instype.Contains("op_sensor"))
                        {
                            //センサー番号の取得
                            int snum = int.Parse(instype.Substring(instype.Length - 1));
                            ins[0] = (byte)(0x80 + snum);
                            ins[1] = 0x00;
                            ins[2] = Id2insLineNumber(next.Attributes["id"].Value);
                            ins[3] = Id2insLineNumber(branch.Attributes["id"].Value);
                        }
                        else
                        {
                            Console.WriteLine("エラー:無効な制御命令です。");
                            Environment.Exit(1);
                        }
                    }
                    else
                    {
                        Console.WriteLine("エラー:XMLファイルの解釈に失敗しました。");
                        Environment.Exit(1);
                    }
                    bin[Id2insLineNumber(fobj.Attributes["id"].Value)] = ins;
                }
            }

            catch
            {
                Console.WriteLine("エラー:XMLファイルの解釈に失敗しました。");
                Environment.Exit(1);
            }
            return bin;

            //Local Funcs

            //Hex2byte "0x20"の形式のテキストをbyteに強制変換
            byte Hex2byte(string hex)
            {
                return byte.Parse(hex.Substring(2), System.Globalization.NumberStyles.HexNumber);
            }

            //Id2insLineNumber Idを命令行番号に変える。-1するだけだけどね。
            byte Id2insLineNumber(string id)
            {
                return (byte)(byte.Parse(id) - 1);
            }

        }

        static public void SaveBinary(string path, List<byte[]> bin)
        {
            try
            {
                using (var bw = new System.IO.BinaryWriter(System.IO.File.OpenWrite(path)))
                {
                    foreach(var arr in bin)
                    {
                        foreach(var b in arr)
                        {
                            bw.Write(b);
                        }
                    }
                    bw.Close();
                }
                Console.WriteLine("情報:バイナリデータが" + path + "に保存されました。");
            }
            catch 
            {
                Console.WriteLine("エラー:バイナリデータを保存できませんでした。");
                Environment.Exit(1);
            }
        }


    }
}