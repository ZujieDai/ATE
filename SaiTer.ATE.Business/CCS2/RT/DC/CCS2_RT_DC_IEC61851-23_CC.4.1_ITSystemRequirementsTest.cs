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
    /// 欧标研测直流：IT系统要求（充电中绝缘故障）
    /// </summary>
    public class CCS2_RT_DC_ITSystemRequirementsTest : BusinessBase
    {
        int trlTimeOut_S = 30;
        int waitTime = 0;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        public CCS2_RT_DC_ITSystemRequirementsTest(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Data_Tmp = new Dictionary<int, string>();//临时测试数据
            OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
            dImgs = new Dictionary<int, string>();//图片存储
            string[] strParams = TrialItem.ResultParams.Split('|');
            if(strParams.Length > 0 && strParams[0].Split('=').Length > 1)
            {
                waitTime = (int)Convert.ToDouble(strParams[0].Split('=')[1]);
            }
            //BMSDemandVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            //LoadCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
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
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    //添加测试条件
                    SetConditionValues();



                    SendNoticeToUIAndTxtFile("设置DC+非对称漏电300KΩ");
                    TrialMethod("DC+非对称漏电300KΩ", "应能充电", "应有警告状态", "警告", 7, 0);


                    SendNoticeToUIAndTxtFile("设置DC-非对称漏电300KΩ");
                    TrialMethod("DC-非对称漏电300KΩ", "应能充电", "应有警告状态", "警告", 0, 7);


                    SendNoticeToUIAndTxtFile("设置DC-、DC+对称漏电300KΩ");
                    TrialMethod("DC+、DC-对称漏电300KΩ", "应能充电", "应有警告状态", "警告", 7, 7);


                    SendNoticeToUIAndTxtFile("设置DC+非对称漏电33KΩ");
                    TrialMethod("DC+非对称漏电33KΩ", "应不能充电", "应有故障状态", "故障", 4, 0, false);


                    SendNoticeToUIAndTxtFile("设置DC-非对称漏电33KΩ");
                    TrialMethod("DC-非对称漏电33KΩ", "应不能充电", "应有故障状态", "故障", 0, 4, false);


                    SendNoticeToUIAndTxtFile("设置DC-、DC+对称漏电33KΩ");
                    TrialMethod("DC+、DC-对称漏电33KΩ", "应不能充电", "应有故障状态", "故障", 4, 4, false);

                }
            }
            catch (Exception ex) { SendException(ex); }
        }

        private void TrialMethod(string sState, string ItemName1, string ItemName2, string ItemName3, int DCPlus, int DCMinus, bool CanCharge = true)
        {
            SetCPRersh_EUDCALL();
            Thread.Sleep(3000);

            if (!CheckSwipingCard(testWorkParam.lstIDs))
            {
                return;
            }
            Thread.Sleep(3000);

            bool[] Ks = new bool[24];
            Ks[0] = true;//DC+DC-控制
            Ks[1] = true;//CC信号控制
            Ks[2] = true;//CP信号控制
            Ks[4] = true;//PE信号控制
            Ks[4] = true;//PE信号控制
            ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, BatteryVoltage_EU, Ks.ToArray(), DCPlus, DCMinus, "0");
            Thread.Sleep(3000);

            if(waitTime > 0)
            {
                CountDownTimeInfo("判断充电机是否能充电倒计时", waitTime, 0);
            }

            d1 = new Dictionary<int, string>();
            d2 = new Dictionary<int, string>();
            var dicVolt = new Dictionary<int, string>();
            string state = AllEquipStateData.DicBMS_EU_DC_StateData[1].SystemState;
            if (CanCharge)
            {
                foreach (int item in testWorkParam.lstIDs)
                {
                    int timeout = 50;
                    double voltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                    while (timeout-- > 0)
                    {
                        if (voltage < LstChargerInfo[0].NominalVoltage * 0.9 || voltage > LstChargerInfo[0].NominalVoltage * 1.1)
                        {
                            voltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                            Thread.Sleep(100);
                        }
                        else
                            break;
                    }
                    dicVolt.Add(item, voltage.ToString("F2"));
                }
                ProcessDataTmp(dicVolt, sState, "直流输出电压(V)", "-", "-");
                if (state == "CurrentDemandReq" || state == "CurrentDemandRes")
                {
                    ProcessDataResult(sState, ItemName1, "能充电", EmTrialResult.Pass);
                }
                else
                {
                    ProcessDataResult(sState, ItemName1, "不能充电", EmTrialResult.Fail);
                }
            }
            else
            {
                foreach (int item in testWorkParam.lstIDs)
                {
                    int timeout = 50;
                    double voltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                    while (timeout-- > 0)
                    {
                        if (voltage > 20)
                        {
                            voltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                            Thread.Sleep(300);
                        }
                    }
                    dicVolt.Add(item, voltage.ToString("F2"));
                }
                ProcessDataTmp(dicVolt, sState, "直流输出电压(V)", "0", "20");
                if (state != "充电中")
                {
                    ProcessDataResult(sState, ItemName1, "不能充电", EmTrialResult.Pass);
                }
                else
                {
                    ProcessDataResult(sState, ItemName1, "能充电", EmTrialResult.Fail);
                }
            }


            CountDownTimeInfo("请人工确认是否有绝缘告警,\r\n 勾选上代表有告警", 100, 2);
            d1.Clear();
            d2.Clear();
            if (DicManualVerifyResult[LstChargerInfo[0].ChargerId])
            {
                ProcessDataResult(sState, ItemName2, "有告警", EmTrialResult.Pass);
            }
            else
            {
                ProcessDataResult(sState, ItemName2, "未告警", EmTrialResult.Fail);
            }
            ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, BatteryVoltage_EU, Ks.ToArray(), 0, 0, "0");
            Thread.Sleep(300);
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            Thread.Sleep(2000);
        }

        private void ProcessDataResult(string CheckState, string ItemName, string strResult, EmTrialResult trialResult)
        {

            LstTrialData[0].BarCode = LstChargerInfo[0].BarCode;
            LstTrialData[0].TrialName = TrialItem.ItemName;
            LstTrialData[0].SchemeName = TrialItem.SchemeName;
            LstTrialData[0].SchemeID = TrialItem.SchemeID;
            LstTrialData[0].ItemName = iIndex.ToString();
            LstTrialData[0].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            double volate = AllEquipStateData.DicBMS_EU_DC_StateData[LstTrialData[0].ChargerId].ChargingVoltage;

            LstTrialData[0].TrialResult = trialResult;


            LstTrialData[0].PKID = LstChargerInfo[0].PKID;
            //界面展示的数据项格式
            //状态|测试结果     
            LstTrialData[0].ExtentData = CheckState + "|" + ItemName + "|-|-|" + strResult + "|" + "报表(勿删)";
            LstTrialData[0].Data2 = LstTrialData[0].ExtentData;
            LstTrialData[0].Data3 = TrialItem.TrialOrder.ToString();
            SendTrialDataToUI(LstTrialData[0]);
            SaveTrialData(LstTrialData[0]);
            iIndex++;

        }
        public override void ProcessData()
        {

        }
    }
}
