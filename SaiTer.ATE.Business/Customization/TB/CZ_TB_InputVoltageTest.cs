using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public class CZ_TB_InputVoltageTest : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据

        int WaitTime = 10;//等待时间，单位秒
        double InputVoltage1 = 253;
        double InputVoltage2 = 187;
        double NormalVoltage = 220;
        double DemandVoltage = 350;
        double DemandCurrent = 10;
        double OutputPowerLower1 = 3000;
        double OutputPowerLower2 = 3000;
        double OutputPowerLower3 = 3000;
        double OutputPowerUpper1 = 4000;
        double OutputPowerUpper2 = 4000;
        double OutputPowerUpper3 = 4000;

        double VoltageRate = 1.3;//电压倍率  输入电压最高不能超过额定电压的倍数
        public CZ_TB_InputVoltageTest(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            //输入电压1(V)=253.00|输入电压(V)=187.00|额定输入电压(V)=220.00|输出电压(V)=370.00|输出电流(A)=10.00|输出功率下限1(W)=3000|输出功率下限1(W)=4000|输出功率下限2(W)=3000|输出功率下限2(W)=4000|输出功率下限3(W)=3000|输出功率下限3(W)=4000
            string[] strParams = TrialItem.ResultParams.Split('|');
            InputVoltage1 = Convert.ToDouble(strParams[0].Split('=')[1]);
            InputVoltage2 = Convert.ToDouble(strParams[1].Split('=')[1]);
            NormalVoltage = Convert.ToDouble(strParams[2].Split('=')[1]);
            DemandVoltage = Convert.ToDouble(strParams[3].Split('=')[1]);
            DemandCurrent = Convert.ToDouble(strParams[4].Split('=')[1]);
            OutputPowerLower1 = Convert.ToDouble(strParams[5].Split('=')[1]);
            OutputPowerUpper1 = Convert.ToDouble(strParams[6].Split('=')[1]);
            OutputPowerLower2 = Convert.ToDouble(strParams[7].Split('=')[1]);
            OutputPowerUpper2 = Convert.ToDouble(strParams[8].Split('=')[1]);
            OutputPowerLower3 = Convert.ToDouble(strParams[9].Split('=')[1]);
            OutputPowerUpper3 = Convert.ToDouble(strParams[10].Split('=')[1]);
        }
        public override void InitEquiMent()
        {
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
                SetACSource(lstIDs, NormalVoltage);

                //SetCPRersh_EUDC();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }

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
                                //界面展示的数据项格式
                                //测试时间|输入电压|是否停机|测试结果     
                                LstTrialData[i].ExtentData = DateTime.Now.ToString() + "|" + NormalVoltage + "|" + InputVoltage1 + "|未停机";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                //for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                //{
                //    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                //    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                //}

                //开始检测流程
                if (testWorkParam.lstIDs.Count > 0)
                {

                    SetACSource(testWorkParam.lstIDs, NormalVoltage);
                    ////闭合开关S2，启动充电
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    if (!CheckSwipingCard(testWorkParam.lstIDs, DemandVoltage + 10, DemandCurrent))
                    {
                        return;
                    }
                    Thread.Sleep(1000);
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage, DemandCurrent + 3, DemandVoltage, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrent_EU_DC(testWorkParam.lstIDs, DemandCurrent);
                    Thread.Sleep(3000);

                    //设置测试条件
                    SetConditionValues();

                    Thread.Sleep(5000); //等待输出电压稳定
                    d1 = new Dictionary<int, string>();
                    d2 = new Dictionary<int, string>();
                    d3 = new Dictionary<int, string>();
                    var dicPower = new Dictionary<int, string>();
                    foreach (int item in testWorkParam.lstIDs)
                    {
                        d1.Add(item, AllEquipStateData.DicACSource_StateData[LstChargerInfo[0].ChargerId].Volt.ToString("F2"));
                        d2.Add(item, AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSVolt.ToString("F2"));
                        d3.Add(item, AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent.ToString("F2"));
                        dicPower.Add(item, (Convert.ToDouble(d2[item]) * Convert.ToDouble(d3[item])).ToString("F2"));
                    }
                    ProcessDataTmp(d1, $"设定输入电压--{NormalVoltage}V", "输入电压(V)", "-", "-");
                    ProcessDataTmp(d2, $"设定输入电压--{NormalVoltage}V", "输出电压(V)", "60", "-");
                    ProcessDataTmp(d3, $"设定输入电压--{NormalVoltage}V", "输出电流(A)", "-", "-");
                    ProcessDataTmp(dicPower, $"设定输入电压--{NormalVoltage}V", "输出功率(W)", OutputPowerLower1.ToString("F2"), OutputPowerUpper1.ToString("F2"));

                    SetACSource(testWorkParam.lstIDs, InputVoltage1);
                    SendNoticeToUIAndTxtFile("已发送交流源调整值：" + InputVoltage1 + "V，等待交流源输出稳定。");
                    //Thread.Sleep(1000 * 16);

                    //采集数据
                    Data_Tmp = new Dictionary<int, string>();
                    d1 = new Dictionary<int, string>();
                    d3 = new Dictionary<int, string>();
                    dicPower = new Dictionary<int, string>();
                    int count = 0;
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double voltage = 0;
                        for (int j = 0; j < WaitTime; j++)
                        {
                            voltage = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSVolt;
                            if (voltage < 20)
                            {
                                count++;
                                if (count >= 3)
                                {
                                    break;
                                }
                            }
                            Thread.Sleep(1000);
                        }
                        Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                        double inputVolt = AllEquipStateData.DicACSource_StateData[testWorkParam.lstIDs[i]].Volt;
                        d1.Add(testWorkParam.lstIDs[i], inputVolt.ToString("F2"));
                        d3.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent.ToString("F2"));
                        dicPower.Add(testWorkParam.lstIDs[i], (voltage * Convert.ToDouble(d3[testWorkParam.lstIDs[i]])).ToString("F2"));
                    }
                    ProcessDataTmp(d1, $"设定输入电压--{InputVoltage1}V", "输入电压(V)", "-", "-");
                    ProcessDataTmp(Data_Tmp, $"设定输入电压--{InputVoltage1}V", "输出电压(V)", "60", "-");
                    ProcessDataTmp(d3, $"设定输入电压--{InputVoltage1}V", "输出电流(A)", "-", "-");
                    ProcessDataTmp(dicPower, $"设定输入电压--{InputVoltage1}V", "输出功率(W)", OutputPowerLower2.ToString("F2"), OutputPowerUpper2.ToString("F2"));


                    //第二次调整输入电压
                    SetACSource(testWorkParam.lstIDs, InputVoltage2);
                    SendNoticeToUIAndTxtFile("已发送交流源调整值：" + InputVoltage2 + "V，等待交流源输出稳定。");
                    //Thread.Sleep(1000 * 16);

                    //采集数据
                    Data_Tmp = new Dictionary<int, string>();
                    d1 = new Dictionary<int, string>();
                    d3 = new Dictionary<int, string>();
                    dicPower = new Dictionary<int, string>();
                    count = 0;
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double voltage = 0;
                        for (int j = 0; j < WaitTime; j++)
                        {
                            voltage = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSVolt;
                            if (voltage < 20)
                            {
                                count++;
                                if (count >= 3)
                                {
                                    break;
                                }
                            }
                            Thread.Sleep(1000);
                        }
                        Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                        double inputVolt = AllEquipStateData.DicACSource_StateData[testWorkParam.lstIDs[i]].Volt;
                        d1.Add(testWorkParam.lstIDs[i], inputVolt.ToString("F2"));
                        d3.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[i]].Channel4RMSCurrent.ToString("F2"));
                        dicPower.Add(testWorkParam.lstIDs[i], (voltage * Convert.ToDouble(d3[testWorkParam.lstIDs[i]])).ToString("F2"));
                    }
                    ProcessDataTmp(d1, $"设定输入电压--{InputVoltage2}V", "输入电压(V)", "-", "-");
                    ProcessDataTmp(Data_Tmp, $"设定输入电压--{InputVoltage2}V", "输出电压(V)", "60", "-");
                    ProcessDataTmp(d3, $"设定输入电压--{InputVoltage2}V", "输出电流(A)", "-", "-");
                    ProcessDataTmp(dicPower, $"设定输入电压--{InputVoltage2}V", "输出功率(W)", OutputPowerLower3.ToString("F2"), OutputPowerUpper3.ToString("F2"));

                    SetLoadDCOFF(testWorkParam.lstIDs);
                    //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("恢复额定电压：" + NormalVoltage + "V，等待交流源输出稳定。");
                    //ControlEquipMent.ACSource.ACSource_SetVolt(lstIDs, NormalVoltage);
                    SetACSource(testWorkParam.lstIDs, NormalVoltage);

                    Thread.Sleep(2000);
                }
            }
        }


        public override void ProcessData()
        {
            try
            {
                foreach (var item in testWorkParam.lstIDs)
                {
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                    LstTrialData[k].ItemName = iIndex.ToString();

                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    double volate = AllEquipStateData.DicBMS_AC_StateData[LstTrialData[k].ChargerId].PhaseA_Voltage;
                    string strResult = "未停机";
                    if (volate < 50)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        strResult = "已停机";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    //界面展示的数据项格式
                    //测试时间|BMS需求电压|输入电压|是否停机|测试结果     
                    LstTrialData[k].ExtentData = LstTrialData[k].SaveTime + "|" + NormalVoltage + "|" + InputVoltage1 + "|" + strResult;
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }
    }
}
