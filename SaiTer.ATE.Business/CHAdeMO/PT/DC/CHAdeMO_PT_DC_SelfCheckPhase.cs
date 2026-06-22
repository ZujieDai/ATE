using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 自检阶段测试
    /// </summary>
    public class CHAdeMO_PT_DC_SelfCheckPhase : BusinessBase
    {
        public CHAdeMO_PT_DC_SelfCheckPhase(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;
        private double BMSMeasureVoltage = 390;//过压参考值
        private int trlTimeOut_S = 5;
        Double CPVoltageMax = 10;
        Double CPVoltageMin = 8;

        double BMSDemandVoltage = 0;
        double BMSDemandCurrent = 0;
        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVoltage = LstChargerInfo[0].NominalVoltage;
            BMSDemandCurrent = 20;

            //if (strParams.Length >= 3)
            //{
            //    TestTime = Convert.ToInt32(Convert.ToDouble(strParams[0].Split('=')[1]));
            //    CPVoltageMax = Convert.ToDouble(strParams[1].Split('=')[1]);
            //    CPVoltageMin = Convert.ToDouble(strParams[2].Split('=')[1]);
            //}
        }

        public override void ExecuteMethod()
        {
            try
            {
                InitializeParams();
                InitEquiMent();
                StartItemFlow();

            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
                //停止过压输出
                ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x8C, false, new string[] { "emtBMS_JP_DC" });
                Thread.Sleep(100);
            }
        }

        private void StartItemFlow()
        {
            try
            {
                SendNoticeToUIAndTxtFile("开始" + TrialItem.ItemName + "--------------------------->");
                _StopWatch.Reset();
                _StopWatch.Start();
                while (true)
                {
                    testWorkParam.lstIDs.Clear();
                    for (int i = 0; i < LstTrialData.Count; i++)
                    {
                        if (LstTrialData[i].IsCheck)
                        {
                            if (LstTrialData[i].TrialResult == EmTrialResult.Wait)
                            {
                                if (!testWorkParam.lstIDs.Contains(LstTrialData[i].ChargerId))
                                {
                                    testWorkParam.lstIDs.Add(LstTrialData[i].ChargerId);
                                }
                            }
                        }
                    }
                    //是否全部有结论
                    if (testWorkParam.lstIDs.Count <= 0) break;
                    //是否超时
                    if (_StopWatch.ElapsedMilliseconds / 1000 > trlTimeOut_S)
                    {
                        for (int i = 0; i < LstTrialData.Count; i++)
                        {
                            if (LstTrialData[i].IsCheck)
                            {
                                if (LstTrialData[i].TrialResult == EmTrialResult.Wait)
                                {
                                    LstTrialData[i].TrialResult = EmTrialResult.Fail;
                                    LstTrialData[i].TrialValue = ((int)(_StopWatch.ElapsedMilliseconds / 1000)).ToString();
                                    int k = LstChargerInfo.FindIndex(s => s.ChargerId == LstTrialData[i].ChargerId);
                                    LstTrialData[i].PKID = LstChargerInfo[k].PKID;
                                    //界面展示的数据项格式
                                    //
                                    LstTrialData[i].ExtentData = "-|-|-|-|null";
                                    SendTrialDataToUI(LstTrialData[i]);
                                }
                            }
                        }
                        break;
                    }
                    if (testWorkParam.lstIDs.Count <= 0)
                    {
                        return;
                    }
                    SetConditionValues();

                    #region 外侧电压小于10V

                    SendNoticeToUIAndTxtFile("关闭BMS");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    SetCPRersh_JPDC();

                    SendNoticeToUIAndTxtFile("设置BMS参数,并启动充电");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(100);

                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 1000);
                    Thread.Sleep(100);

                    ////模拟外侧电压（小于10V目前暂不模拟电压）
                    //ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 5);
                    //Thread.Sleep(100);
                    //ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x8C, true, new string[] { "emtBMS_JP_DC" });
                    //Thread.Sleep(100);
                    //Thread.Sleep(5000);//等待数据刷新


                    d1 = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        //外侧电压具体是模拟过压后的导引充电电压
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_JP_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "外侧电压小于10V", "绝缘检测前外侧电压(V)", "-", "-");

                    SendNoticeToUIAndTxtFile("等待刷卡");
                    int timeout = 100;
                    MessgaeInfo(true, "请刷卡充电!", true);
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_JP_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus_JP_DC(c.Value.SystemState) >= 7);
                        if (ALLCanCharge)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "请刷卡充电!");

                    System.Threading.Thread.Sleep(5000);//等待电压稳定

                    ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x8C, false, new string[] { "emtBMS_JP_DC" });
                    Thread.Sleep(100);
                    System.Threading.Thread.Sleep(5000);//等待数据刷新

                    d1 = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_JP_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "外侧电压小于10V", "输出电压(V)", (BMSDemandVoltage * 0.9).ToString(), "-");



                    #endregion

                    #region 外侧电压大于10V

                    SendNoticeToUIAndTxtFile("关闭BMS");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    SetCPRersh_JPDC();

                    SendNoticeToUIAndTxtFile("设置BMS参数,并启动充电");
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(100);

                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.BMSSetResistance(testWorkParam.lstIDs, 1000);
                    Thread.Sleep(100);

                    //模拟外侧电压
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(testWorkParam.lstIDs, 200);//先固定30V
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x8C, true, new string[] { "emtBMS_JP_DC" });
                    Thread.Sleep(100);
                    Thread.Sleep(5000);//等待数据刷新

                    d1 = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        //外侧电压具体是模拟过压后的导引充电电压
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_JP_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "外侧电压大于10V", "绝缘检测前外侧电压(V)", "-", "-");

                    SendNoticeToUIAndTxtFile("等待刷卡");
                    timeout = 100;
                    MessgaeInfo(true, "请刷卡充电!", true);
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_JP_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus_JP_DC(c.Value.SystemState) >= 7);
                        if (ALLCanCharge)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "请刷卡充电!");

                    System.Threading.Thread.Sleep(5000);//等待电压稳定

                    ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x8C, false, new string[] { "emtBMS_JP_DC" });
                    Thread.Sleep(100);
                    System.Threading.Thread.Sleep(5000);//等待数据刷新

                    d1 = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_JP_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "外侧电压大于10V", "输出电压(V)", "0", "20");


                    #endregion

                }
            }
            catch (Exception ex) { SendException(ex); }
        }

        public override void ProcessData()
        {

        }
    }
}
