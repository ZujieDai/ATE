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
    /// <summary>
    /// 灯语测试
    /// </summary>
    public class CZ_TB_LampSignal : BusinessBase
    {
        string itemFlow = "";
        public CZ_TB_LampSignal(int type) { TrialType = type; }

        int CheckTime = 10;//人工检测时间（秒）

        public override void InitializeParams()
        {
            string[] strParams = TrialItem.ResultParams.Split('|');
            CheckTime = Convert.ToInt32(double.Parse(strParams[0].Split('=')[1]));
        }

        public override void InitEquiMent()
        {

        }

        public override void ExecuteMethod()
        {
            try
            {
                Init();
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
                //SetCPRersh_EUDC();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }

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
                    if (testWorkParam.lstIDs.Count == 0)
                    {
                        return;
                    }


                    List<string> list = new List<string>()
                    {
                        "待机",
                        "插枪",
                        "充电",
                        "充电完成",
                        "故障",
                        "启动失败"
                    };
                    //设置测试条件
                    SetConditionValues();
                    string sTmpName = "外观检查";
                    List<string> infos = new List<string>()
                    {
                        "1个红灯常亮，4个绿灯常灭",
                        "1个红灯常灭，4个绿灯闪烁",
                        "1个红灯常灭，4个绿灯根据SOC\r\n0%~25%，一个闪烁，其他全灭\r\n26%~50%，一个闪烁，一个常亮，其他全灭\r\n51%~75%，一个闪烁，两个常亮，其他全灭\r\n76%~100%，一个闪烁，三个常亮",
                        "1个红灯常灭，4个绿灯常亮",
                        "1个红灯闪烁，4个绿灯常灭",
                        "1个红灯闪烁，4个绿灯常亮"
                    };


                    var ks = GetKStatus16_Charging_EU_DC();
                    double acVolt = AllEquipStateData.DicACSource_StateData[testWorkParam.lstIDs[0]].Volt;
                    //提示人工确认项
                    for (int i = 0; i < list.Count; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                //断开CP
                                ks[2] = false;
                                ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, 390, ks.ToArray(), 0, 0);
                                break;
                            case 1:
                                ks[2] = true;
                                ControlEquipMent.BMS.BMSSetKState_EU_DC(testWorkParam.lstIDs, 390, ks.ToArray(), 0, 0);
                                break;
                            case 2:
                                CheckSwipingCard(testWorkParam.lstIDs);
                                break;
                            case 3:
                                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                                Thread.Sleep(2000);
                                break;
                            case 4:
                                CountDownTimeInfo("请将充电桩设置为故障状态，如按下急停", 999, 2);
                                break;
                            case 5:
                                SetACSource(testWorkParam.lstIDs, acVolt * 1.2);
                                Thread.Sleep(2000);
                                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                                MessgaeInfo(true, "请刷卡充电!");
                                int timeout = 60 * 5;
                                while (timeout-- > 0)
                                {
                                    int state = ChangeBMSChargeStatus_EU_DC(AllEquipStateData.DicBMS_EU_DC_StateData[LstChargerInfo[0].ChargerId].SystemState);
                                    if (state >= 2 /*&& state <= 7*/)
                                    {
                                        break;
                                    }
                                    if (timeout % 5 == 0)
                                    {
                                        int residuetime = timeout / 5;
                                        SendNoticeToUIAndTxtFile("刷卡剩余倒计时:" + residuetime);
                                    }
                                    System.Threading.Thread.Sleep(200);
                                }
                                MessgaeInfo(false, "请刷卡充电!");
                                break;
                        }
                        sTmpName = list[i];
                        string info = $"当前状态为【{sTmpName}】\r\n请确认是否符合【{infos[i]}】\r\n注：勾选上为合格";
                        CountDownTimeInfo(info, CheckTime, 2);
                        ProcessDataResult(testWorkParam.lstIDs, "-", "-", DicManualVerifyResult.First().Value, sTmpName);
                        if (i == 4)
                        {
                            CountDownTimeInfo("请将充电桩恢复正常", 999, 2);
                            //故障恢复
                            SendNoticeToUIAndTxtFile("开始重启交流源恢复故障");
                            ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                            //这个时间不能缩短
                            Thread.Sleep(7000);
                            ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                        }
                        else if (i == 5)
                            SetACSource(testWorkParam.lstIDs, acVolt);
                    }

                    //ProcessData();
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {

        }

    }
}
