using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public class GB_GC_DC_PowerDistributeDoubleCharger : BusinessBase
    {
        int trlTimeOut_S = 30;
        int ChargingWaitTime = 120;
        public GB_GC_DC_PowerDistributeDoubleCharger(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            string[] strParams = TrialItem.ResultParams.Split('|');
            //充电等待时间(s)=120
            if (strParams.Length > 0 && strParams[0].Split('=').Length > 1)
            {
                ChargingWaitTime = (int)Convert.ToDouble(strParams[0].Split('=')[1]);
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
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
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

                    foreach (int cNum in testWorkParam.lstIDs)
                    {
                        //群充可能是一个终端一个条码两把枪
                        string strIsGroupC = ConfigurationManager.AppSettings["isGroupCharger"];
                        if (strIsGroupC != null)
                        {
                            bool isGroupCharger = Convert.ToBoolean(strIsGroupC);
                            if (isGroupCharger)
                            {
                                var chargerInfo = LstChargerInfo.Find(c => c.ChargerId == cNum);
                                //一个条码对应两把枪
                                if (chargerInfo.RES2 == "1")
                                {
                                    if (cNum % 2 == 1)
                                    {
                                        ChargingMethon(new List<int>() { cNum, cNum + 1 }, $"终端{(cNum - 1) / 2 + 1}");
                                    }
                                    //else
                                    //    continue;
                                }
                                //一个条码对应一把枪
                                else
                                {
                                    //单枪不测试
                                }
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
        public override void ProcessData()
        {

        }

        private void ChargingMethon(List<int> lstTestIDs, string sState)
        {
            if (!CheckSwipingCard(lstTestIDs, MaxAllowChargeVoltage, MaxAllowChargeCurrent))
            {
                return;
            }

            //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, MaxAllowChargeCurrent, true, BMSDemandVolt);
            WaitDCVoltage(lstTestIDs, MaxAllowChargeVoltage);
            SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
            SetLoadPara(new List<int>() { lstTestIDs[0] }, MaxAllowChargeVoltage - 20, RatedCurrent, MaxAllowChargeVoltage, RatedCurrent);
            SetLoadPara(new List<int>() { lstTestIDs[1] }, MaxAllowChargeVoltage - 20, RatedCurrent / 2.0, MaxAllowChargeVoltage, RatedCurrent / 2.0);
            Thread.Sleep(1000);
            SetLoadDCON(lstTestIDs);
            WaitDCCurrent(new List<int>() { lstTestIDs[0] }, RatedCurrent);
            WaitDCCurrent(new List<int>() { lstTestIDs[1] }, RatedCurrent / 2.0);

            //测试时间两分钟
            SendNoticeToUIAndTxtFile($"开始充电，测试时间{ChargingWaitTime / 60.0}min...");
            Thread.Sleep(1000 * ChargingWaitTime);

            Dictionary<int, string> dic = new Dictionary<int, string>();
            double power = AllEquipStateData.DicBMS_DC_StateData[lstTestIDs[0]].ChargingVoltage * AllEquipStateData.DicBMS_DC_StateData[lstTestIDs[0]].ChargingCurrent / 1000.0;
            dic.Add(lstTestIDs[0], power.ToString("F2"));
            ProcessDataTmp(dic, sState + " A枪均分功率", "充电功率(kW)", (MaxOutputPower * 0.9).ToString("F2"), (MaxOutputPower * 1.1).ToString("F2"));

            dic = new Dictionary<int, string>();
            power = AllEquipStateData.DicBMS_DC_StateData[lstTestIDs[1]].ChargingVoltage * AllEquipStateData.DicBMS_DC_StateData[lstTestIDs[1]].ChargingCurrent / 1000.0;
            dic.Add(lstTestIDs[1], power.ToString("F2"));
            ProcessDataTmp(dic, sState + " B枪小功率", "充电功率(kW)", "-", "-");

            SendNoticeToUIAndTxtFile($"调整单枪超过均分功率，测试时间{ChargingWaitTime / 60.0}min...");
            SetLoadPara(new List<int>() { lstTestIDs[0] }, MaxAllowChargeVoltage - 20, RatedCurrent + 20, MaxAllowChargeVoltage, RatedCurrent + 20);
            Thread.Sleep(1000 * ChargingWaitTime);

            dic = new Dictionary<int, string>();
            power = AllEquipStateData.DicBMS_DC_StateData[lstTestIDs[0]].ChargingVoltage * AllEquipStateData.DicBMS_DC_StateData[lstTestIDs[0]].ChargingCurrent / 1000.0;
            dic.Add(lstTestIDs[0], power.ToString("F2"));
            ProcessDataTmp(dic, sState + " A枪超均分功率", "充电功率(kW)", (MaxAllowChargeVoltage * (RatedCurrent + 20) * 0.9 / 1000.0).ToString("F2"), (MaxAllowChargeVoltage * (RatedCurrent + 20) * 1.1 / 1000.0).ToString("F2"));


            SetLoadDCOFF(lstTestIDs);


            SetLoadPara(new List<int>() { lstTestIDs[1] }, MaxAllowChargeVoltage - 20, RatedCurrent, MaxAllowChargeVoltage, RatedCurrent);
            SetLoadPara(new List<int>() { lstTestIDs[0] }, MaxAllowChargeVoltage - 20, RatedCurrent / 2.0, MaxAllowChargeVoltage, RatedCurrent / 2.0);
            Thread.Sleep(1000);
            SetLoadDCON(lstTestIDs);
            WaitDCCurrent(new List<int>() { lstTestIDs[1] }, RatedCurrent);
            WaitDCCurrent(new List<int>() { lstTestIDs[0] }, RatedCurrent / 2.0);

            //测试时间两分钟
            SendNoticeToUIAndTxtFile($"修改充电参数，测试时间{ChargingWaitTime / 60.0}min...");
            Thread.Sleep(1000 * ChargingWaitTime);

            dic = new Dictionary<int, string>();
            power = AllEquipStateData.DicBMS_DC_StateData[lstTestIDs[1]].ChargingVoltage * AllEquipStateData.DicBMS_DC_StateData[lstTestIDs[1]].ChargingCurrent / 1000.0;
            dic.Add(lstTestIDs[1], power.ToString("F2"));
            ProcessDataTmp(dic, sState + " B枪均分功率", "充电功率(kW)", (MaxOutputPower * 0.9).ToString("F2"), (MaxOutputPower * 1.1).ToString("F2"));

            dic = new Dictionary<int, string>();
            power = AllEquipStateData.DicBMS_DC_StateData[lstTestIDs[0]].ChargingVoltage * AllEquipStateData.DicBMS_DC_StateData[lstTestIDs[0]].ChargingCurrent / 1000.0;
            dic.Add(lstTestIDs[0], power.ToString("F2"));
            ProcessDataTmp(dic, sState + " A枪小功率", "充电功率(kW)", "-", "-");

            SendNoticeToUIAndTxtFile($"调整单枪超过均分功率，测试时间{ChargingWaitTime / 60.0}min...");
            SetLoadPara(new List<int>() { lstTestIDs[1] }, MaxAllowChargeVoltage - 20, RatedCurrent + 20, MaxAllowChargeVoltage, RatedCurrent + 20);
            Thread.Sleep(1000 * ChargingWaitTime);

            dic = new Dictionary<int, string>();
            power = AllEquipStateData.DicBMS_DC_StateData[lstTestIDs[1]].ChargingVoltage * AllEquipStateData.DicBMS_DC_StateData[lstTestIDs[1]].ChargingCurrent / 1000.0;
            dic.Add(lstTestIDs[1], power.ToString("F2"));
            ProcessDataTmp(dic, sState + " B枪超均分功率", "充电功率(kW)", (MaxAllowChargeVoltage * (RatedCurrent + 20) * 0.9 / 1000.0).ToString("F2"), (MaxAllowChargeVoltage * (RatedCurrent + 20 / 1000.0) * 1.1).ToString("F2"));

            SetLoadDCOFF(lstTestIDs);
            ControlEquipMent.BMS.BMS_OFF(lstTestIDs);
            Thread.Sleep(300);
        }
    }
}
