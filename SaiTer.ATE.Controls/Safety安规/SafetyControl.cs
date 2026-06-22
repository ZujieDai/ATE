using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    public class SafetyControl : SafetyBase
    {
        private string[] ClassNames = new string[] { "emtSafety" , "emtSafety_SE7441" };
        public SafetyControl()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="SchemeID"></param>
        /// <param name="SchemeName"></param>
        /// <returns></returns>
        public override bool SafetyInit(List<int> lstIDs, string SchemeID, string SchemeName, bool isSave = false)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(new string[] { "emtSafety_SE7441" }))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    return item.Value.SafetyInit(SchemeID, SchemeName, isSave);
                }
                else
                {
                    continue;
                }
            }
            return false;
        }
        public override void SafetyOFF(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Safety_OFF();
                }
                else
                {
                    continue;
                }
            }
        }
        public override void SafetySetParam(List<int> lstIDs, string AsciiSendStr, string AsciiOutEndFlag = "\r\n", string AsciiInEndFlag = "\r\n", int SleepTime = 100)
        {

            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();

                if (id.Length > 0)
                {
                    item.Value.SafetySetParam(AsciiSendStr, AsciiOutEndFlag, AsciiInEndFlag);
                    Thread.Sleep(SleepTime);
                }
                else
                {
                    continue;
                }
            }


        }

        public override bool SafetyReadParam(List<int> lstIDs, string AsciiSendStr, string AsciiOutEndFlag = "\r\n", string AsciiInEndFlag = "\r\n")
        {

            bool isok = false;
            string strResult = "NULL";
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                StResultData Resultdata = new StResultData();
                Resultdata.LstData = new List<object>();

                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();

                if (id.Length > 0)
                {
                    Resultdata.ChargeId = id[0];
                    item.Value.SafetyReadParam(AsciiSendStr, AsciiOutEndFlag, AsciiInEndFlag, ref strResult);
                    if (!string.IsNullOrEmpty(strResult))
                    {
                        Resultdata.LstData.Add(strResult);
                    }
                    else
                    {
                        Resultdata.LstData = null;
                    }
                }
                else
                {
                    Resultdata.LstData = null;
                    continue;
                }
                SendResultData(Resultdata);
            }

            return isok;
        }
        public override string SafetyReadResult(List<int> lstIDs, string AsciiSendStr, string AsciiOutEndFlag = "\r\n", string AsciiInEndFlag = "\r\n")
        {
            string strResult = "NULL";
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {

                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();

                if (id.Length > 0)
                {

                    item.Value.SafetyReadParam(AsciiSendStr, AsciiOutEndFlag, AsciiInEndFlag, ref strResult);

                }
            }

            return strResult;
        }

        public override void PauseReadSafetyStateData(List<int> lstIDs, bool IsPause)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();

                if (id.Length > 0)
                {
                    item.Value.PauseReadSafetyStateData(IsPause);
                }
                else
                {
                    continue;
                }
            }
        }
    }
}
