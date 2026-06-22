using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 欧标研测直流：额定输出功率和最大输出功率
    /// </summary>
    public class CCS2_RT_DC_RatedAndMaximumOutputPowerTest : BusinessBase
    {
        private int TestTime = 0;
        private int trlTimeOut_S = 5;
        private double ErrorRate = 0.05;

        public CCS2_RT_DC_RatedAndMaximumOutputPowerTest(int type)
        {
            TrialType = type;
        }

        public override void InitEquiMent()
        {
        }

        public override void InitializeParams()
        {
            string[] strParams = TrialItem.ResultParams.Split('|');

            //判断误差(%)=5
            if (strParams.Length >= 1 && strParams[0].Split('=').Length > 1)
            {
                //BMSVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                //BMSCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
                //BMSVoltage2 = Convert.ToDouble(strParams[2].Split('=')[1]);
                //BMSCurrent2 = Convert.ToDouble(strParams[3].Split('=')[1]);
                //ErrorVoltageRate = Convert.ToDouble(strParams[4].Split('=')[1]) / 100;
                //ErrorCurrentRate = Convert.ToDouble(strParams[5].Split('=')[1]) / 100;
                ErrorRate = Convert.ToDouble(strParams[0].Split('=')[1]) / 100;
            }
        }

        public override void ExecuteMethod()
        {
            try
            {
                Init();
                SetCPRersh_EUDCALL();
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
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
        }

        private void StartItemFlow()
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
                                    //
                                    LstTrialData[i].ExtentData = "-|-|-|-|null";
                                    SendTrialDataToUI(LstTrialData[i]);
                                }
                            }
                        }
                        break;
                    }
                    if (testWorkParam.lstIDs.Count <= 0)
                    {
                        return;
                    }
                    SetConditionValues();

                    if (!CheckSwipingCard(testWorkParam.lstIDs, MaxAllowChargeVoltage, RatedCurrent + 10))
                    {
                        return;
                    }
                    Thread.Sleep(2000);

                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, MaxAllowChargeVoltage - 20, RatedCurrent, MaxAllowChargeVoltage, RatedCurrent);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("等待负载电流稳定中");
                    WaitDCCurrent_EU_DC(testWorkParam.lstIDs, RatedCurrent);
                    Thread.Sleep(5000);//等待回馈负载电流稳定


                    SendNoticeToUIAndTxtFile("判断结果中");
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    Dictionary<int, string> dicP = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4RMSCurrent;
                        dic.Add(item, DCCurrent.ToString("F2"));
                        double DCPower = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4Power;
                        dicP.Add(item, DCPower.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "额定输出和最大输出功率", "充电电流(A)", (RatedCurrent * (1 - ErrorRate)).ToString(), (RatedCurrent * (1 + ErrorRate)).ToString());
                    ProcessDataTmp(dicP, "额定输出和最大输出功率", "输出功率(kW)", (MaxOutputPower * (1 - ErrorRate)).ToString(), (MaxOutputPower * (1 + ErrorRate)).ToString());


                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    SetLoadDCOFF(testWorkParam.lstIDs);

                    //SendNoticeToUIAndTxtFile("关闭BMS中...");
                    //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                }
            }
            catch (Exception ex) { SendException(ex); }


        }

        public override void ProcessData()
        {

        }
    }
}
