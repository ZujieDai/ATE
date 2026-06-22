using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 欧标交流研测：故障保护
    /// </summary>
    public class CCS2_RT_AC_FaultProtectionTest : BusinessBase
    {
        string itemFlow = "";
        //private int CheckTime = 20;//人工检测时间 秒
        double WaitTime = 20;
        double OutputCurrent = 40;
        double DemandCurrent = 30;
        double NormalVoltage = 220;

        public CCS2_RT_AC_FaultProtectionTest(int type)
        {
            TrialType = type;
        }

        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            string[] strParams = TrialItem.ResultParams.Split('|');
            OutputCurrent = Convert.ToDouble(strParams[0].Split('=')[1]);
            DemandCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            NormalVoltage = Convert.ToDouble(strParams[2].Split('=')[1]);
        }
        public override void ExecuteMethod()
        {
            try
            {
                Init();
                SetCPReresh();
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
                SetACSource(lstIDs, NormalVoltage);
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }


        /// <summary>
        /// 测试流程
        /// </summary>
        public void StartItemFlow()
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
                if (_StopWatch.ElapsedMilliseconds / 1000 > 10)
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

                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                //设置测试条件
                SetConditionValues();

                //itemFlow = "故障保护";
                //CountDownTimeInfo("请确定故障保护应包括根据IEC 60364-4-41 允许的一种或多种保护措施。(勾选为PASS)\r\n倒计时结束默认PASS", CheckTime, 2);
                //ProcessData();

                //开始检测流程
                if (testWorkParam.lstIDs.Count > 0)
                {
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    //设置测试条件
                    SetConditionValues();

                    Thread.Sleep(3000); //等待输出电压稳定
                    d1 = new Dictionary<int, string>();
                    d2 = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicACSource_StateData[LstChargerInfo[0].ChargerId].Volt.ToString("F2"));

                        d2.Add(item, AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Voltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "输入电压正常", "输入电压(V)", "-", "-");
                    ProcessDataTmp(d2, "输入电压正常", "输出电压(V)", "-", "-");

                    #region 输入欠压
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    Thread.Sleep(2000);

                    double InputVoltage = NormalVoltage * 0.9;
                    SetACSource(testWorkParam.lstIDs, InputVoltage);

                    SendNoticeToUIAndTxtFile("已发送交流源异常值：" + InputVoltage + "V，等待交流源输出稳定。");
                    //Thread.Sleep(1000 * 16);

                    //采集数据
                    var Data_Tmp = new Dictionary<int, string>();
                    d1 = new Dictionary<int, string>();
                    int count = 0;
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double voltage = 0;
                        for (int j = 0; j < WaitTime; j++)
                        {
                            voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                            if (voltage < 20)
                            {
                                count++;
                                if (count >= 3)
                                {
                                    SendNoticeToUIAndTxtFile("充电桩触发保护。");
                                    break;
                                }
                            }
                            Thread.Sleep(1000);
                        }
                        //voltage = 0;
                        Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                        double inputVolt = AllEquipStateData.DicACSource_StateData[testWorkParam.lstIDs[i]].Volt;
                        d1.Add(testWorkParam.lstIDs[i], inputVolt.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "输入欠压", "异常输入电压(V)", "-", "-");
                    ProcessDataTmp(Data_Tmp, "输入欠压", "保护后桩输出电压(V)", "0", "30");

                    Data_Tmp = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "输入欠压", "CP电压(V)", "-", "-");

                    SendNoticeToUIAndTxtFile("恢复正常电压：" + NormalVoltage + "V，等待交流源输出稳定。");
                    SetACSource(testWorkParam.lstIDs, NormalVoltage);
                    #endregion

                    SetCPReresh();

                    #region 输入过压
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    Thread.Sleep(2000);

                    InputVoltage = NormalVoltage * 1.1;
                    SetACSource(testWorkParam.lstIDs, InputVoltage);

                    SendNoticeToUIAndTxtFile("已发送交流源异常值：" + InputVoltage + "V，等待交流源输出稳定。");
                    //Thread.Sleep(1000 * 16);

                    //采集数据
                    Data_Tmp = new Dictionary<int, string>();
                    d1 = new Dictionary<int, string>();
                    count = 0;
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double voltage = 0;
                        for (int j = 0; j < WaitTime; j++)
                        {
                            voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                            if (voltage < 20)
                            {
                                count++;
                                if (count >= 3)
                                {
                                    SendNoticeToUIAndTxtFile("充电桩触发保护。");
                                    break;
                                }
                            }
                            Thread.Sleep(1000);
                        }
                        //voltage = 0;
                        Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                        double inputVolt = AllEquipStateData.DicACSource_StateData[testWorkParam.lstIDs[i]].Volt;
                        d1.Add(testWorkParam.lstIDs[i], inputVolt.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "输入过压", "异常输入电压(V)", "-", "-");
                    ProcessDataTmp(Data_Tmp, "输入过压", "保护后桩输出电压(V)", "0", "30");

                    Data_Tmp = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "输入过压", "CP电压(V)", "-", "-");

                    SendNoticeToUIAndTxtFile("恢复正常电压：" + NormalVoltage + "V，等待交流源输出稳定。");
                    SetACSource(testWorkParam.lstIDs, NormalVoltage);
                    #endregion

                    SetCPReresh();

                    #region 输出过流
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    Thread.Sleep(2000);

                    SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, DemandCurrent);
                    SendNoticeToUIAndTxtFile("已发送负载正常电流值：" + DemandCurrent + "A，等待负载稳定。");
                    Thread.Sleep(8000);

                    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(lstIDs, LstChargerInfo[0].NominalVoltage, OutputCurrent);
                    SendNoticeToUIAndTxtFile("已发送负载过流值：" + OutputCurrent + "A，等待负载稳定。");
                    Thread.Sleep(13000);

                    Data_Tmp = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString("F2"));
                    }
                    ProcessDataTmp(Data_Tmp, "输出过流", "保护后桩输出电压(V)", "0", "30");

                    Data_Tmp = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Current.ToString("F2"));
                    }
                    ProcessDataTmp(Data_Tmp, "输出过流", "保护后桩输出电流(A)", "0", "5");

                    Data_Tmp = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].CPVoltage.ToString());
                    }
                    ProcessDataTmp(Data_Tmp, "输出过流", "CP电压(V)", "-", "-");

                    SendNoticeToUIAndTxtFile("关闭负载。重启交流源恢复桩故障");

                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);

                    Thread.Sleep(3000);
                    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                    #endregion
                }
            }
        }



        public override void ProcessData()
        {
            foreach (var item in testWorkParam.lstIDs)
            {
                int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                if (k < 0)
                    return;
                LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;


                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                double volate = AllEquipStateData.DicBMS_AC_StateData[LstTrialData[k].ChargerId].PhaseA_Voltage;
                if (DicManualVerifyResult[item])
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     
                LstTrialData[k].ExtentData = $"{itemFlow}|-|-|-|-";
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                SaveTrialData(LstTrialData[k]);
            }
        }
    }
}
