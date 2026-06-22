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
using System.Data;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 漏电流保护测试（测试仪器为拓勉漏电仪）
    /// </summary>
    public class LeakageCurrentTest_TM : BusinessBase
    {
        public LeakageCurrentTest_TM(int type) { TrialType = type; }
        Dictionary<int, string> dicResult = new Dictionary<int, string>();//测试结果数据
        string ItemFlow = "";
        private int 电流电压间隔时间, 电流施加时间, 起始电流, 剩余电流, 电流波形, 直流叠加, 分断电流上限值, 分断时间上限值, 分断电流下限值, 分断时间下限值;

        public override void InitializeParams()
        {
            //数据库参数格式
            //电流电压间隔时间(S)=0.5|电流施加时间(S)=1|起始电流(mA)=0.1|剩余电流(mA)=30.0|分断电流上限值(mA)=30|分断时间上限值(mS)=30|分断电流下限值(mA)=15|分断时间下限值(mS)=0
            //电流波形=AC|直流叠加(mA)=不叠加

            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            电流电压间隔时间 = Convert.ToInt32(double.Parse(strParams[0].Split('=')[1]) * 1000);
            电流施加时间 = Convert.ToInt32(double.Parse(strParams[1].Split('=')[1]) * 1000);
            起始电流 = Convert.ToInt32(double.Parse(strParams[2].Split('=')[1]) * 10);
            剩余电流 = Convert.ToInt32(double.Parse(strParams[3].Split('=')[1]) * 10);
            分断电流上限值 = Convert.ToInt32(double.Parse(strParams[4].Split('=')[1]));
            分断时间上限值 = Convert.ToInt32(double.Parse(strParams[5].Split('=')[1]));
            if (strParams.Length > 6)
            {
                分断电流下限值 = Convert.ToInt32(double.Parse(strParams[6].Split('=')[1]));
                分断时间下限值 = Convert.ToInt32(double.Parse(strParams[7].Split('=')[1]));
            }

            string[] strParams1 = TrialItem.ItemParams.Split('|');
            电流波形 = GetCurrentWave(strParams1[0].Split('=')[1]);
            直流叠加 = GetDCAdd(strParams1[1].Split('=')[1]);
        }
        private int GetCurrentWave(string wave)
        {
            int Current;
            switch (wave)
            {
                case "AC":
                    Current = 1;
                    break;
                case "A0+":
                    Current = 2;
                    break;
                case "A90+":
                    Current = 3;
                    break;
                case "A135+":
                    Current = 4;
                    break;
                case "A0-":
                    Current = 5;
                    break;
                case "A90-":
                    Current = 6;
                    break;
                case "A135-":
                    Current = 7;
                    break;
                case "DC2P+":
                    Current = 8;
                    break;
                case "DC3P+":
                    Current = 9;
                    break;
                case "DCSM+":
                    Current = 10;
                    break;
                case "DC2P-":
                    Current = 11;
                    break;
                case "DC3P-":
                    Current = 12;
                    break;
                case "DCSM-":
                    Current = 13;
                    break;
                case "F":
                    Current = 14;
                    break;
                case "AC_150Hz":
                    Current = 15;
                    break;
                case "AC_400Hz":
                    Current = 16;
                    break;
                case "AC_1kHz":
                    Current = 17;
                    break;
                case "ICCPD":
                    Current = 18;
                    break;
                case "AC_60":
                    Current = 19;
                    break;
                default:
                    Current = 1;
                    break;
            }
            return Current;
        }

        private int GetDCAdd(string dcAdd)
        {
            int Current;
            switch (dcAdd)
            {
                case "不叠加":
                    Current = 1;
                    break;
                case "叠加正6mA":
                    Current = 2;
                    break;
                case "叠加负6mA":
                    Current = 3;
                    break;
                case "叠加正10mA":
                    Current = 4;
                    break;
                case "叠加负10mA":
                    Current = 5;
                    break;
                case "叠加正12mA":
                    Current = 6;
                    break;
                case "叠加负12mA":
                    Current = 7;
                    break;
                case "叠加正40mA":
                    Current = 8;
                    break;
                case "叠加负40mA":
                    Current = 9;
                    break;
                case "叠加正120mA":
                    Current = 10;
                    break;
                case "叠加负120mA":
                    Current = 11;
                    break;
                case "None":
                    Current = 1;
                    break;
                case "Add +6mA":
                    Current = 2;
                    break;
                case "Add -6mA":
                    Current = 3;
                    break;
                case "Add +10mA":
                    Current = 4;
                    break;
                case "Add -10mA":
                    Current = 5;
                    break;
                case "Add +12mA":
                    Current = 6;
                    break;
                case "Add -12mA":
                    Current = 7;
                    break;
                case "Add +40mA":
                    Current = 8;
                    break;
                case "Add -40mA":
                    Current = 9;
                    break;
                case "Add +120mA":
                    Current = 10;
                    break;
                case "Add -120mA":
                    Current = 11;
                    break;
                default:
                    Current = 1;
                    break;
            }
            return Current;
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



                    SetConditionValues();

                    int sleepTime = 50;
                    SendNoticeToUIAndTxtFile("设置漏电仪参数");
                    #region 设置参数
                    ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 32, 1);//断点  单断点
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 41, 2);//波形起
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 42, 电流电压间隔时间);//V/A间隔时间
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 33, 电流施加时间);//电流施加时间
                    Thread.Sleep(sleepTime);

                    ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 1, 起始电流);// 起始电流
                    Thread.Sleep(sleepTime);

                    ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 2, 剩余电流);// 剩余电流
                    Thread.Sleep(sleepTime);

                    ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 4, 电流波形);// 电流波形
                    Thread.Sleep(sleepTime);

                    ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 3, 3);// 电源电压  默认220V
                    Thread.Sleep(sleepTime);

                    ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 6, 1);// 施加位置 默认L1-N
                    Thread.Sleep(sleepTime);

                    ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 7, 直流叠加);// 直流叠加
                    Thread.Sleep(sleepTime);

                    ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 8, 1);// 试品类型  默认整机
                    Thread.Sleep(sleepTime);

                    #endregion
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    Thread.Sleep(3000); //等待电压稳定


                    if ((TrialType > 3200 && TrialType < 3300) || (TrialType >= 3401 && TrialType <= 3440))    //漏电分断时间
                    {
                        SendNoticeToUIAndTxtFile("施加漏电测脱扣时间");
                        ItemFlow = LanguageManager.GetByKey("测动作时间");
                        ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 34, 2);// 测试类型 S2突现时间   （S1突现时间为7）
                    }
                    else if ((TrialType > 3100 && TrialType < 3200) || (TrialType >= 3301 && TrialType <= 3340))
                    {
                        SendNoticeToUIAndTxtFile("施加漏电测脱扣电流");
                        ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 34, 1);// 测试类型 分断电流
                        ItemFlow = LanguageManager.GetByKey("测动作电流");
                    }
                    else if ((TrialType > 3501 && TrialType < 3600))
                    {
                        SendNoticeToUIAndTxtFile("施加闭合剩余电流");
                        ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 34, 3);// 测试类型 闭合时间
                        ItemFlow = "测动作时间";
                    }
                    Thread.Sleep(sleepTime);
                    ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 35, 1);// 启动测试
                    Thread.Sleep(电流施加时间 + 5000);

                    dicResult = ControlEquipMent.LeakageCurrent.LeakageCurrent_ReadData(testWorkParam.lstIDs, 15, 3);// 查询结果
                    ProcessData();

                    string Customer = ConfigurationManager.AppSettings["Customer"];
                    if (Customer != null && Customer.ToString().ToUpper().Equals("HYQCP"))
                    {
                        CountDownTimeInfo("请确认是否有告警提示。\r\n(注:勾选上为有告警)", 20, 2);
                        if (DicManualVerifyResult.First().Value)
                            ProcessDataResult(testWorkParam.lstIDs, "有告警", "锁止装置解锁", true);
                        else
                            ProcessDataResult(testWorkParam.lstIDs, "无告警", "锁止装置解锁", false);
                    }
                    ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 9, 0);// 停止电压
                    ControlEquipMent.LeakageCurrent.LeakageCurrent_SetParams(testWorkParam.lstIDs, 10, 0);// 停止电流
                    //断电恢复故障
                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(2000);


                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
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
                double current = 0;
                if (dicResult[item] == null || dicResult[item].ToString().Contains("超时"))
                {
                    current = -9999;
                }
                else
                {
                    current = double.Parse(dicResult[item].Trim(new char[] { 'm', 'M', 'S', 's', 'a', 'A' }));
                }

                if (/*voltage > LstChargerInfo[0].NominalVoltage - 10 && voltage < LstChargerInfo[0].NominalVoltage + 10 &&*/ current < 0)
                {
                    //电压未断开，能充电，FAIL
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    if ((TrialType > 3200 && TrialType < 3300) || (TrialType >= 3401 && TrialType <= 3440))    //漏电分断时间
                    {
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|" + ItemFlow + "|" + 分断时间下限值 + "|" + 分断时间上限值 + " |" + LanguageManager.GetByKey("未脱扣");
                    }
                    else if ((TrialType > 3100 && TrialType < 3200) || (TrialType >= 3301 && TrialType <= 3340))    //漏电分断电流
                    {
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|" + ItemFlow + "|" + 分断电流下限值 + "|" + 分断电流上限值 + " |" + LanguageManager.GetByKey("未脱扣");
                    }
                    else if ((TrialType > 3501 && TrialType < 3600))    //闭合时间
                    {
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|" + ItemFlow + "|" + 分断时间下限值 + "|" + 分断时间上限值 + " |" + LanguageManager.GetByKey("未脱扣");
                    }
                }
                else
                {
                    if ((current > 分断电流上限值 && TrialType > 3100 && TrialType < 3200) || (current > 分断时间上限值 && TrialType > 3200 && TrialType < 3300)
                     || (current > 分断电流上限值 && TrialType >= 3301 && TrialType <= 3340) || (current > 分断时间上限值 && TrialType >= 3401 && TrialType <= 3440)
                     || (current > 分断时间上限值 && TrialType > 3501 && TrialType < 3600))
                    {
                        //测试电流大于"测试电流上限值(mA)"  不合格
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    }
                    if ((TrialType > 3200 && TrialType < 3300) || (TrialType >= 3401 && TrialType <= 3440))    //漏电分断时间
                    {
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|" + ItemFlow + "|" + 分断时间下限值 + "|" + 分断时间上限值 + " |" + current.ToString();
                    }
                    else if ((TrialType > 3100 && TrialType < 3200) || (TrialType >= 3301 && TrialType <= 3340))    //漏电分断电流
                    {
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|" + ItemFlow + "|" + 分断电流下限值 + "|" + 分断电流上限值 + " |" + current.ToString();
                    }
                    else if ((TrialType > 3501 && TrialType < 3600))    //闭合时间
                    {
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|" + ItemFlow + "|" + 分断时间下限值 + "|" + 分断时间上限值 + " |" + current.ToString();
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
