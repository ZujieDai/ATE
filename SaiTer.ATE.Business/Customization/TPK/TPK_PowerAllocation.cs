using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SaiTer.ATE.DataModel.ACTest;

namespace SaiTer.ATE.Business
{

    public class TPK_PowerAllocation : BusinessBase     //功率分配测试
    {
        int trlTimeOut_S = 30;
        int ChargingWaitTime = 120; //充电等待时间，单位秒
        double ChargingVolt = 750;
        double ChargingCurr = 10;
        double ChargingPower_Avg = 60;//单枪均分功率kW
        public TPK_PowerAllocation(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            //   充电电压(V)=750|充电电流(A)=10|充电等待时间(s)=120
            string[] strParams = TrialItem.ResultParams.Split('|');
            ChargingVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            ChargingPower_Avg = Convert.ToDouble(strParams[1].Split('=')[1]);

            if (strParams.Length > 0 && strParams[2].Split('=').Length > 1)
            {
                ChargingWaitTime = (int)Convert.ToDouble(strParams[2].Split('=')[1]);
            }
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
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "关闭BMS");
                SetLoadDCOFF(lstIDs);
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "关闭负载");
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //保存试验结果               
                SaveTrialResult();
                //发送试验结束刷新UI
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

                    //设置测试条件
                    SetConditionValues();

                    ChargingMethon(1);//先测试第一把枪
                    ChargingMethon(2);
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }
        public override void ProcessData()
        {

        }

        private void ChargingMethon(int iType)
        {
            List<int> lstTestIDs1 = new List<int>();
            List<int> lstTestIDs2 = new List<int>();
            string sState = "双枪功率分配";
            double BMSDemandCurrent = 10;
            foreach (int i in testWorkParam.lstIDs)
            {
                if (iType == 1)
                {
                    if (i % 2 == 0)//偶数,每组的第一个桩
                    {
                        lstTestIDs1.Add(i);
                    }
                    else
                    {
                        lstTestIDs2.Add(i);
                    }
                }
                else
                {
                    if (i % 2 == 0)//偶数,每组的第一个桩
                    {
                        lstTestIDs2.Add(i);
                    }
                    else
                    {
                        lstTestIDs1.Add(i);
                    }
                }
            }
            double TestCurrent = (ChargingPower_Avg * 1.5 * 1000) / ChargingVolt;//测试电流
            TestCurrent = TestCurrent > MaxAllowChargeCurrent ? MaxAllowChargeCurrent : TestCurrent;

            if (!CheckSwipingCard(testWorkParam.lstIDs, ChargingVolt, ChargingCurr))
            {
                return;
            }

            ControlEquipMent.BMS.SetParameter(lstTestIDs2, ChargingVolt, 10 + 5, true, ChargingVolt);//不是主要测试的枪先设置需求
            Thread.Sleep(300);
            ControlEquipMent.BMS.SetParameter(lstTestIDs1, ChargingVolt, TestCurrent + 5, true, ChargingVolt);
            Thread.Sleep(300);

            WaitDCVoltage(testWorkParam.lstIDs, ChargingVolt);
            SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");

            SetLoadPara(lstTestIDs2, ChargingVolt - 10, 10, ChargingVolt, 10);
            SendNoticeToUIAndTxtFile($"设置枪{lstTestIDs2.First()}电压{ChargingVolt}V,带载电流10A，等待负载稳定");
            Thread.Sleep(300);
            SetLoadPara(lstTestIDs1, ChargingVolt - 10, TestCurrent, ChargingVolt, TestCurrent);
            Thread.Sleep(300);
            SendNoticeToUIAndTxtFile($"设置枪{lstTestIDs1.First()}电压{ChargingVolt}V,带载电流{TestCurrent}A，等待负载稳定");

            Thread.Sleep(1000);
            SetLoadDCON(testWorkParam.lstIDs);
            WaitDCCurrent(lstTestIDs1, TestCurrent);//主要测试枪号能够带载成功即可

            //测试时间两分钟
            SendNoticeToUIAndTxtFile($"开始充电，测试时间{ChargingWaitTime}秒...");
            Thread.Sleep(1000 * ChargingWaitTime);

            Dictionary<int, string> dic = new Dictionary<int, string>();
            dic.Clear();
            foreach (var item in lstTestIDs1)
            {
                double current = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                dic.Add(item, current.ToString("F2"));
            }
            ProcessDataTmp(dic, sState+ $"枪{lstTestIDs1.First()}", "充电电流(A)", (TestCurrent * 0.9).ToString("F2"), (TestCurrent * 1.1).ToString("F2"));

            dic.Clear();
            foreach (var item in lstTestIDs2)
            {
                double current = AllEquipStateData.DicBMS_DC_StateData[item].ChargingCurrent;
                dic.Add(item, current.ToString("F2"));
            }
            ProcessDataTmp(dic, sState + $"枪{lstTestIDs2.First()}", "充电电流(A)", (10 * 0.8).ToString("F2"), (10 * 1.2).ToString("F2"));

            dic.Clear();
            foreach (var item in testWorkParam.lstIDs)
            {
                double voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                dic.Add(item, voltage.ToString("F2"));
            }
            ProcessDataTmp(dic, sState, "充电电压(V)", (ChargingVolt * 0.9).ToString("F2"), (ChargingVolt * 1.1).ToString("F2"));

            //SetLoadDCOFF(testWorkParam.lstIDs);
            //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            Thread.Sleep(300);
        }
    }
}
