using SaiTer.ATE.EquipMent;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    public class PowerAnalyzerControl : PowerAnalyzerBase
    {
        private string[] ClassNames = new string[] { "emtPA6500", "emtPA6000", "emtWT333E", "emtWT5000" };

        public override void IntegralClear(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {

                    item.Value.IntegralClear();
                }
            }
        }

        public override void IntegralStart(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {

                    item.Value.IntegralStart();
                }
            }
        }

        public override void IntegralStop(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {

                    item.Value.IntegralStop();
                }
            }
        }

        public override double ReadIntegralValue(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {

                    return item.Value.ReadIntegralValue();
                }
            }
            return -1;
        }

        public override double ReadCurrentHarmonicValue(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {

                    return item.Value.ReadCurrentHarmonicValue();
                }
            }
            return -1;
        }

        public override void Integral123Start(List<int> lstIDs,int iState)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Integral123Start(iState);
                }
            }
        }
        /// <summary>
        /// 设置缩放功能开关
        /// </summary>
        /// <param name="state">关闭=0，打开=1</param>
        public override void SetScalingState(List<int> lstIDs, int state)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {

                    item.Value.SetScalingState(state);
                }
            }
        }
		
        /// <summary>
        /// 设置通道变比
        /// </summary>
        /// <param name="iCH">通道</param>
        /// <param name="isU">是否电压</param>
        /// <param name="iRatio">变比</param>
        public override void SetChannelRatio(List<int> lstIDs, int iCH, bool isU, int iRatio)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.SetChannelRatio(iCH, isU, iRatio);
                }
            }
        }


        public override double ReadDcComponentVoltage(List<int> lstIDs, int iCH)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    return item.Value.ReadDcComponentVoltage(iCH);
                }
            }
            return -1;
        }


        public override double ReadDcComponentCurrent(List<int> lstIDs, int iCH)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    return item.Value.ReadDcComponentCurrent(iCH);
                }
            }
            return -1;
        }

        public override double ReadFreq(List<int> lstIDs, int iCH)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    return item.Value.ReadFreq(iCH);
                }
            }
            return -1;
        }

        public override List<double> ReadCurrentHarmonicValue_50(List<int> lstIDs, int iCH)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    return item.Value.ReadCurrentHarmonicValue_50(iCH);
                }
            }
            return new List<double>();
        }

        public override List<double> ReadVoltageHarmonicValue_50(List<int> lstIDs, int iCH)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    return item.Value.ReadVoltageHarmonicValue_50(iCH);
                }
            }
            return new List<double>();
        }

        public override void SetHarmonicState(List<int> lstIDs, int iCH, bool isON)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.SetHarmonicState(iCH, isON);
                }
            }
        }

        public override double ReadCurrentHarmonic_Total(List<int> lstIDs, int iCH)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {

                    return item.Value.ReadCurrentHarmonic_Total(iCH);
                }
            }
            return -1;
        }

        public override double ReadVoltageHarmonic_Total(List<int> lstIDs, int iCH)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {

                    return item.Value.ReadVoltageHarmonic_Total(iCH);
                }
            }
            return -1;
        }
    }
}
