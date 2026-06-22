using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.BMS;
using SaiTer.ATE.DataModel.CAN;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace SaiTer.ATE.Controls
{
    /// <summary>
    /// BMS导引控制
    /// </summary>
    public class BMSControl : BMSBase
    {
        ESGBDC_Ver eSGBDC_Ver;
        public int MyProperty { get; set; }
        private string[] ClassNames = null;

        /*
         * 操作逻辑：每种导引都遍历一遍，有此设备并包含需要测试的枪ID,就下发指令
         *                             
         *          在业务层就不需要区分是哪种类型的导引
         *          其它相似的设备也可以做此处理
         * 实际环境中留意是否会引起意料外的问题
         */

        public BMSControl()
        {
            SystemEvent.CanProtocolVersionSW += BMSControl_CanProtocolVersionSW;
        }
        public void BMSControlInit()
        {
            if (lstChargerInfo == null || lstChargerInfo.Count == 0)
                ChargerInfoManage.SelectChargerInfo(out lstChargerInfo);
            if (lstChargerInfo == null || lstChargerInfo.Count == 0)
            {
                ClassNames = new string[] { "emtBMS_AC", "emtBMS_GB_DC", "emtBMS_EU_DC", "emtBMS_USA_DC", "emtBMS_JP_DC" };
                return;
            }

            if (ClassNames == null || ClassNames.Length == 0 || ClassNames.ToList().Find(c => c.Replace("emtBMS_", "") == lstChargerInfo.First()?.ChargerType.ToString().Replace("Charger_", "")) == null)
            {
                switch (lstChargerInfo.First()?.ChargerType)
                {
                    case EmChargerType.Charger_GB_AC:
                    case EmChargerType.Charger_USA_AC:
                    case EmChargerType.Charger_EUR_AC:
                    case EmChargerType.Charger_NACS_AC:
                        ClassNames = new string[] { "emtBMS_AC" };
                        break;
                    case EmChargerType.Charger_GB_DC:
                        ClassNames = new string[] { "emtBMS_GB_DC" };
                        break;
                    case EmChargerType.Charger_EUR_DC:
                        ClassNames = new string[] { "emtBMS_EU_DC" };
                        break;
                    case EmChargerType.Charger_USA_DC:
                    case EmChargerType.Charger_NACS_DC:
                        ClassNames = new string[] { "emtBMS_USA_DC" };
                        break;
                    case EmChargerType.Charger_JP_DC:
                        ClassNames = new string[] { "emtBMS_JP_DC" };
                        break;
                    default:
                        ClassNames = new string[] { "emtBMS_AC", "emtBMS_GB_DC", "emtBMS_EU_DC", "emtBMS_USA_DC", "emtBMS_JP_DC" };
                        break;
                }
            }
        }

        private void BMSControl_CanProtocolVersionSW(ESGBDC_Ver ver)
        {
            eSGBDC_Ver= ver;
        }

        public override void BMS_OFF(List<int> lstIDs, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMS_OFF();

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }

        }

        public override void BMS_ON(List<int> lstIDs, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMS_ON();

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        public override void BMS_GetResistance(List<int> lstIDs, ref ushort tR2, ref ushort tR3, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.BMS_GetResistance(ref tR2, ref tR3);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        public override void BMS_SetResistance(List<int> lstIDs, ushort tR2, ushort tR3, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.BMS_SetResistance(tR2, tR3);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        public override Dictionary<int, List<bool>> BMS_GetKState(List<int> lstIDs, string[] classNames = null)
        {
            Dictionary<int, List<bool>> dic = new Dictionary<int, List<bool>>();
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        List<bool> lst = item.Value.BMS_GetKState();
                        dic.Add(id[0], lst);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }

            return dic;
        }


        public override Dictionary<int, List<bool>> BMSGetKState_DC(List<int> lstIDs, out double resistance, out double batteryVoltage, string[] classNames = null)
        {
            Dictionary<int, List<bool>> dic = new Dictionary<int, List<bool>>();
            resistance = 0;
            batteryVoltage = 0;
            try
            {
                BMSControlInit();
                classNames = classNames ?? ClassNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        List<bool> lst = item.Value.BMSGetKState_DC(out resistance, out batteryVoltage);
                        if (lst != null)
                        {
                            dic.Add(id[0], lst);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }

            return dic;
        }
        public override Dictionary<int, List<int>> BMSGetKState_EU_DC(List<int> lstIDs, out double batteryVoltage, string[] classNames = null)
        {
            Dictionary<int, List<int>> dic = new Dictionary<int, List<int>>();
            batteryVoltage = 0;
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        List<int> lst = item.Value.BMSGetKState_EU_DC(out batteryVoltage);
                        dic.Add(id[0], lst);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }

            return dic;
        }
        public override Dictionary<int, List<double>> BMSGetParameter_EU_DC(List<int> lstIDs, byte tComm, string[] classNames = null)
        {
            Dictionary<int, List<double>> dic = new Dictionary<int, List<double>>();
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        List<double> lst = item.Value.BMSGetParameter_EU_DC(tComm);
                        dic.Add(id[0], lst);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }

            return dic;
        }
        public override void BMS_SetKState(List<int> lstIDs, List<bool> bs, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.BMS_SetKState(bs);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        public override void BMSClearError(List<int> lstIDs, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.BMSClearError();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        public override void BMSClearEnergy(List<int> lstIDs, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.BMSClearEnergy();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }



        public override void BMSSetConstAndInspectionError(List<int> lstIDs, double ElecConstant, double InspecError, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.BMSSetConstAndInspectionError(ElecConstant, InspecError);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        public override double BMSGetEnergy(List<int> lstIDs, EmChargerType chargerType = EmChargerType.Charger_GB_DC)
        {
            double result = 0;
            try
            {
                BMSControlInit();
                //国欧标都有1号枪会覆盖上一个数据
                string className = "";
                switch (chargerType)
                {
                    case EmChargerType.Charger_GB_DC:
                        className = "emtBMS_GB_DC";
                        break;
                    case EmChargerType.Charger_EUR_DC:
                        className = "emtBMS_EU_DC";
                        break;
                    case EmChargerType.Charger_GB_AC:
                    case EmChargerType.Charger_EUR_AC:
                    case EmChargerType.Charger_USA_AC:
                        className = "emtBMS_AC";
                        break;
                }
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(className))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        result = item.Value.BMSGetEnergy();

                        //SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + 
                        //    string.Format("{1}号枪：读取标准电能为{0}", result.ToString(), id[0]));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return result;
        }
        public override Dictionary<int, double[]> BMSGetError(List<int> lstIDs, string ElectricConstant16, string InspectionError16, string[] classNames = null)
        {
            Dictionary<int, double[]> dic = new Dictionary<int, double[]>();
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        double[] data = item.Value.BMSGetError(ElectricConstant16, InspectionError16);
                        if (data != null)
                            dic.Add(id[0], data);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return dic;
        }

        public override void BMSSetHCAC(List<int> lstIDs, EmChargerType type, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMSSetHCAC(type);

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        /// <summary>
        /// 直流BMS设置参数（充电阶段）
        /// </summary>
        /// <param name="bmsVolt">BMS需求电压设置(V)</param>
        /// <param name="bmsCurrent">bmsCurrent BMS需求电流设置(A)</param>
        /// <param name="type">true - 恒压  false - 恒流</param>
        /// <param name="ReadyVolt">当前电压</param>
        public override void SetParameter(List<int> lstIDs, Double bmsVolt, Double bmsCurrent, bool type, double ReadyVolt, bool canCharge = true, string[] classNames = null, InsulationState insulationState = InsulationState.正常)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.BMSSetParameter(bmsVolt, bmsCurrent, type, ReadyVolt, canCharge, insulationState);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        /// <summary>
        /// 直流BMS设置参数（握手阶段）
        /// </summary>
        /// <param name="bmsVolt">BMS需求电压设置(V)</param>
        /// /// <param name="BatteryTotalVolt">电池总电压(V)</param>
        public override void SetParameter(List<int> lstIDs, Double bmsVolt, double BatteryTotalVolt = 410, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                //富仁高科需要修改电池总电压
                string strBattery = ConfigurationManager.AppSettings["BatteryTotalVolt"];
                if(strBattery != null && double.TryParse(strBattery, out double battery))
                {
                    BatteryTotalVolt = battery;
                }
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {

                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMSSetParameter(bmsVolt, BatteryTotalVolt);

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        /// <summary>
        /// 直流BMS设置参数
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="bmsVolt">动力蓄电池当前电池电压(V)（欧标不使用电池当前电压）</param>
        /// <param name="maxVolt">最高允许充电电压</param>
        /// <param name="maxCurrent">最高允许充电电流</param>
        public override void SetParameter(List<int> lstIDs, Double bmsVolt, Double maxVolt, double maxCurrent, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {

                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        if (classNames.Contains("emtBMS_GB_DC"))
                        {
                            item.Value.BMSSetParameter(bmsVolt, maxVolt, maxCurrent);
                        }
                        else
                        {
                            var param1 = item.Value.BMSGetParameter_EU_DC(0x97);
                            List<int> ks = new List<int>();
                            for (int i = 0; i < param1.Count - 3; i++)
                            {
                                ks.Add(Convert.ToInt32(param1[i]));
                            }
                            item.Value.BMSSetPara1_EU_DC(ks, bmsVolt, maxCurrent, maxVolt);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// 欧标充电数据设置1
        /// </summary>
        /// <param name="lstIDs">枪信息</param>
        /// <param name="RESS_SoC">RESS SoC值</param>
        /// <param name="MaxCurrent">BMS最大电流值</param>
        /// <param name="MaxVoltage">BMS最大电压值</param>
        public override void BMSSetPara1_EU_DC(List<int> lstIDs, List<int> para1, double RESS_SoC, double MaxCurrent, double MaxVoltage, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {

                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMSSetPara1_EU_DC(para1, RESS_SoC, MaxCurrent, MaxVoltage);

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        /// <summary>
        /// 欧标充电数据设置1
        /// </summary>
        /// <param name="lstIDs">枪信息</param>
        /// <param name="RESS_SoC">RESS SoC值</param>
        /// <param name="MaxCurrent">BMS最大电流值</param>
        /// <param name="MaxVoltage">BMS最大电压值</param>
        public override void BMSSetPara1_EU_DC(List<int> lstIDs, double RESS_SoC, double MaxCurrent, double MaxVoltage, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {

                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMSSetPara1_EU_DC(RESS_SoC, MaxCurrent, MaxVoltage);

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        /// <summary>
        /// 欧标充电数据设置2
        /// </summary>
        /// <param name="lstIDs">枪信息</param>
        /// <param name="FullSOC">Full SoC值</param>
        /// <param name="BulkSOC">Bulk SoC值</param>
        /// <param name="TargetVolt">EV目标需求电压</param>
        /// <param name="TargetCurrent">EV目标需求电流</param>
        /// <param name="ReadyVolt">预充充电电压</param>
        public override void BMSSetPara2_EU_DC(List<int> lstIDs, double FullSOC, double BulkSOC, double TargetVolt, double TargetCurrent, double ReadyVolt, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {

                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMSSetPara2_EU_DC(FullSOC, BulkSOC, TargetVolt, TargetCurrent, ReadyVolt);

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        /// <summary>
        /// 欧标充电数据设置3
        /// </summary>
        /// <param name="lstIDs">枪信息</param>
        /// <param name="FullSOCRemainTime">剩余时间到满SoC</param>
        /// <param name="BulkSOCRemainTime">剩余时间到Bulk SoC</param>
        public override void BMSSetPara3_EU_DC(List<int> lstIDs, double FullSOCRemainTime, double BulkSOCRemainTime, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {

                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMSSetPara3_EU_DC(FullSOCRemainTime, BulkSOCRemainTime);

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }


        /// <summary>
        /// BMS设置互操电阻值
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="resistance">（桩模拟器R3/车模拟器R4）</param>
        /// <returns></returns>
        public override void BMSSetResistance(List<int> lstIDs, double resistance, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMSSetResistance(resistance);

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// BMS设置互操电池电压
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="resistance">电池电压</param>
        /// <returns></returns>
        public override void BMSSetBatteryVoltage(List<int> lstIDs, double batteryVoltage, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMSSetBatteryVoltage(batteryVoltage);

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// 直流BMS双枪并充开关读取（单枪）
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="isON">开关状态</param>
        /// <returns></returns>
        public override void BMSReadCombine_DC(List<int> lstIDs, out bool isON, string[] classNames = null)
        {
            isON = false;
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        isON = false;
                        item.Value.BMSReadCombine_DC(out isON);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// 直流BMS双枪并充开关设置
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="isON">开关状态</param>
        /// <returns></returns>
        public override void BMSSetCombine_DC(List<int> lstIDs, bool isON, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMSSetCombine_DC(isON);

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// BMS设置互操开关（电池电压设置无效待排查原因）
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="resistance">电池电压</param>
        /// <returns></returns>
        public override void BMSSetKState_DC(List<int> lstIDs, double resistance, double batteryVoltage, bool[] bs, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                //开关S也是CC1进行控制，如果两个数据位不一样会导致设置无效
                bs[27] = bs[22];
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.BMSSetKState_DC(resistance, batteryVoltage, bs);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// BMS设置互操控制开关
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="resistance">电池电压</param>
        /// <returns></returns>
        public override void BMSSetKState_DC(List<int> lstIDs, byte tComm, byte[] bs, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMSSetKState_DC(tComm, bs);

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// 直流BMS读取DC+/DC-对PE漏电电阻值
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="DCUpON">DC+对PE的漏电开关 0=断开；1=接通</param>
        /// <param name="DCDownON">DC-对PE的漏电开关 0=断开；1=接通</param>
        /// <param name="DCUpResistance">DC+对PE的漏电电阻值</param>
        /// <param name="DCDownResistance">DC-对PE的漏电电阻值</param>
        public override void BMSReadLeakageResistance_DC(List<int> lstIDs, out int DCUpON, out int DCDownON, out double DCUpResistance, out double DCDownResistance, string[] classNames = null)
        {
            DCUpON = 0; DCDownON = 0; DCUpResistance = 0; DCDownResistance = 0;
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.BMSReadLeakageResistance_DC(out DCUpON, out DCDownON, out DCUpResistance, out DCDownResistance);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// 直流BMS设置DC+/DC-对PE漏电电阻值
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="DCUpON">DC+对PE的漏电开关 0=断开；1=接通</param>
        /// <param name="DCDownON">DC-对PE的漏电开关 0=断开；1=接通</param>
        /// <param name="DCUpResistance">DC+对PE的漏电电阻值</param>
        /// <param name="DCDownResistance">DC-对PE的漏电电阻值</param>
        public override void BMSSetLeakageResistance_DC(List<int> lstIDs, int DCUpON, int DCDownON, double DCUpResistance, double DCDownResistance, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.BMSSetLeakageResistance_DC(DCUpON, DCDownON, DCUpResistance, DCDownResistance);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// BMS设置互操电池电压
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="batteryVoltage">电池电压</param>
        /// <param name="bs">数据内容第123字节的二进制开关（24位）</param>
        /// <param name="DCPlus">DC+绝缘阻值档位</param>
        /// <param name="DCMinus">DC-绝缘阻值档位</param>
        /// <param name="reserved">数据内容第5字节预留</param>
        /// <returns></returns>
        public override void BMSSetKState_EU_DC(List<int> lstIDs, double batteryVoltage, bool[] bs, int DCPlus, int DCMinus, string reserved = "", string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMSSetKState_EU_DC(batteryVoltage, bs, DCPlus, DCMinus, reserved);

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// 赛特BMS直流通断控制
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="tComm">组合指令</param>
        /// <param name="tisTrue"></param>
        public override void BMS_DC_SetControl(List<int> lstIDs, byte tComm, bool tisTrue, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMS_DC_SetControl(tComm, tisTrue);

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        /// <summary>
        /// BMS 协议一致性测试设置
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="byte0"></param>
        /// <param name="byte1"></param>
        /// <param name="byte2"></param>
        /// <param name="byte3"></param>
        /// <param name="byte4"></param>
        /// <param name="byte5"></param>
        /// <param name="byte6"></param>
        /// <param name="byte7"></param>
        public override void BMSProtocolConsistency(List<int> lstIDs, byte byte0, byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMSProtocolConsistency(byte0, byte1, byte2, byte3, byte4, byte5, byte6, byte7);

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        public override void SetProtocolTime(List<int> lstIDs, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.SetProtocolTime();

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        public override Dictionary<int, CanMsgRich> RevCANPacket(List<int> lstIDs, string[] classNames = null)
        {
            Dictionary<int, CanMsgRich> dicPacket = new Dictionary<int, CanMsgRich>();
            GETCANDATA gETCANDATA = new GETCANDATA();


            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        byte[] data = item.Value.RevCANPacket();
                        if (data == null)
                        {
                            return dicPacket;
                        }
                        else
                        {
                            CanMsgRich canMsgRiche = gETCANDATA.DecodePackage2(data.ToList(), eSGBDC_Ver);
                            if (canMsgRiche != null)
                            {
                                dicPacket.Add(id[0], canMsgRiche);
                            }
                        }
                        
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            return dicPacket;
        }
        public override void BMSGetVersion(List<int> lstIDs, out string SoftwareVersion, out string FlowNumber, string[] classNames = null)
        {
            SoftwareVersion = "";
            FlowNumber = "";
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        string softwareVersion = "", flowNumber = "";
                        item.Value.BMSGetVersion(out softwareVersion, out flowNumber);
                        if (!string.IsNullOrWhiteSpace(softwareVersion))
                            SoftwareVersion = softwareVersion;
                        if (!string.IsNullOrWhiteSpace(flowNumber))
                            FlowNumber = flowNumber;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        public override Dictionary<int, bool> GetIsCANStop(List<int> lstIDs)
        {
            try
            {
                BMSControlInit();
                Dictionary<int, bool> dicIsCANStop = new Dictionary<int, bool>();
                // 只有国标DC有CAN报文功能
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent("emtBMS_GB_DC"))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        dicIsCANStop.Add(id[0], item.Value.IsCANStop);
                    }
                }
                return dicIsCANStop;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return null;
            }
        }

        public override Dictionary<int, List<DataModel.CAN.CanMsgRich>> GetCANDATA(List<int> lstIDs)
        {
            try
            {
                BMSControlInit();
                Dictionary<int, List<DataModel.CAN.CanMsgRich>> dicCANDATA = new Dictionary<int, List<CanMsgRich>>();
                // 只有国标DC有CAN报文功能
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent("emtBMS_GB_DC"))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        dicCANDATA.Add(id[0], item.Value.CANDATA);
                    }
                }
                return dicCANDATA;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return null;
            }
        }
        /// <summary>
        /// 国标直流所有充电需求参数都重新设置2发送
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="settingCharging">充电所有的需求参数</param>
        public override void BMSDC_SetAllParameter(List<int> lstIDs, SettingCharging settingCharging, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {

                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMSSetALLParameter(settingCharging);

                    }
                }
            }

            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
        public override Dictionary<int, int> GetK3K4StopTime(List<int> lstIDs, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                Dictionary<int, int> dicK3K4StopTime = new Dictionary<int, int>();
                // 只有国标DC有这个需求
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        int time = -999;
                        item.Value.GetK3K4StopTime(out time);
                        dicK3K4StopTime.Add(id[0], time);
                    }
                }
                return dicK3K4StopTime;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return null;
            }
        }

        public override Dictionary<int, List<int>> BMSGetData_JP_DC(List<int> lstIDs, out int[] ErrorSign, out int[] StateSign, string[] classNames = null)
        {
            ErrorSign = new int[8];
            StateSign = new int[8];
            try
            {
                BMSControlInit();
                Dictionary<int, List<int>> dicData = new Dictionary<int, List<int>>();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        dicData.Add(id[0], item.Value.BMSGetData_JP_DC(out ErrorSign, out StateSign));
                    }
                }
                return dicData;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return null;
            }
        }
        
        public override void BMSSetData_JP_DC(List<int> lstIDs, string MinBatteryVolt, string MaxBatteryVolt, string ChargingRateConst, string MaxChargingTime_S, string MaxChargingTime_M,
            string ChargingET, string CHAdeMONumber, string TargetBatteryVolt, string ChargingCurrent, int[] ErrorSign, int[] StateSign, string ChargingRate, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {

                        item.Value.BMSSetData_JP_DC(MinBatteryVolt, MaxBatteryVolt, ChargingRateConst, MaxChargingTime_S, MaxChargingTime_M,
                            ChargingET, CHAdeMONumber, TargetBatteryVolt, ChargingCurrent, ErrorSign, StateSign, ChargingRate);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        public override void BMSSetParameter_JP_DC(List<int> lstIDs, double bmsVolt, double bmsCurrent, double maxVolt, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.BMSSetParameter_JP_DC(bmsVolt, bmsCurrent, maxVolt);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        public override Dictionary<int, List<bool>> BMSGetKState_JP_DC(List<int> lstIDs, out int DCPRState, out int DCMRState, out double BatteryVolt, string[] classNames = null)
        {
            DCPRState = 0; DCMRState = 0; BatteryVolt = 0;
            try
            {
                BMSControlInit();
                Dictionary<int, List<bool>> res = new Dictionary<int, List<bool>>();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        var lstKState = item.Value.BMSGetKState_JP_DC(out DCPRState, out DCMRState, out BatteryVolt);
                        res.Add(id[0], lstKState);
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return null;
            }
        }

        public override void BMSGetTempRH(List<int> lstIDs, out double Temp, out double RH, string[] classNames = null) 
        { 
            Temp = 0; RH = 0;
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.BMSGetTempRH(out Temp, out RH);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

        public override void BMSSendEUMsg(List<int> lstIDs, string EUMsg, string[] classNames = null)
        {
            try
            {
                BMSControlInit();
                classNames = classNames == null ? ClassNames : classNames;
                foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(classNames))
                {
                    int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                    if (id.Length > 0)
                    {
                        item.Value.SendEUMsg(EUMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }

    }
}
