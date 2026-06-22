using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;


namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 安规-接地测试类
    /// </summary>
    public class EarthingConductor : BusinessBase
    {
        List<int> ChargerIndexLst = new List<int>();
        float trlTimeOut_S = 8;//超时时间
        string GBCurrent, HISETResistance, LOSETResistance, TestTime, RampTime, GBReferenceValue, TestFreq;
        string info = "";
        int index = -1;//不同测量点需要控制的继电器索引号
        List<int> lst_Swichs = new List<int>();//多个继电器开关的集合
        public EarthingConductor(int trialType)
        {
            TrialType = trialType;
        }
        /// <summary>
        /// 初始化当前检测项信息 
        /// </summary>
        public override void InitializeParams()
        {
            Init();
            lst_Swichs.Clear();

            //GB电流值(A)|HISET电阻值(MΩ)|LOSET电阻值(MΩ)| 测试时间(S)|斜坡时间(S)|GB参考值(MΩ)|测试频率(Hz)
            string[] strParams = TrialItem.ResultParams.Split('|');
            GBCurrent = strParams[0].Split('=')[1].Trim('\r');
            HISETResistance = strParams[1].Split('=')[1].Trim('\r');
            LOSETResistance = strParams[2].Split('=')[1].Trim('\r');
            TestTime = strParams[3].Split('=')[1].Trim('\r');
            RampTime = strParams[4].Split('=')[1].Trim('\r');
            GBReferenceValue = strParams[5].Split('=')[1].Trim('\r');
            TestFreq = strParams[6].Split('=')[1].Trim('\r');

            string[] strParams2 = TrialItem.ItemParams.Split('|');
            if (strParams2[0].Split('=').Count() != 1)
            {
                index = Convert.ToInt32(strParams2[0].Split('=')[1].Trim('\r')) - 1;
            }
            for(int i = 0; i < strParams2.Length; i++)
            {
                if (strParams2[i].Split('=').Count() != 1)
                {
                    lst_Swichs.Add(Convert.ToInt32(strParams2[i].Split('=')[1].Trim('\r')) - 1);
                }
            }
        }
        /// <summary>
        /// 设备初始化
        /// </summary>
        public override void InitEquiMent()
        {
            ControlEquipMent.ACSource?.ACSource_OFF(lstIDs);
            SendNoticeToUIAndTxtFile("切换继电器状态");
            List<bool> list = new List<bool>() { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
            EmTrialType emTrialType = (EmTrialType)TrialType;
            switch (emTrialType)
            {
                case (EmTrialType.直流耐压_输入对地):
                case (EmTrialType.交流耐压_输入对地):
                case (EmTrialType.绝缘电阻_输入对地):
                    //K2,K4
                    list[5] = true;
                    list[7] = true;

                    info = "闭合KM2、KM4 (程控板对应S6、S8继电器";
                    foreach (int itmp in lst_Swichs)
                    {
                        list[itmp] = true;
                        info = info + "、S" + (itmp + 1).ToString() + "继电器";
                    }
                    info = info + ")";
                    break;
                case (EmTrialType.直流耐压_输出对地):
                case (EmTrialType.绝缘电阻_输出对地):
                case (EmTrialType.交流耐压_输出对地):
                    //K1,K4
                    list[4] = true;
                    list[7] = true;
                    info = "闭合KM1、KM4 (程控板对应S5、S8继电器";
                    foreach (int itmp in lst_Swichs)
                    {
                        list[itmp] = true;
                        info = info + "、S" + (itmp + 1).ToString() + "继电器";
                    }
                    info = info + ")";
                    break;
                case (EmTrialType.绝缘电阻_输入对输出):
                case (EmTrialType.直流耐压_输入对输出):
                case (EmTrialType.交流耐压_输入对输出):
                    //K2,K3
                    list[5] = true;
                    list[6] = true;
                    //info = "闭合KM2、KM3 (程控板对应S6、S7继电器)";
                    info = "闭合KM2、KM3 (程控板对应S6、S7继电器";
                    foreach (int itmp in lst_Swichs)
                    {
                        list[itmp] = true;
                        info = info + "、S" + (itmp + 1).ToString() + "继电器";
                    }
                    info = info + ")";
                    break;
                case (EmTrialType.接地试验1):
                case (EmTrialType.接地试验2):
                case (EmTrialType.接地试验3):
                    //K4,K5
                    list[7] = true;
                    list[8] = true;
                    list[index] = true;
                    //info = "闭合KM4、KM5 (程控板对应S8、S9继电器，以及S" + Convert.ToInt32(index + 1) + ")";
                    info = "闭合KM4、KM5 (程控板对应S8、S9继电器";
                    foreach (int itmp in lst_Swichs)
                    {
                        list[itmp] = true;
                        info = info + "、S" + (itmp + 1).ToString() + "继电器";
                    }
                    info = info + ")";
                    break;
            }
            Thread.Sleep(500);
            ChargerIndexLst = new List<int> { 1 };
            string SaftyControlNumber = ConfigurationManager.AppSettings["SaftyControlNumber"];
            if (SaftyControlNumber != null && int.TryParse(SaftyControlNumber, out int num))
            {
                ChargerIndexLst = new List<int> { num };
            }
            ControlEquipMent.ControlBoard.ControlResistanceSetRelay(ChargerIndexLst, list);
            Thread.Sleep(500);

            //ControlEquipMent.ControlBoard.ControlResistanceSetRelay(ChargerIndexLst, list);
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
                                //GB电流值(A)|HISET电阻值(MΩ)|LOSET电阻值(MΩ)| 测试时间(S)|斜坡时间(S)|GB参考值(MΩ)|测试频率(Hz)
                                //LstTrialData[k].ExtentData = Result.LstData[0].ToString().TrimEnd('\0');
                                LstTrialData[i].ExtentData = "接地测试|接地|-|" + HISETResistance + "|null";
                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                ControlEquipMent.Safety.SafetyOFF(testWorkParam.lstIDs);
                Thread.Sleep(100);
                int waitTime = 50;
                if (testWorkParam.lstIDs.Count > 0)
                {
                    SendNoticeToUIAndTxtFile("正在设置安规参数");
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MAIN:FUNC MANU ", "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:EDIT:MODE GB ", "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:GB:CURR " + GBCurrent, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:GB:RHIS " + HISETResistance, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:GB:RLOS " + LOSETResistance, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:GB:TTIM " + TestTime, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:RTIM " + RampTime, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:GB:REF " + GBReferenceValue, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MANU:GB:FREQ " + TestFreq, "\r\n", "\r\n");
                    Thread.Sleep(waitTime);
                    SendNoticeToUIAndTxtFile(info + ",启动安规检测，大约需要 " + (double.Parse(TestTime) + 1.00).ToString() + "秒，等待安规测试仪结果");
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "FUNC:TEST ON", "\r\n", "\r\n");
                    Thread.Sleep(500);
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    int num = Convert.ToInt32(double.Parse(TestTime));
                    int testTime = (Convert.ToInt32(double.Parse(TestTime))) * 1000 + Convert.ToInt32(double.Parse(RampTime) * 1000);
                    while (sw.ElapsedMilliseconds < testTime)
                    {
                        try
                        {
                            //解决安规已经FAIL但是还在倒计时的问题
                            ControlEquipMent.Safety.SafetyReadParam(testWorkParam.lstIDs, "MEAS?", "\r\n", "\r\n");
                            StResultData Result = ResultData.Dequeue();
                            //按下安规的停止物理按钮
                            if (Result.LstData == null || Result.LstData.Count < 1) break;
                            string[] ReturnStrS = Result.LstData[0].ToString().Split(',');
                            string rest = ReturnStrS[1].ToUpper().Trim();
                            if (rest.Equals("FAIL") || rest.Equals("ERROR") || rest.Equals("STOP"))
                            {
                                ControlEquipMent.Safety.SafetyOFF(testWorkParam.lstIDs);
                                Thread.Sleep(500);
                                break;
                            }

                            int t = testTime - (int)sw.ElapsedMilliseconds;

                            if (t / 1000 % 5 == 0)
                            {
                                if (num != t / 1000)
                                {
                                    SystemEvent.SendLogMessage("剩余时间 " + t / 1000 + "秒   \r\t  \r\t ");
                                    num = t / 1000;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Log.LogException(ex);
                        }
                        Thread.Sleep(100);
                    }
                    sw.Stop();
                    //恢复开关状态
                    List<bool> list = new List<bool>() { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(ChargerIndexLst, list);
                    Thread.Sleep(500);
                    SendNoticeToUIAndTxtFile("正在读取安规数据");

                    ControlEquipMent.Safety.SafetyReadParam(testWorkParam.lstIDs, "MEAS?", "\r\n", "\r\n");
                    //开始判断数据
                    ProcessData();

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
                    string data = "接地测试|接地|-|" + HISETResistance + "|null";
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
                            //状态|数据项|下限|上限|测量值
                            data = "接地测试|接地|-|" + HISETResistance + "|" + ReturnStrS[3];
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
                            //状态|数据项|下限|上限|测量值
                            data = "接地测试|接地|-|" + HISETResistance + "|null";
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                            testWorkParam.lstIDs.Remove(Result.ChargeId);
                        }
                    }
                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();


                    //界面展示的数据项格式
                    ////GB电流值(A)|HISET电阻值(MΩ)|LOSET电阻值(MΩ)| 测试时间(S)|斜坡时间(S)|GB参考值(MΩ)|测试频率(Hz) 
                    ////LstTrialData[k].ExtentData = Result.LstData[0].ToString().TrimEnd('\0');
                    //string data = GBCurrent + "|" + HISETResistance + "|" + LOSETResistance + "|" +
                    //                TestTime + "|" + RampTime + "|" + GBReferenceValue + "|" + TestFreq;



                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    LstTrialData[k].ExtentData = data;
                    LstTrialData[k].Data2 = data;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    //LstTrialData[k].TrialCondition = "供电电压V=" + AllEquipStateData.DicACSource_StateData[LstTrialData[k].ChargerId].Volt + "|" +
                    //    "供电电流A=" + AllEquipStateData.DicACSource_StateData[LstTrialData[k].ChargerId].Current + "|" +
                    //    "供电频率=" + AllEquipStateData.DicACSource_StateData[LstTrialData[k].ChargerId].Freq;

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
