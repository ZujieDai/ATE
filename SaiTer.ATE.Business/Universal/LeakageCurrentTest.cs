using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.InterFace;
using System.Diagnostics;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 漏电流保护测试（测试仪器为中佳漏电板）
    /// </summary>
    public class LeakageCurrentTest : BusinessBase
    {
        public LeakageCurrentTest(int type) { TrialType = type; }
        string ItemFlow = "";
        string State = "";
        private int 检测时间, 额定电流In, In倍数, 动作电流上限, 动作电流下限, 动作时间上限, 动作时间下限, 电流上升率, 不动作电流输出时间;
        private int 测试电流上限值, 测试时间上限值, 测试结束延时时间, 测试开始延时时间;
        public override void InitializeParams()
        {
            //数据库参数格式
            //检测时间(S)=10|AC额定电流In(mA)=30.0|AC-In倍数=1.0|AC动作电流上限(mA)=30.0|
            //AC动作电流下限(mA)=0.0|AC动作时间上限(ms)=100.0|AC动作时间下限(ms)=0.0|AC电流上升率(mA/s)=1.0|
            //AC不动作电流输出时间(s)=0.1000|测试电流上限值(mA)=35|测试时间上限值(ms)=100|测试结束延时时间(S)=5|
            //测试开始延时时间(S)=5

            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            检测时间 = Convert.ToInt32(double.Parse(strParams[0].Split('=')[1]));
            额定电流In = Convert.ToInt32(double.Parse(strParams[1].Split('=')[1]));
            In倍数 = Convert.ToInt32(double.Parse(strParams[2].Split('=')[1]));
            动作电流上限 = Convert.ToInt32(double.Parse(strParams[3].Split('=')[1]));
            动作电流下限 = Convert.ToInt32(double.Parse(strParams[4].Split('=')[1]));
            动作时间上限 = Convert.ToInt32(double.Parse(strParams[5].Split('=')[1]));
            动作时间下限 = Convert.ToInt32(double.Parse(strParams[6].Split('=')[1]));
            电流上升率 = Convert.ToInt32(double.Parse(strParams[7].Split('=')[1]));
            不动作电流输出时间 = Convert.ToInt32(double.Parse(strParams[8].Split('=')[1]));
            测试电流上限值 = Convert.ToInt32(double.Parse(strParams[9].Split('=')[1]));
            测试时间上限值 = Convert.ToInt32(double.Parse(strParams[10].Split('=')[1]));
            测试结束延时时间 = Convert.ToInt32(double.Parse(strParams[11].Split('=')[1]));
            测试开始延时时间 = Convert.ToInt32(double.Parse(strParams[12].Split('=')[1]));
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
                SetCPReresh();
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
                                    LstTrialData[i].ExtentData = "null|null|null|null|null";

                                    SendTrialDataToUI(LstTrialData[i]);
                                }
                            }
                        }
                        break;
                    }

                    if (!CheckChargerIn(testWorkParam.lstIDs))
                    {
                        return;
                    }


                    ItemFlow = "测动作电流";
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                    int sleepTime = 20;
                    SendNoticeToUIAndTxtFile("设置漏电仪参数");


                    if (TrialType == (int)EmTrialType.漏电保护试验_交流模式)
                    {
                        ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 11, 0);//切换交流
                        Thread.Sleep(sleepTime);
                        ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 12, 0);//交流模式，测动作电流
                        Thread.Sleep(sleepTime);
                    }
                    else if (TrialType == (int)EmTrialType.漏电保护试验_脉动直流模式)
                    {
                        ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 11, 1);//切换脉动直流
                        ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 13, 0);//脉动直流模式，测动作电流
                    }



                    SetLeakageCurrentParams(sleepTime);

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    SetS2S1ON();

                    //设置测试条件
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        int key = testWorkParam.lstIDs[i];
                        if (AllEquipStateData.DicBMS_AC_StateData.Count == 1)
                        {
                            key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                        }
                        //d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                        //d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                        d1.Add(testWorkParam.lstIDs[i], "220");//青岛HR产测没有交流源，暂时固定
                        d2.Add(testWorkParam.lstIDs[i], "50");
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);

                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    List<int> lstCount = Clone<int>(testWorkParam.lstIDs);

                    while (sw.ElapsedMilliseconds < 100 * 1000)
                    {
                        for (int i = 0; i < lstCount.Count; i++)
                        {
                            //lstCount.Add(testWorkParam.lstIDs[i]);
                            int t = 200 - (int)sw.ElapsedMilliseconds / 1000;
                            //SystemEvent.SendLogMessage("剩余时间 " + t + "秒   \r\t  \r\t ");
                            string AlarmInfo = AllEquipStateData.DicZJLeakageCurrent_StateData[lstCount[i]].AlarmInfo;
                            if (AlarmInfo.Contains("正常") || AlarmInfo.Contains("异常"))
                            {
                                // if (++lstCount[testWorkParam.lstIDs[i]] >= 4)
                                lstCount.RemoveAt(i);
                            }
                        }
                        if (lstCount.Count == 0)
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                    sw.Stop();
                    Thread.Sleep(2000);
                    ProcessData();

                    CountDownTimeInfo("请恢复漏保后点击确定", 999, 0);

                    #region ========青岛HR客户要求不测动作时间+====

                    string Customer = ConfigurationManager.AppSettings["Customer"];
                    if (!string.IsNullOrEmpty(Customer) && !Customer.Equals("HR"))
                    {
                        ItemFlow = "测动作时间";
                        if (testWorkParam.lstIDs.Count == 0)
                        {
                            return;
                        }
                        SendNoticeToUIAndTxtFile("关闭导引");

                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(3000);
                        ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                        SendNoticeToUIAndTxtFile("设置漏电仪参数");

                        if (TrialType == (int)EmTrialType.漏电保护试验_交流模式)
                        {
                            ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 11, 0);//交流模式
                            Thread.Sleep(sleepTime);
                            ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 12, 2);//突然电流测动作时间
                            Thread.Sleep(sleepTime);
                        }
                        else if (TrialType == (int)EmTrialType.漏电保护试验_脉动直流模式)
                        {
                            ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 11, 1);//脉动直流模式
                            Thread.Sleep(sleepTime);
                            ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 13, 1);//突然电流测动作时间
                            Thread.Sleep(sleepTime);
                        }

                        SetLeakageCurrentParams(sleepTime);
                        SendNoticeToUIAndTxtFile("设置漏电仪预先调整电流");

                        ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 41, 1);//预先调整电流
                        Thread.Sleep(3000);
                        SendNoticeToUIAndTxtFile("等待预先调整动作电流");

                        int time = 200;


                        sw.Start();
                        lstCount.Clear();
                        lstCount = Clone<int>(testWorkParam.lstIDs);
                        while (sw.ElapsedMilliseconds < time * 1000)
                        {
                            int t = 200 - (int)sw.ElapsedMilliseconds / 1000;
                            SystemEvent.SendLogMessage("剩余时间 " + t + "秒   \r\t  \r\t ");
                            int count = 0;
                            for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                            {

                                string strInfo = AllEquipStateData.DicZJLeakageCurrent_StateData[testWorkParam.lstIDs[i]].PresetCurrent;
                                if (strInfo != null && strInfo.Contains("完成"))
                                {
                                    lstCount.Remove(testWorkParam.lstIDs[i]);
                                }
                            }
                            if (lstCount.Count == 0)
                            {
                                break;
                            }
                            Thread.Sleep(2000);
                        }
                        sw.Stop();
                        SendNoticeToUIAndTxtFile("测试开始延时" + 测试开始延时时间 + "S，请稍候...");


                        SendNoticeToUIAndTxtFile("设备正在自检，请稍候...");

                        for (int n = 0; n < 10; n++)
                        {
                            // SetS2S1ON();
                            ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 46, 1);//S2开
                            Thread.Sleep(1000);

                            ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 45, 1);//S1开
                            Thread.Sleep(1000);
                            bool isOK = true;
                            for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                            {
                                string AlarmInfo = AllEquipStateData.DicZJLeakageCurrent_StateData[testWorkParam.lstIDs[i]].AlarmInfo;
                                if (!AlarmInfo.Contains("OK"))
                                {
                                    isOK = false;
                                }
                            }
                            if (isOK)//所有的漏电板状态都变成“复位OK”
                            {
                                break;
                            }
                        }
                        ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                        //注：此处不能使用刷卡自动判断的方法。 因为上电后瞬间会脱扣，BMS电压值上不去，会一直等待刷卡
                        CountDownTimeInfo("请给桩刷卡上电,刷卡后点击确认按钮，或者等待倒计时结束自动判断", 30, 0);
                        Thread.Sleep(2000);
                        ProcessTimeData();
                    }
                    #endregion
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

       

        private void SetS2S1ON()
        {
            SendNoticeToUIAndTxtFile("闭合漏电仪S2,S1,开始检测");
            ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 44, 1);//复位            

            Thread.Sleep(1000);
            ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 46, 1);//S2开
            Thread.Sleep(2000);

            ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 45, 1);//S1开
            Thread.Sleep(2000);
        }

        private void SetLeakageCurrentParams(int sleepTime)
        {
            if (TrialType == (int)EmTrialType.漏电保护试验_交流模式)
            {

                ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 14, 额定电流In * 10);
                Thread.Sleep(sleepTime);
                ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 16, In倍数 * 10);
                Thread.Sleep(sleepTime);
                ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 19, 动作电流上限 * 10);
                Thread.Sleep(sleepTime);
                ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 20, 动作电流下限 * 10);
                Thread.Sleep(sleepTime);
                ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 21, 动作时间上限 * 10);
                Thread.Sleep(sleepTime);
                ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 22, 动作时间下限 * 10);
                Thread.Sleep(sleepTime);
                ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 10, 电流上升率 * 10);
                Thread.Sleep(sleepTime);
            }
            else if (TrialType == (int)EmTrialType.漏电保护试验_脉动直流模式)
            {

                ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 18, 0);//角度  0°

                ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 15, 额定电流In * 10);
                Thread.Sleep(sleepTime);
                ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 17, In倍数 * 10);
                Thread.Sleep(sleepTime);
                ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 23, 动作电流上限 * 10);
                Thread.Sleep(sleepTime);
                ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 24, 动作电流下限 * 10);
                Thread.Sleep(sleepTime);
                ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 25, 动作时间上限 * 10);
                Thread.Sleep(sleepTime);
                ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 26, 动作时间下限 * 10);
                Thread.Sleep(sleepTime);
                ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 27, 电流上升率 * 10);
                Thread.Sleep(sleepTime);
            }


            ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 28, 不动作电流输出时间 * 10 * 1000);
            Thread.Sleep(sleepTime);
            ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 40, 0);//R相
            Thread.Sleep(sleepTime);
            ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 34, 0);//S3关
            Thread.Sleep(sleepTime);
            ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 44, 1);//复位
            Thread.Sleep(sleepTime);
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
                double voltage = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Voltage;
                double current = AllEquipStateData.DicZJLeakageCurrent_StateData[item].TestCurrent;
                if (voltage > LstChargerInfo[0].NominalVoltage - 10 && voltage < LstChargerInfo[0].NominalVoltage + 10)
                {
                    //电压未断开，能充电，FAIL
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    LstTrialData[k].ExtentData = TrialItem.ItemName + "|测动作电流(mA)|0|30|未脱扣";
                }
                else
                {
                    if (current > 测试电流上限值)
                    {
                        //测试电流大于"测试电流上限值(mA)"  不合格
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|测动作电流(mA)|0|30|" + current.ToString("F2");
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|测动作电流(mA)|0|30|" + current.ToString("F2");
                    }
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].ItemName = ItemFlow;
                SendTrialDataToUI(LstTrialData[k]);
                SaveTrialData(LstTrialData[k]);
            }
        }

        private void ProcessTimeData()
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
                double voltage = AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Voltage;
                double testTime = AllEquipStateData.DicZJLeakageCurrent_StateData[item].TestTime;
                if (voltage > LstChargerInfo[0].NominalVoltage - 10 && voltage < LstChargerInfo[0].NominalVoltage + 10)
                {
                    //电压未断开，能充电，FAIL
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    LstTrialData[k].ExtentData = TrialItem.ItemName + "|测动作时间(mS)|0|100|未脱扣";
                }
                else
                {
                    if (testTime > 测试时间上限值)
                    {
                        //  不合格
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|测动作时间(mS)|0|100|" + testTime.ToString("F2");
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|测动作时间(mS)|0|100|" + testTime.ToString("F2");
                    }
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].ItemName = ItemFlow;
                SendTrialDataToUI(LstTrialData[k]);
                SaveTrialData(LstTrialData[k]);
            }
        }
    }
}
