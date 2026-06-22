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
    /// 充放电检测（南通GX储能测试项目）
    /// </summary>

    public class CZ_NTGX_CN_ChargingAndDischarging : BusinessBase
    {
        public CZ_NTGX_CN_ChargingAndDischarging(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;//测试时间
        double BMSDemandVolt = 220;//额定电压
        double BMSDemandCurrent = 0;//额定电流
        double ExceedBattery = 390;//超过的电压值
        double Power_kW = 10;
        double RunTime_s = 3 * 60;
        int trlTimeOut_S = 0;


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

                    #region 放电功能
                    //插枪完毕
                    Dictionary<int, string> dic = new Dictionary<int, string>();

                    SendNoticeToUIAndTxtFile("设备正在启动放电中，请稍候...");
                    Thread.Sleep(1000);

                    CountDownTimeInfo("请设定交流放电功率【" + Power_kW + " kW】", 30, 0);

                    //放电检测
                    if (!CheckDisCharge_NTGX_CN(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    Thread.Sleep(5000);



                    //启动负载
                    BMSDemandCurrent = Math.Round(Power_kW * 1000 / BMSDemandVolt / 3, 2);
                    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, BMSDemandVolt, BMSDemandCurrent);
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("等待负载启动...");
                    Thread.Sleep(5000);


                    CountDownTimeInfo("等待带载稳定", 20, 0);


                    //放电状态运行时间
                    DateTime dts = DateTime.Now;
                    while (dts.AddSeconds(RunTime_s) > DateTime.Now)//目前固定3分钟
                    {
                        Thread.Sleep(1000);
                    }


                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel1RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "放电状态", "放电电压(V)", (BMSDemandVolt - 30).ToString(), (BMSDemandVolt + 30).ToString());

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel1RMSCurrent;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "放电状态", "放电电流(A)", (BMSDemandCurrent - 5).ToString(), (BMSDemandCurrent + 5).ToString());


                    //停止负载
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);

                    //停止交流源
                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(3000);

                    //停止放电
                    CountDownTimeInfo("请停止放电动作，操作完成后点击确定", 30, 0);
                    Thread.Sleep(2000);


                    #endregion


                    #region 充电功能

                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");

                    CountDownTimeInfo("请把枪插入充电桩的充电口，操作完成后点击确定", 90, 0);

                    //充电检测
                    if (!CheckCharging_NTGX_CN(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    Thread.Sleep(5000);

                    CountDownTimeInfo("等待充电运行稳定", 20, 0);


                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        //double? DCVoltage = AllEquipStateData.DicChargerNTGXCtrl_StateData.FirstOrDefault().Value?.ChargingVoltage;//电流无法读出，这里采样功率分析仪
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel5RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "充电状态", "充电电压(V)", "200", "-");
                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        //double? DCVoltage = AllEquipStateData.DicChargerNTGXCtrl_StateData.FirstOrDefault().Value?.ChargingCurrent;//电流无法读出，这里采样功率分析仪
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel5RMSCurrent;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "充电状态", "充电电流(A)", "-", "-");

                    //停止充电
                    SendNoticeToUIAndTxtFile("设备正在停止充电，请稍候...");
                    ControlEquipMent.ChargerCtrl.ChargerStop(testWorkParam.lstIDs);
                    Thread.Sleep(5000);

                    #endregion

                    ////直流放电检测（充电桩的正常充电）
                    //SetCPRersh_EUDC();
                    //SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");
                    //if (!CheckSwipingCard(testWorkParam.lstIDs))
                    //{
                    //    return;
                    //}
                    //Thread.Sleep(5000);
                    ////启动负载
                    //ControlEquipMent.ChargerCtrl.SetLoadParam_Charger(testWorkParam.lstIDs, BMSDemandVolt - 20, BMSDemandCurrent);
                    //ControlEquipMent.ChargerCtrl.LoadStart_Charger(testWorkParam.lstIDs);
                    //SendNoticeToUIAndTxtFile("等待负载启动...");
                    //Thread.Sleep(5000);
                    //dic = new Dictionary<int, string>();
                    //foreach (var item in testWorkParam.lstIDs)
                    //{
                    //    double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                    //    dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    //}
                    //ProcessDataTmp(dic, "放电状态", "放电电压(V)", GetErrLimit_I_CCS2_DC(BMSDemandCurrent)[0].ToString(), GetErrLimit_I_CCS2_DC(BMSDemandCurrent)[1].ToString());

                    //foreach (var item in testWorkParam.lstIDs)
                    //{
                    //    double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSCurrent;
                    //    dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    //}
                    //ProcessDataTmp(dic, "放电状态", "放电电流(A)", GetErrLimit_I_CCS2_DC(BMSDemandCurrent)[0].ToString(), GetErrLimit_I_CCS2_DC(BMSDemandCurrent)[1].ToString());

                    ////停止负载
                    //ControlEquipMent.ChargerCtrl.LoadStop_Charger(testWorkParam.lstIDs);
                    ////停止放电
                    //MessgaeInfo(true, "请停止放电动作，操作完成后点击确定", true);
                    //Thread.Sleep(2000);
                }
            }
            catch (Exception ex)
            {
                //停止负载
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);

                SendException(ex);
            }


        }


        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
        }

        public override void InitializeParams()
        {
            Init();
            //放电电压(V)=220|放电功率(kW)=10|运行时间(s)=180
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = double.Parse(strParams[0].Split('=')[1]);
            //BMSDemandCurrent = double.Parse(strParams[1].Split('=')[1]);
            Power_kW = double.Parse(strParams[1].Split('=')[1]);
            RunTime_s = double.Parse(strParams[2].Split('=')[1]);


        }

        public override void ProcessData()
        {

        }


    }
}
