using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标产测直流：充电模式和连接方式检查（人工）
    /// </summary>
    public class GB_PT_DC_ChargerModeAndConnectWay : BusinessBase
    {
        string itemFlow = "";
        public GB_PT_DC_ChargerModeAndConnectWay(int type) { TrialType = type; }

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
                SetCPReresh();
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

                    //设置测试条件
                    SetConditionValues();

                    itemFlow = "充电模式4";
                    string info = $"【{itemFlow}】充电机采用的充电模式应符合GB/T 18487.1-2015中5.1规定的充电模式4，用于电动汽车连接到直流供电设备的情况，应用于永久连接在电网(电源)的设备和通过电缆与电网(电源)连接为其供电的设备.\r\n(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessDataResult(testWorkParam.lstIDs, "-", itemFlow, DicManualVerifyResult.First().Value, "充电模式检查");

                    itemFlow = "连接方式C";
                    info = $"【{itemFlow}】请确认连接方式应符合GB/T 18487.1-2015中3.1.3.3规定的连接方式C，将电动汽车和交流电网连接时，使用了和供电设备永久连接在一起的充电电缆和车辆插头.\r\n(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessDataResult(testWorkParam.lstIDs, "-", itemFlow, DicManualVerifyResult.First().Value, "连接方式检查");
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
        }

        public override void ProcessData()
        {

        }

    }
}
