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

namespace SaiTer.ATE.Business
{



    public class TPK_InsulationDetection : BusinessBase
    {

        public TPK_InsulationDetection(int type)
        {
            TrialType = type;
        }
        private double ChargingVolt, ChargingCurr, ChargingTime;
        int Gear = 0;

        public override void InitializeParams()
        {
            Init();

            //充电电压(V)=750|充电电流(A)=100|充电时间(分)=2|绝缘电阻档位=0
            try
            {
                string[] strParams = TrialItem.ResultParams.Split('|');
                ChargingVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
                ChargingCurr = Convert.ToDouble(strParams[1].Split('=')[1]);
                ChargingTime = Convert.ToDouble(strParams[2].Split('=')[1]);
                Gear = (int)Convert.ToDouble(strParams[3].Split('=')[1]);

                if (Gear>1)  //只有1，2 小于50K 有效
                {
                    Gear = 0;  //  2,3,4,5,6  时默认为 15.3K 
                }
            }
            catch
            {

            }

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
                var kstate = GetKStatus16_Charging_DC();
                ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, kstate.ToArray());
                Thread.Sleep(300);
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "关闭BMS");
                SetLoadDCOFF(lstIDs);
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "关闭负载");
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
            }
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
                                //CP占空比测量值1(%)|CP占空比测量值2(%)|CP占空比测量值3(%)|CP占空比下限|CP占空比上限|测试结果|查看示波器截图
                                //LstTrialData[i].ExtentData = "null|null|null|" + PwmMin + "|" + PwmMax;
                                LstTrialData[i].ExtentData = "-|-|-|-|-";

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


                int KStatus1 = 0, KStatus2 = 0;
                string resistanceVal = "";
                List<bool> lstKState = GetKStatus16_Charging_DC();
                //绝缘检测  刷卡前  设置  15.3K 

                switch (Gear)
                {
                    case 0: //15.3KΩ
                        KStatus1 = 1;
                        KStatus2 = 9;
                        resistanceVal = "15.3KΩ";
                        break;
                    case 1: //20KΩ

                        KStatus1 = 2;
                        KStatus2 = 10;
                        resistanceVal = "20KΩ";

                        break;
                    case 2: //75KΩ
                        KStatus1 = 3;
                        KStatus2 = 11;
                        resistanceVal = "75KΩ";

                        break;
                    case 3: //100KΩ
                        KStatus1 = 4;
                        KStatus2 = 12;
                        resistanceVal = "100KΩ";

                        break;
                    case 4: //200KΩ
                        KStatus1 = 5;
                        KStatus2 = 13;
                        resistanceVal = "200KΩ";

                        break;
                    case 5: //300KΩ
                        KStatus1 = 6;
                        KStatus2 = 14;
                        resistanceVal = "300KΩ";

                        break;
                    case 6: //600KΩ
                        KStatus1 = 7;
                        KStatus2 = 15;
                        resistanceVal = "600KΩ";

                        break;
                    default:


                        KStatus1 = 1;
                        KStatus2 = 9;
                        resistanceVal = "15.3KΩ";
                        break;


                }

                lstKState[0] = false;
                lstKState[KStatus1] = true;//DC+非对称漏电15.3K
                lstKState[8] = false;
                lstKState[KStatus2] = true;//DC-非对称漏电15.3K

                SendNoticeToUIAndTxtFile($"DC+、DC-对称漏电{resistanceVal}");
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, lstKState.ToArray());


                //  ChargingVolt = ChargingVolt + 20 >= MaxAllowChargeVoltage ? MaxAllowChargeVoltage : ChargingVolt + 20;
                //if (!CheckSwipingCard(testWorkParam.lstIDs, ChargingVolt, ChargingCurr, MaxAllowChargeVoltage, false)) //刷卡
                //{
                //    return;
                //}


                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, ChargingVolt, ChargingCurr + 10, true, ChargingVolt);   //需求BMS充电参数设置
                Thread.Sleep(500);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                CountDownTimeInfo("请刷卡充电,刷卡后点确定！", 99, 2);

                SendNoticeToUIAndTxtFile($"DC+、DC-对称漏电{resistanceVal}，请检查充电桩是否有绝缘告警？");
                Thread.Sleep(1000 * 8);
                //  SendNoticeToUIAndTxtFile(string.Format("设置BMS电压{0}V,带载电流{1}A，等待负载稳定", ChargingVolt, ChargingCurr));
                //if (ChargingVolt == MaxAllowChargeVoltage)   //不带载
                //    SetLoadPara(testWorkParam.lstIDs, ChargingVolt - 10, ChargingCurr, ChargingVolt - 5, ChargingCurr);  // 原AgingCurr + 20
                //else
                //    SetLoadPara(testWorkParam.lstIDs, ChargingVolt - 20, ChargingCurr, ChargingVolt - 20, ChargingCurr); // 原AgingCurr + 20
                //Thread.Sleep(1000);
                //SetLoadDCON(testWorkParam.lstIDs);
                //WaitDCCurrentWithTime(testWorkParam.lstIDs, ChargingCurr, 40);
                //Thread.Sleep(1000 * 5);


                Dictionary<int, string> dicAgingVolt = new Dictionary<int, string>();
                //Dictionary<int, string> dicAgingCurr = new Dictionary<int, string>();
                //foreach (var itmp in testWorkParam.lstIDs)
                //{
                //    double voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                //    if (voltage == 0)
                //    {
                //        Thread.Sleep(500);
                //        voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                //    }
                //    dicAgingVolt.Add(itmp, voltage.ToString("F2"));

                //    double current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;
                //    if (current == 0)
                //    {
                //        Thread.Sleep(500);
                //        current = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingCurrent;
                //    }
                //    dicAgingCurr.Add(itmp, current.ToString("F2"));
                //}
                //  ProcessDataTmp(dicAgingVolt, string.Format("设定电压{0}V", ChargingVolt), "桩实际电压(V)", (ChargingVolt * 0.9).ToString(), (ChargingVolt * 1.1).ToString());
                // ProcessDataTmp(dicAgingCurr, string.Format("设定电流{0}A", ChargingCurr), "桩实际电流(V)", (ChargingCurr *0.9).ToString(), ( ChargingCurr*1.1).ToString());



                CountDownTimeInfo("请确认桩是否绝缘检测告警？\r\n注：勾选上为合格。", 99, 2);
                ProcessDataResults(testWorkParam.lstIDs, "-", "-", DicManualVerifyResult, "绝缘检测");

                // DicManualVerifyResult[item] ? "有告警" : "未告警")

                //dicAgingVolt.Clear();

                //foreach (var itmp in testWorkParam.lstIDs)
                //{
                //    double voltage = AllEquipStateData.DicBMS_DC_StateData[itmp].ChargingVoltage;
                //    dicAgingVolt.Add(itmp, voltage.ToString("F2"));
                //}

                //ProcessDataTmp(dicAgingVolt, "绝缘检测", "桩实际电压(V)", "0", "60");

                Dictionary<int, string> dicVolt = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    int timeout = 50;
                    double voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                    while (timeout-- > 0)
                    {
                        if (voltage > 20)
                        {
                            voltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                            Thread.Sleep(100);
                        }
                    }
                    dicVolt.Add(item, voltage.ToString("F2"));
                }


                ProcessDataTmp(dicVolt, "绝缘检测", "直流输出电压(V)", "0", "60");


                Dictionary<int, string> dd1 = new Dictionary<int, string>();
                Dictionary<int, string> dd2 = new Dictionary<int, string>();
                Dictionary<int, EmTrialResult> dicResult = new Dictionary<int, EmTrialResult>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    string state = AllEquipStateData.DicBMS_DC_StateData[item].ChargingState;
                    dd1.Add(item, state);
                    dd2.Add(item, state == "充电中" ? "能充电" : "不能充电");
                    dicResult.Add(item, state != "充电中" ? EmTrialResult.Pass : EmTrialResult.Fail);
                }
                ProcessDataResults(testWorkParam.lstIDs, dd2, $"DC+、DC-对称漏电{resistanceVal}", dicResult, "能否充电");





                Dictionary<int, string> d1 = new Dictionary<int, string>();
                // Dictionary<int, EmTrialResult> dicResult = new Dictionary<int, EmTrialResult>();
                dicResult.Clear();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, DicManualVerifyResult[item] ? "有告警" : "未告警");

                    dicResult.Add(item, DicManualVerifyResult[item] ? EmTrialResult.Pass : EmTrialResult.Fail);
                }
                ProcessDataResults(testWorkParam.lstIDs, d1, $"DC+、DC-对称漏电{resistanceVal}", dicResult, "能否有告警");

                // ProcessDataTmp(d1, $"DC+、DC-对称漏电{resistanceVal}", "充电桩绝缘检测应告警", "-", "-");


                Thread.Sleep(500);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                //   ProcessDataResults(testWorkParam.lstIDs, d1, ItemName2, dicResult, sState);




            }
        }
        //private void ProcessDataResult(string CheckState, string ItemName, string strResult, EmTrialResult trialResult)
        //{

        //    LstTrialData[0].BarCode = LstChargerInfo[0].BarCode;
        //    LstTrialData[0].TrialName = TrialItem.ItemName;
        //    LstTrialData[0].SchemeName = TrialItem.SchemeName;
        //    LstTrialData[0].SchemeID = TrialItem.SchemeID;
        //    LstTrialData[0].ItemName = iIndex.ToString();
        //    LstTrialData[0].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //    double volate = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[0].ChargerId].ChargingVoltage;

        //    LstTrialData[0].TrialResult = trialResult;


        //    LstTrialData[0].PKID = LstChargerInfo[0].PKID;
        //    //界面展示的数据项格式
        //    //状态|测试结果     
        //    LstTrialData[0].ExtentData = CheckState + "|" + ItemName + "|-|-|" + strResult + "|" + "报表(勿删)";
        //    LstTrialData[0].Data2 = LstTrialData[0].ExtentData;
        //    LstTrialData[0].Data3 = TrialItem.TrialOrder.ToString();
        //    SendTrialDataToUI(LstTrialData[0]);
        //    SaveTrialData(LstTrialData[0]);
        //    iIndex++;

        //}
        public override void ProcessData()
        {

        }
    }
}
