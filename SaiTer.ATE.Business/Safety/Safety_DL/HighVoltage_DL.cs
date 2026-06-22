using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;


namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 交流、直流耐压测试类
    /// </summary>
    public class HighVoltage_DL : BusinessBase
    {
        float trlTimeOut_S = 8;//超时时间

        //界面展示的数据项格式
        //交（直）流耐压值(kV)|HISET电流值(mA)|LOSET电流值(mA)| 测试时间(S)|斜坡时间(S)|ACW(DCW)参考值(mA)|ARC电流值(mA)|测试频率(Hz)

        string VoltageValue, HISETCurrent, LOSETCurrent, TestTime, RampTime, ACW_DCW, ARC, TestFreq;
        List<bool> list = new List<bool>() { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };//程控板继电器
        /// <summary>
        /// 小的检定点名称
        /// </summary>
        string TrialFlowName = "";
        public HighVoltage_DL(int trialType)
        {
            TrialType = trialType;
        }
        /// <summary>
        /// 初始化当前检测项信息 
        /// </summary>
        public override void InitializeParams()
        {
            ControlEquipMent.ACSource.ACSource_OFF(lstIDs);
            string[] strParams = TrialItem.ResultParams.Split('|');
            VoltageValue = strParams[0].Split('=')[1].Trim('\r');
            HISETCurrent = strParams[1].Split('=')[1].Trim('\r');
            LOSETCurrent = strParams[2].Split('=')[1].Trim('\r');
            TestTime = strParams[3].Split('=')[1].Trim('\r');
            RampTime = strParams[4].Split('=')[1].Trim('\r');
            ACW_DCW = strParams[5].Split('=')[1].Trim('\r');
            ARC = strParams[6].Split('=')[1].Trim('\r');
            if (strParams.Length >= 8)
            {
                TestFreq = strParams[7].Split('=')[1].Trim('\r');
            }
        }
        /// <summary>
        /// 设备初始化
        /// </summary>
        public override void InitEquiMent()
        {
            SendNoticeToUIAndTxtFile("切换继电器状态");
            EmTrialType emTrialType = (EmTrialType)TrialType;
            switch (emTrialType)
            {
                case (EmTrialType.直流耐压_输入对地):
                case (EmTrialType.交流耐压_输入对地):
                case (EmTrialType.绝缘电阻_输入对地):
                    //K2,K4
                    list[5] = true;
                    list[7] = true;
                    break;
                case (EmTrialType.直流耐压_输出对地):
                case (EmTrialType.绝缘电阻_输出对地):
                case (EmTrialType.交流耐压_输出对地):
                    //K1,K4
                    list[4] = true;
                    list[7] = true;
                    break;
                case (EmTrialType.绝缘电阻_输入对输出):
                case (EmTrialType.直流耐压_输入对输出):
                case (EmTrialType.交流耐压_输入对输出):
                    //K2,K3
                    list[5] = true;
                    list[6] = true;
                    break;
                case (EmTrialType.接地试验1):
                case (EmTrialType.接地试验2):
                case (EmTrialType.接地试验3):
                    //K4,K5
                    list[7] = true;
                    list[8] = true;
                    break;
            }
            ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);
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
                ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "FUNC:TEST OFF", "\r\n", "\r\n");
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
                                //交流耐压值(kV)|HISET电流值(mA)|LOSET电流值(mA)| 测试时间(S)|斜坡时间(S)|ACW参考值(mA)|ARC电流值(mA)|测试频率(Hz)|测试结果
                                //LstTrialData[k].ExtentData = Result.LstData[0].ToString().TrimEnd('\0');
                                //LstTrialData[i].ExtentData = VoltageValue + "|" + HISETCurrent + "|" + LOSETCurrent + "|" +
                                //    TestTime + "|" + RampTime + "|" + ACW_DCW + "|" + ARC;
                                //if (TrialType == (int)EmTrialType.交流耐压_输入对地
                                //      || TrialType == (int)EmTrialType.交流耐压_输出对地
                                //      || TrialType == (int)EmTrialType.交流耐压_输入对输出)
                                //{
                                //    LstTrialData[i].ExtentData += "|" + TestFreq;
                                //}
                                LstTrialData[i].ExtentData = TrialItem.ItemName.Split('_')[0] + "|" + TrialItem.ItemName.Split('_')[1] + "|-|" + HISETCurrent + "|null";
                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                ControlEquipMent.Safety.SafetyOFF(testWorkParam.lstIDs);
                Thread.Sleep(2000);
                int waitTime = 100;
                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }

                SendNoticeToUIAndTxtFile("正在设置安规参数");
                ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MAIN:FUNC MANU ", "\r\n", "\r\n");
                if (TrialType == (int)EmTrialType.直流耐压_输入对地
                   || TrialType == (int)EmTrialType.直流耐压_输出对地
                   || TrialType == (int)EmTrialType.直流耐压_输入对输出)
                {
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:EDIT:MODE DCW ", "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:DCW:VOLT " + VoltageValue, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:DCW:CHIS " + HISETCurrent, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:DCW:CHIS " + LOSETCurrent, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:DCW:TTIM " + TestTime, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:RTIM " + RampTime, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:DCW:REF " + ACW_DCW, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:DCW:ARCC " + ARC, "\r\n", "\r\n");
                }

                if (TrialType == (int)EmTrialType.交流耐压_输入对地
                    || TrialType == (int)EmTrialType.交流耐压_输出对地
                    || TrialType == (int)EmTrialType.交流耐压_输入对输出)
                {
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:EDIT:MODE ACW ", "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:ACW:VOLT " + VoltageValue, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:ACW:CHIS " + HISETCurrent, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:ACW:CHIS " + LOSETCurrent, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:ACW:TTIM " + TestTime, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:RTIM " + RampTime, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:ACW:REF " + ACW_DCW, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:ACW:ARCC " + ARC, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:ACW:FREQ " + TestFreq, "\r\n", "\r\n");
                }

                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    SendNoticeToUIAndTxtFile("启动安规检测" + testWorkParam.lstIDs[i] + "号枪，大约需要 " + (double.Parse(TestTime) + 2.00).ToString() + "秒，等待安规测试仪结果");
                    ControlEquipMent.Safety.SafetySetParam(new List<int>(1) { testWorkParam.lstIDs[i] }, "FUNC:TEST ON", "\r\n", "\r\n");
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    while (sw.ElapsedMilliseconds < (Convert.ToInt32(double.Parse(TestTime)) + 2) * 1000)
                    {
                        int t = (Convert.ToInt32(double.Parse(TestTime)) + 2) - (int)sw.ElapsedMilliseconds / 1000;
                        SystemEvent.SendLogMessage("剩余时间 " + t + " 秒   \r\t  \r\t ");
                        int sleepTime = 1;
                        if (t > 10)
                        {
                            sleepTime = 5;
                        }
                        else
                        {
                            sleepTime = 2;
                        }
                        Thread.Sleep(sleepTime * 1000);
                    }
                    sw.Stop();

                    SendNoticeToUIAndTxtFile("正在读取安规数据");

                    int k = LstTrialData.FindIndex(s => s.ChargerId == testWorkParam.lstIDs[i]);



                    string strResult = ControlEquipMent.Safety.SafetyReadResult(new List<int>(1) { testWorkParam.lstIDs[i] }, "MEAS?", "\r\n", "\r\n");
                    string[] ReturnStrS = strResult.Split(',');
                    string data = "";
                    if (ReturnStrS.Length >= 5)
                    {
                        data = TrialItem.ItemName.Split('_')[0] + "|" + TrialItem.ItemName.Split('_')[1] + "|" + LOSETCurrent + "|" + HISETCurrent + "|" + ReturnStrS[3];

                        // LstTrialData[k].TrialValue = ReturnStrS[1];
                        if (ReturnStrS[1].ToUpper().Contains("PASS"))
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        }
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    int j = LstChargerInfo.FindIndex(s => s.ChargerId == testWorkParam.lstIDs[i]);
                    LstTrialData[k].BarCode = LstChargerInfo[j].BarCode;
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    LstTrialData[k].ItemName = TrialFlowName;
                    LstTrialData[k].PKID = LstChargerInfo[j].PKID;
                    LstTrialData[k].ExtentData = data;
                    LstTrialData[k].Data2 = data;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].TrialCondition = "供电电压V=0|供电电流A=0|供电频率=0";
                    SendTrialDataToUI(LstTrialData[k]);
                    SaveTrialData(LstTrialData[k]);

                }

            }
        }



        public override void ProcessData()
        {
            Stopwatch TempTime = new Stopwatch();
            TempTime.Reset();
            TempTime.Start();
            while (true)
            {
                if (ResultData.Count >= testWorkParam.lstIDs.Count)
                {
                    break;
                }
                if (TempTime.ElapsedMilliseconds / 1000 > 30)
                {
                    break;
                }
                Thread.Sleep(100);
            }
            TempTime.Stop();
            try
            {
                while (ResultData.Count > 0)
                {
                    string data = TrialItem.ItemName.Split('_')[0] + "|" + TrialItem.ItemName.Split('_')[1] + "|-|" + HISETCurrent + "|null";
                    StResultData Result = ResultData.Dequeue();
                    int k = LstTrialData.FindIndex(s => s.ChargerId == Result.ChargeId);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == Result.ChargeId);
                    if (k < 0)
                        continue;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                    if (Result.LstData == null)
                    {
                        //continue;
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        testWorkParam.lstIDs.Remove(Result.ChargeId);
                    }

                    else
                    {
                        LstTrialData[k].Data1 = Result.LstData[0].ToString();
                        string[] ReturnStrS = Result.LstData[0].ToString().Split(',');
                        if (ReturnStrS.Length >= 5)
                        {
                            data = TrialItem.ItemName.Split('_')[0] + "|" + TrialItem.ItemName.Split('_')[1] + "|-|" + HISETCurrent + "|" + ReturnStrS[3];
                            LstTrialData[k].TrialValue = ReturnStrS[1];
                            if (ReturnStrS[1].ToUpper().Contains("PASS"))
                            {
                                LstTrialData[k].TrialResult = EmTrialResult.Pass;
                            }
                            else
                            {
                                LstTrialData[k].TrialResult = EmTrialResult.Fail;
                                testWorkParam.lstIDs.Remove(Result.ChargeId);
                            }
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            testWorkParam.lstIDs.Remove(Result.ChargeId);
                        }
                    }
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();

                    //界面展示的数据项格式
                    //交流耐压值(kV)|HISET电流值(mA)|LOSET电流值(mA)| 测试时间(S)|斜坡时间(S)|ACW参考值(mA)|ARC电流值(mA)|测试频率(Hz)|测试结果
                    //LstTrialData[k].ExtentData = Result.LstData[0].ToString().TrimEnd('\0');
                    //string data = VoltageValue + "|" + HISETCurrent + "|" + LOSETCurrent + "|" +
                    //                TestTime + "|" + RampTime + "|" + ACW_DCW + "|" + ARC;
                    //if (TrialType == (int)EmTrialType.交流耐压_输入对地
                    //    || TrialType == (int)EmTrialType.交流耐压_输出对地
                    //    || TrialType == (int)EmTrialType.交流耐压_输入对输出)
                    //{
                    //    data += "|" + TestFreq;
                    //}
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
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
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

    }
}
