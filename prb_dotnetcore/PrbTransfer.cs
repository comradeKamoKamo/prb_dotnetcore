using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using HidSharp;
namespace prb_dotnetcore
{
    class PrbTransfer
    {
        private HidDevice prbDevice;
        private List<byte> byteCodes = new List<byte>();
        private List<byte> data = new List<byte>();

        public PrbTransfer(List<byte[]> bin)
        {
            //命令セットをバイトコードに変換
            foreach (var arr in bin)
            {
                byteCodes.AddRange(arr);
            }
            //データを変換
            Preparation();

        }

        public void WriteData()
        {
            Console.WriteLine("情報:転送を開始します。");
            HidStream ps = null;
            try
            {
                ps = prbDevice.Open();
            }
            catch
            {
                Console.WriteLine("エラー:プロロボを開けませんでした。プロロボへのアクセス権限が十分でない可能性があります。");
                Environment.Exit(1);
            }
            //書き込み...自分でも意味わからん。
            int offset = 0;
            int tryMax = 5;
            byte[] ends = new byte[65];
            for (int i = 0; i < 14; i++)
            {
                byte[] wrt = data.GetRange(offset, 65).ToArray();
                bool[] rslts = new bool[tryMax];
                bool tCnt = false;
                for (int c = 0; c < tryMax; c++)
                {
                    try
                    {
                        ps.Write(wrt);
                        rslts[c] = true;
                    }
                    catch
                    {
                        rslts[c] = false;
                    }
                    
                    if (rslts[c] && tCnt) break;
                    if (rslts[c]) tCnt = true;
                }
                if (!rslts.Any(n => true))
                {
                    Console.WriteLine("エラー:プロロボへの転送に失敗しました。");
                    Environment.Exit(1);
                }
                ends = wrt;
                offset += 37;
            }
            bool etCnt = false;
            ends[0] = 0;
            ends[1] = 8;
            ends[2] = 0;
            ends[3] = 0;
            ends[4] = 0;
            bool[] endRslts = new bool[tryMax];
            for (int c = 0; c < tryMax; c++)
            {
                try
                {
                    ps.Write(ends);
                    endRslts[c] = true;
                }
                catch
                {
                    endRslts[c] = false;
                }
                if (endRslts[c] && etCnt) break;
                if (endRslts[c]) etCnt = true;
            }
            if (!endRslts.Any(n => true))
            {
                Console.WriteLine("エラー:プロロボへの転送に失敗しました。");
                Environment.Exit(1);
            }
            Console.WriteLine("\n情報:プロロボへの転送に成功しました。");
        }

        private void Preparation()
        {
            //事前変換処理
            int bCnt = 0;
            data.Add(0);
            data.Add(1);
            data.Add(0);
            data.Add(0);
            data.Add(32);
            data.Add(0);
            data.Add(0);
            data.Add(0);
            data.Add(0);
            for (int i = 0; i < 28; i++)
            {
                data.Add(byteCodes[bCnt]);
                bCnt++;
            }
            byte twentyCnt = 32;
            for (int i = 0; i < 7; i++)
            {
                data.Add(0);
                data.Add(1);
                data.Add(twentyCnt);
                data.Add(0);
                data.Add(32);
                for (int c = 0; c < 32; c++)
                {
                    data.Add(byteCodes[bCnt]);
                    bCnt++;
                }
                twentyCnt += 32;
            }
            twentyCnt = 0;
            for (int i = 0; i < 8; i++)
            {
                data.Add(0);
                data.Add(1);
                data.Add(twentyCnt);
                data.Add(1);
                data.Add(32);
                for (int c = 0; c < 32; c++)
                {
                    data.Add(byteCodes[bCnt]);
                    bCnt++;
                }
                twentyCnt += 32;
            }
            //FF
            data.AddRange(
                Enumerable.Repeat<byte>
                (0xFF, 1024 - data.Count).ToList());
        }

        public void ConnectPrb()
        {
            prbDevice = DeviceList.Local.GetHidDeviceOrNull(vendorID: 0x2431, productID:0x0311);
            if(prbDevice is null)
            {
                Console.WriteLine("エラー:プロロボが見つかりませんでした。");
                Environment.Exit(1);
            }
            Console.WriteLine("情報:プロロボが見つかりました。");
        }
    }
}
