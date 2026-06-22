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
    /// 国标直流低压辅助电源测试/低压辅源测试   程控板控制 辅源负载
    /// </summary>
    public class AuxiliaryPower_Ctrl : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        int LoadCurrent = 0;

        public AuxiliaryPower_Ctrl(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            LoadCurrent = Convert.ToInt32(double.Parse(strParams[0].Split('=')[1]));
            if (LoadCurrent < 0 || LoadCurrent > 16)
            {
                LoadCurrent = 12;
            }
            //LoadCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;

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
                SetCPReresh();
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


                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, 20, BMSDemandVolt, 20);

                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    Thread.Sleep(1000 * 5);//等待回馈负载电流稳定
                    SetConditionValues();


                    SendNoticeToUIAndTxtFile("发送过流指令");

                    double apsVoltage = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].APSVoltage;

                    if (apsVoltage >= 18)
                    {
                        ControlEquipMent.AuxiliaryLoadCtrl.Set24VCurrent(testWorkParam.lstIDs, LoadCurrent);
                    }
                    else
                    {
                        ControlEquipMent.AuxiliaryLoadCtrl.Set12VCurrent(testWorkParam.lstIDs, LoadCurrent);
                    }

                    Thread.Sleep(5000);
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "辅源过流", "充电电压", "0", "20");
                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].APSVoltage;
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "辅源过流", "辅源电压", "0", "2");

                    ControlEquipMent.AuxiliaryLoadCtrl.CancelAllState(testWorkParam.lstIDs);
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    SetCPReresh();
                    Thread.Sleep(1000);

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }


                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, 20, BMSDemandVolt, 20);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    Thread.Sleep(1000 * 5);//等待负载电流稳定
                    SendNoticeToUIAndTxtFile("发送短路指令");

                    ControlEquipMent.AuxiliaryLoadCtrl.SetShortCircuite(testWorkParam.lstIDs);
                    Thread.Sleep(2000);

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "辅源短路", "充电电压", "0", "20");

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].APSVoltage;
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "辅源短路", "辅源电压", "0", "2");

                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    ControlEquipMent.AuxiliaryLoadCtrl.CancelAllState(testWorkParam.lstIDs);
                    SetCPReresh();
                    Thread.Sleep(2000);

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    SendNoticeToUIAndTxtFile("发送过压指令");
                    if (apsVoltage >= 18)
                    {
                        ControlEquipMent.AuxiliaryLoadCtrl.Set24VoltOver(testWorkParam.lstIDs);
                    }
                    else
                    {
                        ControlEquipMent.AuxiliaryLoadCtrl.Set12VoltOver(testWorkParam.lstIDs);
                    }

                    Thread.Sleep(5000);
                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }

                    ProcessDataTmp(dic, "辅源过压", "充电电压", "0", "20");
                    ControlEquipMent.AuxiliaryLoadCtrl.CancelAllState(testWorkParam.lstIDs);
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
