using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.CAN
{
    public class MutiPackage
    {
        private string Symbol;
        private int PlanCnt;
        private int AppendCnt;
        private int PackageId;
        private int TextId;
        public bool Finish { get; set; }
        List<byte> CollectData;
        public MutiPackage()
        {
            this.Symbol = string.Empty;
            AppendCnt = 0;
            PackageId = 0;
            TextId = 0;
            CollectData = new List<byte>();
        }
        public void UpdateMutiPackage(EachFrameModel model)
        {


        }
        //public string GetReadyOrFinish(List<byte> content)
        //{
        //    string readyOrFinish = BaseConvert.AsciiBytes2String(content, 0, 2);

        //    if (readyOrFinish == "11")
        //        return "准备";
        //    else  //"13"
        //        return "完成";
        //}
        public string UpdateMutiPackage_Head(List<byte> content)
        {
            string[] arr = Function.SplitMsgData(content);

            this.AppendCnt = 0;
            this.PackageId++;
            this.TextId++;
            this.PlanCnt = SetCountPlan(arr);
            this.Symbol = DecodeSymbol(content);
            this.CollectData.Clear();
            return this.Symbol;
        }
        public string UpdateMutiPackage_Ready(List<byte> content)
        {
            this.Symbol = DecodeSymbol(content);
            return this.Symbol;
        }
        public void AppendContentPackage(List<byte> content)
        {
            content.RemoveAt(0);
            this.AppendCnt += 7; //每一包7个字节
            CollectData.AddRange(content);
        }
        private int SetCountPlan(string[] arr)
        {
            string hex = arr[3];
            int len = BaseConvert.HexStr2Int32(hex);
            return len;
        }
        //private int UpdateCntReady(List<byte> content)
        //{

        //}
        private string DecodeSymbol(List<byte> content)
        {
            //byte[] arr = content.ToArray();
            string msgData = content[6].ToString("x2");
            string symbol = Function.GetSymbolByPgn(msgData);
            return symbol;
        }

        public string GetSymbol()
        {
            return this.Symbol;
        }
        public int GetCountPlan()
        {
            return this.PlanCnt;
        }
        public List<byte> GetMutiContent()
        {
            return CollectData;
        }
        public int GetAppendCnt()
        {
            return this.AppendCnt;
        }
        public int GetCurrentPackageId()
        {
            return this.PackageId;
        }
        public int GetCurrentTextId()
        {
            return this.TextId;
        }
    }
}
