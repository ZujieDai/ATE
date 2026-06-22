using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输入电压异常检测（包含输入过压、输入欠压）
    /// </summary>
    public class InputVoltageError_TM : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据

        double InputVoltage = 220;
        double NormalVoltage = 220;
        double WaitTime = 20;

        double VoltageRate = 1.3;//电压倍率  输入电压最高不能超过额定电压的倍数
        public InputVoltageError_TM(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            InputVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
            NormalVoltage = Convert.ToDouble(strParams[1].Split('=')[1]);
            if (NormalVoltage > 220)
            {
                NormalVoltage = 220;
            }
            VoltageRate = Convert.ToDouble(TrialItem.ItemParams.Split('|')[0].Split('=')[1]);
            if (InputVoltage / NormalVoltage >= VoltageRate)
            {
                InputVoltage = NormalVoltage * VoltageRate;
            }
            if (strParams.Length > 2)
            {
                //等待断电时间(s)
                WaitTime = Convert.ToDouble(strParams[2].Split('=')[1]);
            }
        }
        public override void InitEquiMent()
        {
            //由于常德TM使用的稳压源无法做过压欠压，所以加了一个变频源进行控制
            string Customer = ConfigurationManager.AppSettings["Customer"];
            if (Customer != null && Customer.Equals("TM"))
            {
                SendNoticeToUIAndTxtFile("关闭所有交流源输出并切换变频源和控制继电器。");
                //防止交流源刚启动输出
                Thread.Sleep(1000);
                ControlEquipMent.ACSource.ACSource_OFF(new List<int>() { 1, 2 });
                Thread.Sleep(2000);
                List<bool> lstRelay = new List<bool>();
                for (int i = 0; i < 16; i++)
                {
                    lstRelay.Add(false);
                }
                lstRelay[15] = true;
                ControlEquipMent.ControlBoard.ControlResistanceSetRelay(new List<int> { 2 }, lstRelay);
                Thread.Sleep(300);
                SendNoticeToUIAndTxtFile("恢复交流源输出。");
                ControlEquipMent.ACSource.ACSource_ON(new List<int>() { 2 });
                Thread.Sleep(2000);
            }
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
                //由于常德TM使用的稳压源无法做过压欠压，所以加了一个变频源进行控制
                string Customer = ConfigurationManager.AppSettings["Customer"];
                if (Customer != null && Customer.Equals("TM"))
                {
                    SendNoticeToUIAndTxtFile("关闭所有交流源输出并切换变频源和控制继电器。");
                    ControlEquipMent.ACSource.ACSource_OFF(new List<int>() { 1, 2 });
                    Thread.Sleep(2000);
                    List<bool> lstRelay = new List<bool>();
                    for (int i = 0; i < 16; i++)
                    {
                        lstRelay.Add(false);
                    }
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(new List<int> { 2 }, lstRelay);
                    Thread.Sleep(300);
                    SendNoticeToUIAndTxtFile("恢复交流源输出。");
                    ControlEquipMent.ACSource.ACSource_ON(new List<int>() { 1 });
                    Thread.Sleep(2000);
                }

                SetCPReresh();
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
                                LstTrialData[i].ExtentData = DateTime.Now.ToString() + "|" + NormalVoltage + "|" + InputVoltage + "|未停机";

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

                    ////闭合开关S2，启动充电
                    //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }



                    Thread.Sleep(2000);

                    ControlEquipMent.ACSource.ACSource_SetVolt(new List<int>() { 2 }, InputVoltage);
                    //ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("已发送交流源异常值：" + InputVoltage + "V，等待交流源输出稳定。");
                    //Thread.Sleep(1000 * 16);

                    //设置测试条件
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        int key = testWorkParam.lstIDs[i];
                        if (AllEquipStateData.DicACSource_StateData.Count == 1)
                        {
                            key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                        }
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                        d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);

                    //采集数据
                    Data_Tmp = new Dictionary<int, string>();
                    int count = 0;
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double voltage = 0;
                        if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_DC)
                        {
                            voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                            for (int j = 0; j < WaitTime; j++)
                            {
                                if (voltage > 20)
                                {
                                    voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;

                                    Thread.Sleep(1000);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        else if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_AC ||
                            LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC || LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC)
                        {
                            for (int j = 0; j < WaitTime; j++)
                            {
                                voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
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
                            //voltage = 0;
                        }
                        else if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_DC ||
                                LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_DC)
                        {
                            for (int j = 0; j < WaitTime; j++)
                            {
                                voltage = AllEquipStateData.DicBMS_EU_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
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
                            //voltage = 0;
                        }
                        Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                    }
                    ProcessDataTmp(Data_Tmp, "输入电压异常", "保护后桩输出电压(V)", "0", "30");

                    SendNoticeToUIAndTxtFile("恢复正常电压：" + NormalVoltage + "V，等待交流源输出稳定。");
                    ControlEquipMent.ACSource.ACSource_SetVolt(new List<int>() { 2 }, NormalVoltage);
                    //ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);

                    //这个是有恢复测试的
                    if (TrialType == (int)EmTrialType.输入过压保护及恢复测试
                        || TrialType == (int)EmTrialType.输入欠压保护及恢复测试)
                    {
                        Thread.Sleep(5000);

                        Data_Tmp = new Dictionary<int, string>();
                        for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                        {
                            double voltage = 0;
                            if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_DC ||
                                    LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_DC ||
                                    LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_DC)
                            {
                                for (int j = 0; j < WaitTime; j++)
                                {
                                    voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                                    if (voltage < 80)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            else if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_AC ||
                                LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC ||
                                LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC)
                            {
                                for (int j = 0; j < WaitTime; j++)
                                {
                                    voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                                    if (voltage < 80)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                        }
                        ProcessDataTmp(Data_Tmp, "输入电压正常", "恢复后桩输出电压(V)", "80", "260");

                    }
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
                    LstTrialData[k].ExtentData = LstTrialData[k].SaveTime + "|" + NormalVoltage + "|" + InputVoltage + "|" + strResult;
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
