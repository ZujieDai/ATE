using SaiTer.ATE.EquipMent;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    /// <summary>
    /// 交流源设备控制
    /// </summary>
    public class ACSourceControl : ACSourceBase
    {
        private string[] ClassNames = new string[] { "emtACSource_SPWY", "emtACSource_STAS", "emtACSource_CtrlBoard", "emtACSource_HY", "emtACSource_XH", "emtACSource_AN", "emtACSource_GT", "emtACSource_TMP", "emtACSource_AKSB" };
        
        private void SetCtrlBoard_ACSource(List<int> lstIDs, KeyValuePair<int, EquipMentBase> item, bool isClose)
        {
            #region 如果有电网程控板控制
            List<int> Switches = new List<int>();
            string[] ctrl_switch = ConfigurationManager.AppSettings["CtrlACSourceSwitch"]?.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            if (ctrl_switch != null && ctrl_switch.Length > 0)
            {
                foreach (string index in ctrl_switch)
                {
                    if (int.TryParse(index, out int result) && result > 0 && result < 17)
                        Switches.Add(result - 1);
                }
                //如果不为程控板而且配置了闭合电网开关
                if (Switches.Count > 0 && !(item.Value is emtACSource_CtrlBoard))
                {
                    var cb = DitControlEquipMent("emtControlBoard");
                    if (cb.Values.First() is emtControlBoard controlBoard)
                    {
                        List<bool> lstConditionState = controlBoard.ControlBoardReadState();
                        for (int i = 0; i < Switches.Count; i++)
                            lstConditionState[Switches[i]] = isClose;
                        controlBoard.ControlResistanceSetRelay(lstConditionState);
                    }
                }
            }
            #endregion

            #region 可能需要闭合继电器才可以交流源输出
            foreach (KeyValuePair<int, EquipMentBase> dio in DitControlEquipMent(new string[] { "emtDIORelay" }))
            {
                int[] id = lstIDs.Intersect(dio.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    if (dio.Value is emtDIORelay emtDIORelay)
                    {
                        string[] configParams = EquipmentConfigManage.GetConfigParams((int)lstChargerInfo.Find(c => c.ChargerId == id[0])?.ChargerType, "AC_Output", "emtDIORelay", "");
                        if (configParams == null)
                            return;
                        if (configParams.Length > 0 && configParams[0] != null)
                        {
                            string[] closeRelay = configParams[0].Split('=')[1].Split('|');
                            foreach (string str in closeRelay)
                            {
                                emtDIORelay.SetRelaySwitch(Convert.ToUInt32(str) - 1, isClose);
                            }
                        }
                    }
                }
            }
            #endregion
        }

        public override void ACSource_ON(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                bool isThis = id.Length > 0;
                //如果是单交流源对多枪供电，应该设置1号交流源
                if (!lstIDs.Contains(1) && DitControlEquipMent(ClassNames).Count == 1)
                {
                    isThis = true;
                }
                if (isThis)
                {
                    SetCtrlBoard_ACSource(lstIDs, item, true);
                    item.Value.ACSource_ON();

                    //交流源的频率和枪信息的一致
                    if (ChargerInfoManage.SelectChargerInfo(out lstChargerInfo))
                    {
                        if (lstChargerInfo.Count > 0)
                        {
                            var ChargerInfo = lstChargerInfo.First(s => s.ChargerId.Equals(lstIDs[0]));
                            item.Value.ACSource_SetFreq(ChargerInfo.Frequency);
                        }
                    }
                }
            }
        }
        public override void ACSource_OFF(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                bool isThis = id.Length > 0;
                //如果是单交流源对多枪供电，应该设置1号交流源
                if (!lstIDs.Contains(1) && DitControlEquipMent(ClassNames).Count == 1)
                {
                    isThis = true;
                }
                if (isThis)
                {
                    SetCtrlBoard_ACSource(lstIDs, item, false);
                    item.Value.ACSource_OFF();
                }
            }
        }

        public override void ACSource_SetVolt(List<int> lstIDs, double voltage)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                bool isThis = id.Length > 0;
                //如果是单交流源对多枪供电，应该设置1号交流源
                if (!lstIDs.Contains(1) && DitControlEquipMent(ClassNames).Count == 1)
                {
                    isThis = true;
                }
                if (isThis)
                {
                    item.Value.ACSource_SetVolt(voltage);
                }
            }
        }
        public override void ACSource_SetFreq(List<int> lstIDs, double freq)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                bool isThis = id.Length > 0;
                //如果是单交流源对多枪供电，应该设置1号交流源
                if (!lstIDs.Contains(1) && DitControlEquipMent(ClassNames).Count == 1)
                {
                    isThis = true;
                }
                if (isThis)
                {
                    item.Value.ACSource_SetFreq(freq);
                }
            }
        }


        public override void ACSource_DisConnect(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                bool isThis = id.Length > 0;
                //如果是单交流源对多枪供电，应该设置1号交流源
                if (!lstIDs.Contains(1) && DitControlEquipMent(ClassNames).Count == 1)
                {
                    isThis = true;
                }
                if (isThis)
                {
                    item.Value.ACSource_DisConnect();
                }
            }
        }

        public override void ACSource_SetVolt3(List<int> lstIDs, double VoltA, double VoltB, double VoltC)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                bool isThis = id.Length > 0;
                //如果是单交流源对多枪供电，应该设置1号交流源
                if (!lstIDs.Contains(1) && DitControlEquipMent(ClassNames).Count == 1)
                {
                    isThis = true;
                }
                if (isThis)
                {
                    item.Value.ACSource_SetVolt3(VoltA, VoltB, VoltC);
                }
            }
        }

        public override void ACSource_SetAngle3(List<int> lstIDs, double AngleA, double AngleB, double AngleC)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                bool isThis = id.Length > 0;
                //如果是单交流源对多枪供电，应该设置1号交流源
                if (!lstIDs.Contains(1) && DitControlEquipMent(ClassNames).Count == 1)
                {
                    isThis = true;
                }
                if (isThis)
                {
                    item.Value.ACSource_SetAngle3(AngleA, AngleB, AngleC);
                }
            }
        }

        public override void ACSource_SetOpenPhase(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                bool isThis = id.Length > 0;
                //如果是单交流源对多枪供电，应该设置1号交流源
                if (!lstIDs.Contains(1) && DitControlEquipMent(ClassNames).Count == 1)
                {
                    isThis = true;
                }
                if (isThis)
                {
                    item.Value.ACSource_SetOpenPhase();
                }
            }
        }

    }
}
