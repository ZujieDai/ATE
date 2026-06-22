using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using SaiTer.ATE.DataModel.Protocol;
using SaiTer.ATE.EquipMent;

namespace SaiTer.ATE.Business
{
    public class InsulationResistance_SE : BusinessBase
    {
        string FlowItemName = "";
        float trlTimeOut_S = 8;//超时时间
        int waitTime = 100;
        string TestResult = "";

        string info = "";
        /// <summary>
        /// 小的检定点名称
        /// </summary>
        string TrialFlowName = "";
        int testType = 1;
        string chanelSet = "";

        public InsulationResistance_SE(int trialType)
        {
            TrialType = trialType;
        }
        /// <summary>
        /// 初始化当前检测项信息 
        /// </summary>
        public override void InitializeParams()
        {
            switch (TrialType)
            {
                case (int)EmTrialType.绝缘电阻_输入对输出:
                    FlowItemName = "绝缘电阻";
                    chanelSet = "HLOOOOOOOOOOOOOOOOOOOOOO";
                    testType = 1;
                    break;
                case (int)EmTrialType.绝缘电阻_输入对地:
                    FlowItemName = "绝缘电阻";
                    testType = 2;
                    chanelSet = "HOOLLOOOOOOOOOOOOOOOOOOO";
                    break;
                case (int)EmTrialType.绝缘电阻_输出对地:
                    FlowItemName = "绝缘电阻";
                    chanelSet = "OHOLLOOOOOOOOOOOOOOOOOOO";
                    testType = 3;
                    break;
                case (int)EmTrialType.交流耐压_输入对输出:
                    FlowItemName = "交流耐压";
                    testType = 4;
                    chanelSet = "HLOOOOOOOOOOOOOOOOOOOOOO";
                    break;
                case (int)EmTrialType.交流耐压_输入对地:
                    FlowItemName = "交流耐压";
                    testType = 5;
                    chanelSet = "HOOLLOOOOOOOOOOOOOOOOOOO";
                    break;
                case (int)EmTrialType.交流耐压_输出对地:
                    FlowItemName = "交流耐压";
                    testType = 6;
                    chanelSet = "OHOLLOOOOOOOOOOOOOOOOOOO";
                    break;
                case (int)EmTrialType.直流耐压_输入对输出:
                    FlowItemName = "直流耐压";
                    testType = 7;
                    chanelSet = "HLOOOOOOOOOOOOOOOOOOOOOO";
                    break;
                case (int)EmTrialType.直流耐压_输入对地:
                    FlowItemName = "直流耐压";
                    testType = 8;
                    chanelSet = "HOOLLOOOOOOOOOOOOOOOOOOO";
                    break;
                case (int)EmTrialType.直流耐压_输出对地:
                    FlowItemName = "直流耐压";
                    testType = 9;
                    chanelSet = "OHOLLOOOOOOOOOOOOOOOOOOO";
                    break;
                case (int)EmTrialType.接地试验1:
                    FlowItemName = "接地试验";
                    testType = 10;
                    chanelSet = "6";
                    break;
                case (int)EmTrialType.接地试验2:
                    FlowItemName = "接地试验";
                    testType = 11;
                    chanelSet = "7";
                    break;
                case (int)EmTrialType.接地试验3:
                    FlowItemName = "接地试验";
                    testType = 12;
                    chanelSet = "8";
                    break;
                case (int)EmTrialType.接地试验_工装:
                    FlowItemName = "接地试验";
                    testType = 13;
                    chanelSet = "5";
                    break;
                default:
                    return;
            }

            //ControlEquipMent.ACSource?.ACSource_OFF(lstIDs);
        }
        /// <summary>
        /// 设备初始化
        /// </summary>
        public override void InitEquiMent()
        {
            ((emtSafety_SE7441)ControlEquipMent.Safety.DitEquipMentBase.First().Value).isSafetyInit = true;
            Thread.Sleep(100);
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
                ((emtSafety_SE7441)ControlEquipMent.Safety.DitEquipMentBase.First().Value).isSafetyInit = false;
                //保存试验结果
                ControlEquipMent.Safety.SafetyOFF(lstIDs);
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
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
                                //IR电压值(KV)|HISET电阻值(MΩ)|LOSET电阻值(MΩ)| 测试时间(S)|斜坡时间(S)|IR参考值(MΩ)|测试结果
                                //LstTrialData[k].ExtentData = Result.LstData[0].ToString().TrimEnd('\0');
                                //LstTrialData[i].ExtentData = IRVolt + "|" + HISETResistance + "|" + LOSETResistance + "|" +
                                //    TestTime + "|" + RampTime + "|" + IRReferenceValue;

                                LstTrialData[i].ExtentData = TrialItem.ItemName + "|测试超时|-|-|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                if (testWorkParam.lstIDs.Count > 0)
                {
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, $"SS 0{testType}", "\n", "\n", 0);
                    Thread.Sleep(waitTime);
                    //if (testType != 4)
                    //    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, $"ES {chanelSet}", "\n", "\n", 0);
                    //else
                    //    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, $"ESN {chanelSet}", "\n", "\n", 0);
                    //Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "TEST", "\n", "\n", 0);
                    Thread.Sleep(waitTime);

                    //SetControlboard(0);
                    StarTestItem(testType);
                }
            }
        }

        private void StarTestItem(int testType)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //ControlEquipMent.Safety.SafetyOFF(testWorkParam.lstIDs);
            //Thread.Sleep(waitTime);

            //ControlEquipMent.Safety.SafetyReadResult(testWorkParam.lstIDs, "TEST", "\n", "\n");

            //string testResult = ControlEquipMent.Safety.SafetyReadResult(testWorkParam.lstIDs, $"RD {testType}?", "\n", "\n");
            //string testResult = ControlEquipMent.Safety.SafetyReadResult(testWorkParam.lstIDs, $"TD?", "\n", "\n");
            int t = 0;  //执行次数
            while (true)
            {
                try
                {
                    //间隔0.1S以上读的数据是完全一样的，则测试已停止
                    string result = ControlEquipMent.Safety.SafetyReadResult(testWorkParam.lstIDs, $"TD?", "\n", "\n");
                    if (TestResult.Equals(result))
                    {
                        Thread.Sleep(waitTime);
                        result = ControlEquipMent.Safety.SafetyReadResult(testWorkParam.lstIDs, $"TD?", "\n", "\n"); 
                        if (TestResult.Equals(result))
                        {
                            TestResult = ControlEquipMent.Safety.SafetyReadResult(testWorkParam.lstIDs, $"RD {testType}?", "\n", "\n");
                            Thread.Sleep(waitTime);
                            ControlEquipMent.Safety.SafetyOFF(testWorkParam.lstIDs);
                            Thread.Sleep(waitTime);
                            break;
                        }
                    }
                    //Thread.Sleep(50);   //通讯底层接收消息有固定延迟，超过100ms

                    TestResult = result;
                    //if (t % 10 == 0)
                    //    SystemEvent.SendLogMessage("已测试时间 " + t / 10 + "秒   \r\t  \r\t ");
                    t++;
                }
                catch (Exception ex)
                {
                    Log.Log.LogException(ex);
                }
            }
            //var time = sw.ElapsedMilliseconds;
            //sw.Stop();
            //Thread.Sleep(500);

            // Thread.Sleep((Convert.ToInt32(double.Parse(TestTime)) + 2) * 1000);
            //SendNoticeToUIAndTxtFile("正在读取安规数据");

            //TestResult = ControlEquipMent.Safety.SafetyReadResult(testWorkParam.lstIDs, $"RD {testType}?", "\n", "\n");
            //ui发送提示
            // CountDownTimeInfo("请断开充电桩输入线缆与测试设备的连接，确认与安规设备符合【输入对地】的接线", 30);

            //开始判断数据
            ProcessData();
        }


        public override void ProcessData()
        {
            try
            {
                foreach (int item in testWorkParam.lstIDs)
                {
                    string[] ReturnStrS = null;
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    string data = FlowItemName + "|测试结果|-|-|null";
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                    if (string.IsNullOrEmpty(TestResult))
                    {
                        //continue;
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        //testWorkParam.lstIDs.Remove(Result.ChargeId);
                    }

                    else
                    {
                        LstTrialData[k].Data1 = TestResult;
                        //Log.Log.LogMessage("[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "] "+ TrialItem.ItemName + ": " + Result.LstData[0].ToString(), "设备日志_安规仪");
                        ReturnStrS = TestResult.Split(',');
                        if (ReturnStrS.Length >= 5)
                        {
                            data = FlowItemName + "|测试结果|-|-|" + ReturnStrS[2];

                            LstTrialData[k].TrialValue = ReturnStrS[1];
                            //Log.Log.LogMessage("安规测试结果：" + Result.LstData[0]);
                            if (ReturnStrS[2].ToUpper().Contains("PASS"))
                            {
                                ////2652M ohm
                                //string str = ReturnStrS[3].Split('M')[0];
                                //if (Convert.ToDouble(str) > Convert.ToDouble(HISETResistance) ||
                                //    Convert.ToDouble(str) < Convert.ToDouble(LOSETResistance))
                                //{
                                //    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                                //}
                                //else
                                //{
                                LstTrialData[k].TrialResult = EmTrialResult.Pass;
                                //}
                            }
                            else
                            {
                                LstTrialData[k].TrialResult = EmTrialResult.Fail;
                                //testWorkParam.lstIDs.Remove(Result.ChargeId);
                            }
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            //testWorkParam.lstIDs.Remove(Result.ChargeId);
                        }

                    }
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    LstTrialData[k].ItemName = TrialFlowName;

                    //界面展示的数据项格式
                    //IR电压值(KV)|HISET电阻值(MΩ)|LOSET电阻值(MΩ)| 测试时间(S)|斜坡时间(S)|IR参考值(MΩ)|测试结果
                    //LstTrialData[k].ExtentData = Result.LstData[0].ToString().TrimEnd('\0');
                    //string data = IRVolt + "|" + HISETResistance + "|" + LOSETResistance + "|" +
                    //    TestTime + "|" + RampTime + "|" + IRReferenceValue;
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    LstTrialData[k].ItemName = FlowItemName;
                    LstTrialData[k].ExtentData = data;
                    LstTrialData[k].Data2 = data;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    //LstTrialData[k].TrialCondition = "供电电压V=" + AllEquipStateData.DicACSource_StateData[LstTrialData[k].ChargerId].Volt + "|" +
                    //  "供电电流A=" + AllEquipStateData.DicACSource_StateData[LstTrialData[k].ChargerId].Current + "|" +
                    //  "供电频率=" + AllEquipStateData.DicACSource_StateData[LstTrialData[k].ChargerId].Freq;
                    LstTrialData[k].TrialCondition = "供电电压V=0|供电电流A=0|供电频率=0";
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);

                    if (ReturnStrS != null && ReturnStrS.Length > 5)
                    {
                        //通过测试方案获取安规方案的信息
                        string schemeName = LstChargerInfo.FirstOrDefault()?.SchemeName;
                        List<StSchemeInfo> lstSchemeInfo = new List<StSchemeInfo>();
                        SchemeInfoManage.GetSchemeInfo(ref lstSchemeInfo);
                        int SchemeId = lstSchemeInfo.FirstOrDefault(s => s.SchemeName.Equals(schemeName)).SchemeID;
                        EquipmentConfigModel EquipmentConfig = EquipmentConfigManage.GetEquipConfigs()?.Find(s => s.EquipmentName.Equals("Safety_SE7441") && s.ConfigType.Equals("Safety_Params") && s.ChargerType == SchemeId);
                        if (EquipmentConfig != null)
                        {
                            string[] param_IR1 = EquipmentConfig.Params1.Split(';')[0].Split('|');
                            string[] param_IR2 = EquipmentConfig.Params1.Split(';')[1].Split('|');
                            string[] param_IR3 = EquipmentConfig.Params1.Split(';')[2].Split('|');
                            string[] param_ACW1 = EquipmentConfig.Params2.Split(';')[0].Split('|');
                            string[] param_ACW2 = EquipmentConfig.Params2.Split(';')[1].Split('|');
                            string[] param_ACW3 = EquipmentConfig.Params2.Split(';')[2].Split('|');
                            string[] param_DCW1 = EquipmentConfig.Params3.Split(';')[0].Split('|');
                            string[] param_DCW2 = EquipmentConfig.Params3.Split(';')[1].Split('|');
                            string[] param_DCW3 = EquipmentConfig.Params3.Split(';')[2].Split('|');
                            string[] param_GND1 = EquipmentConfig.Remark.Split(';')[0].Split('|');
                            string[] param_GND2 = EquipmentConfig.Remark.Split(';')[1].Split('|');
                            string[] param_GND3 = EquipmentConfig.Remark.Split(';')[2].Split('|');
                            string[] param_GND4 = EquipmentConfig.Remark.Split(';')[3].Split('|');
                            var dd = new Dictionary<int, string>();
                            string IR_HISET, IR_LOSET, ACW_HISET, ACW_LOSET, DCW_HISET, DCW_LOSET, GND_HISET, GND_LOSET;
                            switch (testType)
                            {
                                case 1:
                                    IR_HISET = param_IR1[1].Split('=')[1];
                                    if (Convert.ToInt32(IR_HISET) == 0)
                                        IR_HISET = "-";
                                    IR_LOSET = param_IR1[2].Split('=')[1];
                                    dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    ProcessDataTmp(dd, FlowItemName, "电阻值(MΩ)", IR_LOSET, IR_HISET);
                                    break;
                                case 2:
                                    IR_HISET = param_IR2[1].Split('=')[1];
                                    if (Convert.ToInt32(IR_HISET) == 0)
                                        IR_HISET = "-";
                                    IR_LOSET = param_IR2[2].Split('=')[1];
                                    dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    ProcessDataTmp(dd, FlowItemName, "电阻值(MΩ)", IR_LOSET, IR_HISET);
                                    break;
                                case 3:
                                    IR_HISET = param_IR3[1].Split('=')[1];
                                    if (Convert.ToInt32(IR_HISET) == 0)
                                        IR_HISET = "-";
                                    IR_LOSET = param_IR3[2].Split('=')[1];
                                    dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    ProcessDataTmp(dd, FlowItemName, "电阻值(MΩ)", IR_LOSET, IR_HISET);
                                    break;
                                case 4:
                                    ACW_HISET = param_ACW1[1].Split('=')[1];
                                    if (Convert.ToInt32(ACW_HISET) == 0)
                                        ACW_HISET = "-";
                                    ACW_LOSET = param_ACW1[2].Split('=')[1];
                                    ////总电流
                                    //dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    //ProcessDataTmp(dd, FlowItemName, "电流值T(mA)", ACW_LOSET, ACW_HISET);
                                    //dd = new Dictionary<int, string>();
                                    ////阻性电流
                                    //dd.Add(testWorkParam.lstIDs[0], ReturnStrS[5]);
                                    //ProcessDataTmp(dd, FlowItemName, "电流值R(mA)", ACW_LOSET, ACW_HISET);
                                    dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    ProcessDataTmp(dd, FlowItemName, "电流值(mA)", ACW_LOSET, ACW_HISET);
                                    break;
                                case 5:
                                    ACW_HISET = param_ACW2[1].Split('=')[1];
                                    if (Convert.ToInt32(ACW_HISET) == 0)
                                        ACW_HISET = "-";
                                    ACW_LOSET = param_ACW2[2].Split('=')[1];
                                    //dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    //ProcessDataTmp(dd, FlowItemName, "电流值T(mA)", ACW_LOSET, ACW_HISET);
                                    //dd = new Dictionary<int, string>();
                                    //dd.Add(testWorkParam.lstIDs[0], ReturnStrS[5]);
                                    //ProcessDataTmp(dd, FlowItemName, "电流值R(mA)", ACW_LOSET, ACW_HISET);
                                    dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    ProcessDataTmp(dd, FlowItemName, "电流值(mA)", ACW_LOSET, ACW_HISET);
                                    break;
                                case 6:
                                    ACW_HISET = param_ACW3[1].Split('=')[1];
                                    if (Convert.ToInt32(ACW_HISET) == 0)
                                        ACW_HISET = "-";
                                    ACW_LOSET = param_ACW3[2].Split('=')[1];
                                    //dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    //ProcessDataTmp(dd, FlowItemName, "电流值T(mA)", ACW_LOSET, ACW_HISET);
                                    //dd = new Dictionary<int, string>();
                                    //dd.Add(testWorkParam.lstIDs[0], ReturnStrS[5]);
                                    //ProcessDataTmp(dd, FlowItemName, "电流值R(mA)", ACW_LOSET, ACW_HISET);
                                    dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    ProcessDataTmp(dd, FlowItemName, "电流值(mA)", ACW_LOSET, ACW_HISET);
                                    break;
                                case 7:
                                    DCW_HISET = param_DCW1[1].Split('=')[1];
                                    if (Convert.ToInt32(DCW_HISET) == 0)
                                        DCW_HISET = "-";
                                    DCW_LOSET = param_DCW1[2].Split('=')[1];
                                    dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    ProcessDataTmp(dd, FlowItemName, "电流值(uA)", DCW_LOSET, DCW_HISET);
                                    break;
                                case 8:
                                    DCW_HISET = param_DCW2[1].Split('=')[1];
                                    if (Convert.ToInt32(DCW_HISET) == 0)
                                        DCW_HISET = "-";
                                    DCW_LOSET = param_DCW2[2].Split('=')[1];
                                    dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    ProcessDataTmp(dd, FlowItemName, "电流值(uA)", DCW_LOSET, DCW_HISET);
                                    break;
                                case 9:
                                    DCW_HISET = param_DCW3[1].Split('=')[1];
                                    if (Convert.ToInt32(DCW_HISET) == 0)
                                        DCW_HISET = "-";
                                    DCW_LOSET = param_DCW3[2].Split('=')[1];
                                    dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    ProcessDataTmp(dd, FlowItemName, "电流值(uA)", DCW_LOSET, DCW_HISET);
                                    break;
                                case 10:
                                    GND_HISET = param_GND1[2].Split('=')[1];
                                    if (Convert.ToInt32(GND_HISET) == 0)
                                        GND_HISET = "-";
                                    GND_LOSET = param_GND1[3].Split('=')[1];
                                    dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    ProcessDataTmp(dd, FlowItemName, "电阻值(mΩ)", GND_LOSET, GND_HISET);
                                    break;
                                case 11:
                                    GND_HISET = param_GND2[2].Split('=')[1];
                                    if (Convert.ToInt32(GND_HISET) == 0)
                                        GND_HISET = "-";
                                    GND_LOSET = param_GND2[3].Split('=')[1];
                                    dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    ProcessDataTmp(dd, FlowItemName, "电阻值(mΩ)", GND_LOSET, GND_HISET);
                                    break;
                                case 12:
                                    GND_HISET = param_GND3[2].Split('=')[1];
                                    if (Convert.ToInt32(GND_HISET) == 0)
                                        GND_HISET = "-";
                                    GND_LOSET = param_GND3[3].Split('=')[1];
                                    dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    ProcessDataTmp(dd, FlowItemName, "电阻值(mΩ)", GND_LOSET, GND_HISET);
                                    break;
                                case 13:
                                    GND_HISET = param_GND4[2].Split('=')[1];
                                    if (Convert.ToInt32(GND_HISET) == 0)
                                        GND_HISET = "-";
                                    GND_LOSET = param_GND4[3].Split('=')[1];
                                    dd.Add(testWorkParam.lstIDs[0], ReturnStrS[4]);
                                    ProcessDataTmp(dd, FlowItemName, "电阻值(mΩ)", GND_LOSET, GND_HISET);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }
    }
}
