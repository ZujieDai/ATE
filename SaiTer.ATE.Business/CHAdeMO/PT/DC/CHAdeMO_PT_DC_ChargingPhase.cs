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
    /// 充电阶段测试
    /// </summary>
    public class CHAdeMO_PT_DC_ChargingPhase : BusinessBase
    {
        public CHAdeMO_PT_DC_ChargingPhase(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;
        private double BMSMeasureVoltage = 390;//过压参考值
        private int trlTimeOut_S = 5;
        Double CPVoltageMax = 10;
        Double CPVoltageMin = 8;

        double BMSDemandVoltage = 0;
        double BMSDemandCurrent = 0;
        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVoltage = LstChargerInfo[0].NominalVoltage;
            //BMSDemandCurrent = LstChargerInfo[0].NominalCurrent;
            BMSDemandCurrent = 20;

            if (strParams.Length >= 2)
            {
                BMSDemandVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                BMSDemandCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            }
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


                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    SendNoticeToUIAndTxtFile(string.Format("设置导引需求电压为{0}，需求电流为{1}", BMSDemandVoltage, BMSDemandCurrent));
                    ControlEquipMent.BMS.BMSSetParameter_JP_DC(testWorkParam.lstIDs, BMSDemandVoltage, BMSDemandCurrent, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(100);

                    System.Threading.Thread.Sleep(5000);//等待电压稳定

                    //带载
                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVoltage - 10, BMSDemandCurrent + 10, BMSDemandVoltage, BMSDemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime_JP_DC(testWorkParam.lstIDs, BMSDemandCurrent, 35);

                    System.Threading.Thread.Sleep(2000);//等待电流稳定

                    d1 = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_JP_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "充电阶段测试", "输出电压(V)", (BMSDemandVoltage*0.9).ToString(), (BMSDemandVoltage * 1.1).ToString());

                    d1 = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_JP_DC_StateData[testWorkParam.lstIDs[i]].ChargingCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "充电阶段测试", "输出电流(A)", (BMSDemandCurrent * 0.9).ToString(), (BMSDemandCurrent * 1.1).ToString());

                    SetLoadDCOFF(testWorkParam.lstIDs);

                }
            }
            catch (Exception ex) { SendException(ex); }
        }

        public override void ProcessData()
        {

        }
    }
}
