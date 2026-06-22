using SaiTer.ATE.DataModel;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    /// <summary>
    /// 电阻负载设备控制
    /// </summary>
    public class ResistanceLoadControl : ResistanceLoadBase
    {
        private string[] ClassNames = new string[] { "emtResistanceLoad_AC", "emtResistanceLoad_DC", "emtResistanceLoad_MultiChannel_DC", "emtResistanceLoad_MultiChannel_AC" };
        /// <summary>
        /// 启动负载输出
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void ResistanceLoad_ON(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        item.Value.ResisLoad_ON();
                    });
                }
            }

        }
        /// <summary>
        /// 关闭负载
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void ResistanceLoad_OFF(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    //ThreadPool.QueueUserWorkItem(state =>
                    //{
                    //    item.Value.ResisLoad_OFF();
                    //});
                    item.Value.ResisLoad_OFF();
                }
            }

        }

        /// <summary>
        /// 并机
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void ResistanceLoad_Parallel(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.ResisLoad_Parallel();
                }
                //item.Value.FeedbackLoad_Parallel();

            }

        }

        /// <summary>
        /// 取消并机
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void ResistanceLoad_NoParallel(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                //item.Value.FeedbackLoad_NoParallel();

                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.ResisLoad_NoParallel();
                }
            }

        }

        /// <summary>
        /// 设置负载需求电压电流
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="voltage">需求电压</param>
        public override void SetResisLoadVolCurr(List<int> lstIDs, double voltage, double current)
        {
            string x = ConfigurationManager.AppSettings["IsResisLoadConnect"];//三相电流是否并连到A相  0代表并连
            string curr_rate = ConfigurationManager.AppSettings["LoadRate"];//负载电流倍率

            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    double Current = current;
                    double? ResisLoadVolt = null;
                    if(AllEquipStateData.DicResisLoad_StateData != null && AllEquipStateData.DicResisLoad_StateData.Count > 0)
                    {
                        ResisLoadVolt = AllEquipStateData.DicResisLoad_StateData[id[0]].ActualVolt_A;
                    }
                    else if(AllEquipStateData.DicResisLoad_MultiChannel_AC_StateData != null && AllEquipStateData.DicResisLoad_MultiChannel_AC_StateData.Count > 0)
                    {
                        ResisLoadVolt = AllEquipStateData.DicResisLoad_MultiChannel_AC_StateData[id[0]].Voltage;
                    }
                    else if (AllEquipStateData.DicResisLoad_MultiChannel_DC_StateData != null && AllEquipStateData.DicResisLoad_MultiChannel_DC_StateData.Count > 0)
                    {
                        ResisLoadVolt = AllEquipStateData.DicResisLoad_MultiChannel_DC_StateData[id[0]].Voltage;
                    }
                    if (AllEquipStateData.DicBMS_AC_StateData != null && AllEquipStateData.DicBMS_AC_StateData.Count > 0)
                    {
                        if (AllEquipStateData.DicBMS_AC_StateData[id[0]].PhaseA_Voltage > 70
                              && AllEquipStateData.DicBMS_AC_StateData[id[0]].PhaseB_Voltage > 70
                              && AllEquipStateData.DicBMS_AC_StateData[id[0]].PhaseC_Voltage > 70)
                        {
                            if (x != null && x == "2")//没有并连开关
                            {
                                Current = current;
                            }
                            else if (x != null && x != "0")//未并连
                            {
                                Current = current * 3;
                            }
                            else
                            {
                                Current = current;
                            }
                        }
                        else
                        {
                            if (x != null && x != "0")//未并连
                            {
                                Current = current;
                            }
                            else

                            {
                                Current = current / 3;
                            }

                        }
                        Thread.Sleep(300);
                        if (ResisLoadVolt < 100)
                            ResisLoadVolt = voltage;
                        #region 可能以前负载没微调，电压小投出的电流也会同步小，接近设就得投大些，硬件确认有微调的话，上位机就不用管按需求投（120V美标桩需要转换）
                        //交流电阻载有微调，直流没有
                        //美标桩 电压低于220V，负载要按电压比例加大投载(同时判断导引和负载的电压都满足条件，防止单个设备瞬时采样错误)
                        if (/*ResisLoadVolt!= null && */ResisLoadVolt > 50 && ResisLoadVolt < 200)
                        {
                            if (AllEquipStateData.DicBMS_AC_StateData.Count >= id.Length)
                            {
                                double bmsVolt = AllEquipStateData.DicBMS_AC_StateData[id[0]].PhaseA_Voltage;
                                if (bmsVolt < 100)
                                    bmsVolt = voltage;
                                if (bmsVolt > 50 &&
                                    bmsVolt < 200)
                                {
                                    if (ChargerInfoManage.SelectChargerInfo(out lstChargerInfo))
                                    {
                                        Current *= 220 / voltage;
                                        //防止美标120V的桩触发微调的逻辑，与额定电压有差异（未验证，先按旧的逻辑）
                                        //Current *= 220 / lstChargerInfo.First().NominalVoltage;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    //ThreadPool.QueueUserWorkItem(state =>
                    //{
                    //    item.Value.SetResisLoadVoltCurr(voltage, Current);
                    //});
                    if (string.IsNullOrEmpty(curr_rate))
                        item.Value.SetResisLoadVoltCurr(voltage, Current);
                    else
                        item.Value.SetResisLoadVoltCurr(voltage, Current, Convert.ToInt32(curr_rate));
                }
            }

        }

        ///// <summary>
        ///// 设置负载需求电流
        ///// </summary>
        ///// <param name="lstIDs">枪编号集合</param>
        ///// <param name="voltage">需求电流</param>
        //public override void SetResisLoadCurrent(List<int> lstIDs, double current)
        //{
        //    foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent("emtResistanceLoad"))
        //    {
        //        int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
        //        if (id.Length > 0)
        //        {
        //            item.Value.SetResisLoadCurr(current);
        //        }
        //    }
        //}

        /// <summary>
        /// 设置负载需求功率
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="voltage">需求电压</param>
        /// <param name="power">需求功率</param>
        public override void SetResisLoadPower(List<int> lstIDs, double voltage, double power)
        {
            string x = ConfigurationManager.AppSettings["IsResisLoadConnect"];//三相电流是否并连到A相  0代表并连
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    double Power = 0;
                    if (AllEquipStateData.DicBMS_AC_StateData != null && AllEquipStateData.DicBMS_AC_StateData.Count > 0)
                    {
                        if (AllEquipStateData.DicBMS_AC_StateData[id[0]].PhaseA_Voltage > 70
                              && AllEquipStateData.DicBMS_AC_StateData[id[0]].PhaseB_Voltage > 70
                              && AllEquipStateData.DicBMS_AC_StateData[id[0]].PhaseC_Voltage > 70)
                        {
                            if (x != null && x != "0")//未并连
                            {
                                Power = power * 3;
                            }
                            else
                            {
                                Power = power;
                            }

                        }
                        else
                        {
                            if (x != null && x != "0")//未并连
                            {
                                Power = power;
                            }
                            else
                            {
                                Power = power / 3;
                            }

                        }
                    }
                    ThreadPool.QueueUserWorkItem(state => { item.Value.SetResisLoadPower(voltage, Power); });
                }
            }
        }
    }
}
