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
    /// 效率功率因素欧标
    /// </summary>
    public class EfficiencyPowerFactorEU : BusinessBase
    {
        int Count = 0;
        public EfficiencyPowerFactorEU(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;
        double Efficiency = 88;//效率(%)1
        double Efficiency2 = 93;//效率(%)2
        double PowerFactrer = 0.95;//功率因数1
        double PowerFactrer2 = 0.98;//功率因数2
        double[] CurrentS = new double[3];
        private double BMSMeasureVoltage = 390;//过压参考值
        private int trlTimeOut_S = 5;
        Dictionary<int, string> Rates = new Dictionary<int, string>();
        Dictionary<int, string> Efficiencys = new Dictionary<int, string>();
        Dictionary<int, string> Powerfactors = new Dictionary<int, string>();

        double[] CurrentS2 = new double[3];
        double[] VoltageS2 = new double[3];
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
                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    Thread.Sleep(2000);//等待回馈负载电流稳定



                    double MidChargeVoltage =MaxOutputPower * 1000 / ResiLoadCurrent;


                    Double DemandCurrent3 = MaxOutputPower * 1000 / MidChargeVoltage / 5;

                    DemandCurrent3 =CompareMaximum(DemandCurrent3, ResiLoadCurrent);


                    Double MaxCurrent1 = MidChargeVoltage * 0.24;
                    DemandCurrent3 =CompareMaximum(DemandCurrent3, ResiLoadCurrent);
                    DemandCurrent3 =CompareMaximum(DemandCurrent3, MaxCurrent1);



                    Double DemandCurrent2 = MaxOutputPower * 1000 / MidChargeVoltage / 2;

                    DemandCurrent2 =CompareMaximum(DemandCurrent2, ResiLoadCurrent);



                    DemandCurrent2 =CompareMaximum(DemandCurrent2, ResiLoadCurrent);
                    DemandCurrent2 =CompareMaximum(DemandCurrent2, MaxCurrent1);



                    Double DemandCurrent1 =MaxOutputPower * 1000 / MidChargeVoltage;

                    DemandCurrent1 =CompareMaximum(DemandCurrent1, ResiLoadCurrent);



                    DemandCurrent1 =CompareMaximum(DemandCurrent1, ResiLoadCurrent);
                    DemandCurrent1 =CompareMaximum(DemandCurrent1, MaxCurrent1);







                    CurrentS[0] = DemandCurrent1;
                    CurrentS[1] = DemandCurrent2;
                    CurrentS[2] = DemandCurrent3;





                    for (int i = 0; i < 3; i++)
                    {
                        bool LOAD = false;
                        if (ControlEquipMent.FeedbackLoad != null)
                        {
                            LOAD = true;
                        }
                        else
                        {
                            double VoltageMax = CurrentS[i] / 0.24;
                            if (VoltageMax <= MidChargeVoltage)
                            {
                                LOAD = true;
                            }
                        }
                        if (LOAD)
                        {
                            SendNoticeToUIAndTxtFile("正在测试中...");
                            System.Threading.Thread.Sleep(2000);
                            bool FeedOn = AllEquipStateData.DicPowerAnalyzer_StateData.Any(kvp => kvp.Value.Channel4RMSCurrent > 2);
                            if (FeedOn)
                            {
                                System.Threading.Thread.Sleep(3000);
                                SendNoticeToUIAndTxtFile("关闭负载中...");

                                SetLoadPara(testWorkParam.lstIDs, MidChargeVoltage - 20, 5, MidChargeVoltage,5);
                                System.Threading.Thread.Sleep(3000);
                                SetLoadDCOFF(testWorkParam.lstIDs);
                            }

                            System.Threading.Thread.Sleep(5000);
                            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, MidChargeVoltage, CurrentS[i], true, BMSMeasureVoltage);

                            SendNoticeToUIAndTxtFile("等待充电电压稳定中...");
                            int timeout2 = 60;
                            while (timeout2-- > 0)
                            {
                                bool StabilizeVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.Any(kvp => kvp.Value.ChargingVoltage < MidChargeVoltage * 0.85 || kvp.Value.ChargingVoltage > MidChargeVoltage * 1.1);
                                if (!StabilizeVoltage)
                                {
                                    break;
                                }

                                System.Threading.Thread.Sleep(1000);
                            }
                            System.Threading.Thread.Sleep(10 * 1000);



                            SetLoadPara(testWorkParam.lstIDs, MidChargeVoltage - 20, CurrentS[i] + 5, MidChargeVoltage-5, CurrentS[i]);
                            SetLoadDCON(testWorkParam.lstIDs);

                            //SendNoticeToUIAndTxtFile("等待充电电压稳定中...");
                            System.Threading.Thread.Sleep(8000);


                            int time = 10;
                            while (time-- > 0)
                            {
                                bool StabilizePower = AllEquipStateData.DicPowerAnalyzer_StateData.Any(kvp => kvp.Value.Channel4Power <= 0);
                                if (!StabilizePower)
                                {
                                    break;
                                }
                                System.Threading.Thread.Sleep(1000);
                            }
                            System.Threading.Thread.Sleep(3000);
                            Rates.Clear();
                            foreach (var item in testWorkParam.lstIDs)
                            {
                                double RealPower = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4Power;
                                double Rate = RealPower / MaxOutputPower;
                                Rates.Add(item, Rate.ToString("F2"));
                            }
                            Efficiencys.Clear();
                            foreach (var item in testWorkParam.lstIDs)
                            {
                                double efficiency = AllEquipStateData.DicPowerAnalyzer_StateData[item].Efficiency;

                                Efficiencys.Add(item, efficiency.ToString("F2"));
                            }
                            Powerfactors.Clear();
                            foreach (var item in testWorkParam.lstIDs)
                            {
                                double powerfactor = AllEquipStateData.DicPowerAnalyzer_StateData[item].TotalPowerFactor;

                                Powerfactors.Add(item, powerfactor.ToString("F2"));
                            }
                            ProcessDataRate(CurrentS[i]);
                            ProcessDataPowerfactor(CurrentS[i]);
                        }
                    }



                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    //SendNoticeToUIAndTxtFile("关闭导引中...");
                    //SetLoadDCOFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    Thread.Sleep(2000);//等待回馈负载电流稳定


                    Double DemandVoltage1_2 = MinAllowChargeVoltage;
                    Double DemandVoltage3_2 = MaxAllowChargeVoltage;
                    Double DemandVoltage2_2 = (DemandVoltage1_2 + DemandVoltage3_2) / 2;
                    VoltageS2[0] = DemandVoltage3_2;
                    VoltageS2[1] = DemandVoltage2_2;
                    VoltageS2[2] = DemandVoltage1_2;

                    Double DemandCurrent1_2 = MaxOutputPower * 1000 / DemandVoltage1_2;
                    DemandCurrent1_2 = CompareMaximum(DemandCurrent1_2, ResiLoadCurrent);
                    Double MaxCurrent2 = DemandVoltage1_2 * 0.24;
                    DemandCurrent1_2 = CompareMaximum(DemandCurrent1_2, ResiLoadCurrent);
                    DemandCurrent1_2 = CompareMaximum(DemandCurrent1_2, MaxCurrent2);
                    Double DemandCurrent2_2 = MaxOutputPower * 1000 / DemandVoltage2_2;
                    DemandCurrent2_2 = CompareMaximum(DemandCurrent2_2, ResiLoadCurrent);
                    Double MaxCurrent3 = DemandVoltage2_2 * 0.24;
                    DemandCurrent2_2 = CompareMaximum(DemandCurrent2_2, ResiLoadCurrent);
                    DemandCurrent2_2 = CompareMaximum(DemandCurrent2_2, MaxCurrent3);
                    Double DemandCurrent3_2 = MaxOutputPower * 1000 / DemandVoltage3_2;
                    DemandCurrent3_2 = CompareMaximum(DemandCurrent3_2, ResiLoadCurrent);
                    Double MaxCurrent4 = DemandVoltage3_2 * 0.24;
                    DemandCurrent3_2 = CompareMaximum(DemandCurrent3_2, ResiLoadCurrent);
                    DemandCurrent3_2 = CompareMaximum(DemandCurrent3_2, MaxCurrent4);
                    CurrentS2[0] = DemandCurrent3_2;
                    CurrentS2[1] = DemandCurrent2_2;
                    CurrentS2[2] = DemandCurrent1_2;


                    for (int i = 0; i < 3; i++)
                    {
                        bool LOAD = false;
                        if (ControlEquipMent.FeedbackLoad != null)
                        {
                            LOAD = true;
                        }
                        else
                        {
                            double VoltageMax = CurrentS2[i] / 0.24;
                            if (VoltageMax <= VoltageS2[i])
                            {
                                LOAD = true;
                            }
                        }
                        if (LOAD)
                        {
                            SendNoticeToUIAndTxtFile("正在测试中...");
                            bool FeedOn = AllEquipStateData.DicPowerAnalyzer_StateData.Any(kvp => kvp.Value.Channel4RMSCurrent > 2);
                            if (FeedOn)
                            {
                                SendNoticeToUIAndTxtFile("关闭负载中...");

                                SetLoadPara(testWorkParam.lstIDs, VoltageS2[i] - 20, 5, VoltageS2[i],5);
                                System.Threading.Thread.Sleep(3000);
                                SetLoadDCOFF(testWorkParam.lstIDs);
                            }

                            System.Threading.Thread.Sleep(5000);
                            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, VoltageS2[i], CurrentS2[i], true, 390);

                            int timeout2 = 100;
                            SendNoticeToUIAndTxtFile("等待充电电压稳定中...");

                            while (timeout2-- > 0)
                            {
                                bool StabilizeVoltage = AllEquipStateData.DicBMS_EU_DC_StateData.Any(kvp => kvp.Value.ChargingVoltage < VoltageS2[i] * 0.85 || kvp.Value.ChargingVoltage >VoltageS2[i] * 1.1);
                                if (!StabilizeVoltage)
                                {
                                    break;
                                }

                                System.Threading.Thread.Sleep(1000);
                            }
                            System.Threading.Thread.Sleep(10 * 1000);


                            SetLoadPara(testWorkParam.lstIDs, VoltageS2[i] - 20, CurrentS2[i] + 5, VoltageS2[i]-5, CurrentS2[i]);

                            SetLoadDCON(testWorkParam.lstIDs);

                            System.Threading.Thread.Sleep(20 * 1000);

                            //SendNoticeToUIAndTxtFile("等待充电电压稳定中...");
                            System.Threading.Thread.Sleep(8000);

                            int time = 10;
                            while (time-- > 0)
                            {
                                bool StabilizePower = AllEquipStateData.DicPowerAnalyzer_StateData.Any(kvp => kvp.Value.Channel4Power <= 0);
                                if (!StabilizePower)
                                {
                                    break;
                                }
                                System.Threading.Thread.Sleep(1000);
                            }
                            System.Threading.Thread.Sleep(3000);
                            Rates.Clear();
                            foreach (var item in testWorkParam.lstIDs)
                            {
                                double RealPower = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4Power;
                                double Rate = RealPower / MaxOutputPower;
                                Rates.Add(item, Rate.ToString("F2"));
                            }
                            Efficiencys.Clear();
                            foreach (var item in testWorkParam.lstIDs)
                            {
                                double efficiency = AllEquipStateData.DicPowerAnalyzer_StateData[item].Efficiency;

                                Efficiencys.Add(item, efficiency.ToString("F2"));
                            }
                            Powerfactors.Clear();
                            foreach (var item in testWorkParam.lstIDs)
                            {
                                double powerfactor = AllEquipStateData.DicPowerAnalyzer_StateData[item].TotalPowerFactor;

                                Powerfactors.Add(item, powerfactor.ToString("F2"));
                            }
                            ProcessDataRate(CurrentS2[i]);
                            ProcessDataPowerfactor(CurrentS2[i]);
                        }
             

                    }

                    SendNoticeToUIAndTxtFile("关闭负载中...");

                    SetLoadDCOFF(testWorkParam.lstIDs);

                }
            }
            catch (Exception ex) { SendException(ex); }


        }

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
        }

        public override void InitializeParams()
        {
            Init();
            Count = 0;
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;

            if (strParams.Length >= 4)
            {
                //BMSVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                //BMSCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
                //BMSVoltage2 = Convert.ToDouble(strParams[2].Split('=')[1]);
                //BMSCurrent2 = Convert.ToDouble(strParams[3].Split('=')[1]);
                //ErrorVoltageRate = Convert.ToDouble(strParams[4].Split('=')[1]) / 100;
                //ErrorCurrentRate = Convert.ToDouble(strParams[5].Split('=')[1]) / 100;
                //TestTime = Convert.ToInt32(strParams[0].Split('=')[1]);
                Efficiency = Convert.ToDouble(strParams[0].Split('=')[1]);
                PowerFactrer = Convert.ToDouble(strParams[1].Split('=')[1]);
                Efficiency2 = Convert.ToDouble(strParams[2].Split('=')[1]);
                PowerFactrer2 = Convert.ToDouble(strParams[3].Split('=')[1]);
            }
        }

        public override void ProcessData()
        {
     
        }


        public void ProcessDataRate(double Current)
        {
            try
            {
                Count++;
                foreach (var item in testWorkParam.lstIDs)
                {
               
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;

                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].ItemName = iIndex.ToString();
                    double Rate = Convert.ToDouble(Rates[LstChargerInfo[i].ChargerId]);//数据
                    double efficiency = Convert.ToDouble(Efficiencys[LstChargerInfo[i].ChargerId]);//数据
                    if (Rate <= 0.5)
                    {
                        if(efficiency>= Efficiency)
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        }
                        LstTrialData[i].ExtentData = "第"+Count+"点"+"带载电流" + Current + "|效率|"+ Efficiency + "|-|" + efficiency;
                    }
                    else
                    {
                        if (efficiency >= Efficiency2)
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        }
                        LstTrialData[i].ExtentData = "第" + Count + "点" + "带载电流" + Current + "|效率|" + Efficiency2 + "|-|" + efficiency;
                    }

                    //界面展示的数据项格式
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }
            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }


        public void ProcessDataPowerfactor(double Current)
        {
            try
            {
                Count++;
                foreach (var item in testWorkParam.lstIDs)
                {
             
                    int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                    int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                    if (k < 0)
                        return;
                    LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;

                    LstTrialData[k].TrialName = TrialItem.ItemName;
                    LstTrialData[k].SchemeName = TrialItem.SchemeName;
                    LstTrialData[k].SchemeID = TrialItem.SchemeID;
                    LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LstTrialData[k].ItemName = iIndex.ToString();
                    double Rate = Convert.ToDouble(Rates[LstChargerInfo[i].ChargerId]);//数据
                    double powerfactor = Convert.ToDouble(Powerfactors[LstChargerInfo[i].ChargerId]);//数据
                    if (Rate <= 0.5)
                    {
                        if (powerfactor >= PowerFactrer)
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        }
                        LstTrialData[i].ExtentData = "第" + Count + "点" + "带载电流" + Current + "|输入功率因数(%)|" + PowerFactrer + "|-|" + powerfactor;
                    }
                    else
                    {
                        if (powerfactor >= PowerFactrer2)
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        }
                        else
                        {
                            LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        }
                        LstTrialData[i].ExtentData = "第" + Count + "点" + "带载电流" + Current + "|输入功率因数(%)|" + PowerFactrer2 + "|-|" + powerfactor;
                    }

                    //界面展示的数据项格式
                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);
                    iIndex++;
                }


            }

            catch (Exception ex)
            {
                SendException(ex);
            }
        }

        public  double CompareMaximum(double value, double maxvalue)
        {
            try
            {
                value = value >= maxvalue ? maxvalue : value;
                return value;
            }
            catch
            {
                return 0;
            }
        }

    }
}
