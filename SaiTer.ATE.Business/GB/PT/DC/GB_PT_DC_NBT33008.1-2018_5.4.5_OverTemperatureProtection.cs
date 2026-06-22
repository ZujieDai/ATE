using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标产测直流：过温保护试验
    /// </summary>
    public class GB_PT_DC_OverTemperatureProtection : BusinessBase
    {
        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;


        public GB_PT_DC_OverTemperatureProtection(int trialType) { TrialType = trialType; }
        public override void InitializeParams()
        {
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
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

        /// <summary>
        /// 测试流程
        /// </summary>
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

                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }

                //设置测试条件
                SetConditionValues();

                SendNoticeToUIAndTxtFile("设备开启负载中，请稍候...");
                SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, ResiLoadCurrent, BMSDemandVolt - 5, ResiLoadCurrent);
                Thread.Sleep(1000);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrent(testWorkParam.lstIDs, ResiLoadCurrent);
                Thread.Sleep(1000);
                d3 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    var data = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value.Channel4Power;
                    int timeout = 50;
                    while (timeout-- > 0)
                    {
                        if (data < ResiLoadCurrent * MaxAllowChargeVoltage / 1000 * 0.8 || data > ResiLoadCurrent * MaxAllowChargeVoltage / 1000 * 1.2)
                        {
                            data = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value.Channel4Power;
                            Thread.Sleep(100);
                            continue;
                        }
                        break;
                    }
                    d3.Add(item, data.ToString("F2"));
                }
                ProcessDataTmp(d3, "正常充电状态", "输出功率值(kW)", "-", "-");

                CountDownTimeInfo("请人工模拟内部温度超过过温保护值", 999, 0);
                SetLoadDCOFF(testWorkParam.lstIDs);
                Thread.Sleep(10 * 1000);

                d3 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    var data = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value.Channel4Power;
                    int timeout = 30;
                    while (timeout-- > 0)
                    {
                        if (data > ResiLoadCurrent * 200 / 1000 * 0.8 || data < 0)
                        {
                            data = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value.Channel4Power;
                            Thread.Sleep(100);
                            continue;
                        }
                        break;
                    }
                    d3.Add(item, data.ToString("F2"));
                }
                ProcessDataTmp(d3, "过温保护状态", "输出功率值(kW)", "-", "-");
                CountDownTimeInfo("请检查充电机应降低输出功率或切断直流输出，并发出告警提示。\r\n（勾选枪号则为Pass）", 999, 2);
                //ProcessDataConnect("应发出告警提示", "是否有告警提示");
                d1 = new Dictionary<int, string>();
                var dicResult = new Dictionary<int, EmTrialResult>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, DicManualVerifyResult[item] ? "是" : "否");
                    dicResult.Add(item, DicManualVerifyResult[item] ? EmTrialResult.Pass : EmTrialResult.Fail);
                }
                ProcessDataResults(testWorkParam.lstIDs, d1, "是否有告警提示", dicResult, "应发出告警提示");
                ProcessData();
                CountDownTimeInfo("请人工恢复充电机内部温度到正常范围内。", 999, 0);
            }
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
                LstTrialData[k].ItemName = iIndex.ToString();


                //double voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage;
                //if (voltage >= 0 && voltage <= 20)
                if (DicManualVerifyResult[item])
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    LstTrialData[k].ExtentData = $"过温保护状态|是否降低输出功率或切断直流输出|-|-|是";
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    LstTrialData[k].ExtentData = $"过温保护状态|是否降低输出功率或切断直流输出|-|-|否";
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
                iIndex++;
            }
        }
    }
}
