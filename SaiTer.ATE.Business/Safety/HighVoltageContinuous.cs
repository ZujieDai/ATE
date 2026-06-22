using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 交流、直流耐压测试类（连续测试）
    /// </summary>
    public class HighVoltageContinuous : BusinessBase
    {
        string FlowItemName;
        float trlTimeOut_S = 8;//超时时间

        //界面展示的数据项格式
        //交（直）流耐压值(kV)|HISET电流值(mA)|LOSET电流值(mA)| 测试时间(S)|斜坡时间(S)|ACW(DCW)参考值(mA)|ARC电流值(mA)|测试频率(Hz)

        string VoltageValue, HISETCurrent, LOSETCurrent, TestTime, RampTime, ACW_DCW, ARC, TestFreq;
        string info = "";
        string InputSwitch = "";
        public HighVoltageContinuous(int trialType)
        {
            TrialType = trialType;
        }
        /// <summary>
        /// 初始化当前检测项信息 
        /// </summary>
        public override void InitializeParams()
        {
            ControlEquipMent.ACSource?.ACSource_OFF(lstIDs);
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
            //和输入有关的测试项，要加一个闭合开关
            //输入闭合开关（从1开始）=14
            string[] otherParams = TrialItem.ItemParams?.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            if (otherParams[0].Split('=').Length > 1)
                InputSwitch = otherParams[0].Split('=')[1].Trim('\r');
        }
        /// <summary>
        /// 设备初始化
        /// </summary>
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
                //保存试验结果
                ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "FUNC:TEST OFF", "\r\n", "\r\n");
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
        }

        /// <summary>
        /// 根据类型切换继电器状态
        /// </summary>
        /// <param name="type">0-输入对地，1-输出对地，2-输入对输出</param>
        private void SetControlboard(int type)
        {
            SendNoticeToUIAndTxtFile("切换继电器状态");
            List<bool> list = new List<bool>() { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
            int sw;
            switch (type)
            {
                case 0:
                    //K2,K4
                    list[5] = true;
                    list[7] = true;
                    if (!string.IsNullOrEmpty(InputSwitch) && int.TryParse(InputSwitch, out sw))
                    {
                        sw--;
                        list[sw] = true;
                        info = "闭合KM2、KM4 (程控板对应S6、S8继电器，以及S" + Convert.ToInt32(sw + 1) + ")";
                    }
                    else
                        info = "闭合KM2、KM4 (程控板对应S6、S8继电器";
                    break;
                case 1:
                    //K1,K4
                    list[4] = true;
                    list[7] = true;
                    info = "闭合KM1、KM4 (程控板对应S5、S8继电器";
                    break;
                case 2:
                    //K2,K3
                    list[5] = true;
                    list[6] = true;
                    if (!string.IsNullOrEmpty(InputSwitch) && int.TryParse(InputSwitch, out sw))
                    {
                        sw--;
                        list[sw] = true;
                        info = "闭合KM2、KM3 (程控板对应S6、S7继电器，以及S" + Convert.ToInt32(sw + 1) + ")";
                    }
                    else
                        info = "闭合KM2、KM3 (程控板对应S6、S7继电器";
                    break;
            }
            Thread.Sleep(300);
            ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);
        }

        private void StarTestItem()
        {
            ControlEquipMent.Safety.SafetyOFF(testWorkParam.lstIDs);
            Thread.Sleep(100);

            SendNoticeToUIAndTxtFile(info + ", 启动安规检测，大约需要 " + (double.Parse(TestTime) + double.Parse(RampTime)).ToString() + "秒，等待安规测试仪结果");
            ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "FUNC:TEST ON", "\r\n", "\r\n");
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
            Thread.Sleep(500);

            SendNoticeToUIAndTxtFile("正在读取安规数据");

            ControlEquipMent.Safety.SafetyReadParam(testWorkParam.lstIDs, "MEAS?", "\r\n", "\r\n");

            //开始判断数据
            ProcessData();
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
                                LstTrialData[i].ExtentData = TrialItem.ItemName + "|测试超时|-|" + HISETCurrent + "|null";
                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                if (testWorkParam.lstIDs.Count > 0)
                {
                    int waitTime = 10;
                    SendNoticeToUIAndTxtFile("正在设置安规参数");
                    ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "MAIN:FUNC MANU ", "\r\n", "\r\n");
                    if (TrialType == (int)EmTrialType.直流耐压)
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
                    if (TrialType == (int)EmTrialType.交流耐压)
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

                    FlowItemName = "输入对地";
                    SetControlboard(0);
                    StarTestItem();

                    FlowItemName = "输出对地";
                    SetControlboard(1);
                    StarTestItem();

                    FlowItemName = "输入对输出";
                    SetControlboard(2);
                    StarTestItem();

                    //恢复开关状态
                    List<bool> list = new List<bool>() { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);
                    Thread.Sleep(300);
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
                    string data = TrialItem.ItemName + "|" + FlowItemName + "|" + LOSETCurrent + "|" + HISETCurrent + "|null";
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
                        //testWorkParam.lstIDs.Remove(Result.ChargeId);
                    }

                    else
                    {
                        LstTrialData[k].Data1 = Result.LstData[0].ToString();
                        string[] ReturnStrS = Result.LstData[0].ToString().Split(',');
                        if (ReturnStrS.Length >= 5)
                        {
                            data = TrialItem.ItemName + "|" + FlowItemName + "|" + LOSETCurrent + "|" + HISETCurrent + "|" + ReturnStrS[3];
                            LstTrialData[k].TrialValue = ReturnStrS[1];
                            //惠州TB不判断安规测试仪返回的判定结果，只判断测量值
                            string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                            if (ReturnStrS[1].ToUpper().Contains("PASS") || (Customer != null && Customer.Contains("TB")))
                            {
                                LstTrialData[k].TrialResult = EmTrialResult.Pass;
                                //   00.94 mA
                                string str = ReturnStrS[3].Split(' ')[0];
                                if (Convert.ToDouble(str) > Convert.ToDouble(HISETCurrent) ||
                                    Convert.ToDouble(str) < Convert.ToDouble(LOSETCurrent))
                                {
                                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                                }
                                else
                                {
                                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                                }
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
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
        }

    }
}
