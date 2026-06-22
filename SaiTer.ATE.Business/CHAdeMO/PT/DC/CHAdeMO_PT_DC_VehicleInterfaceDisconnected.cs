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
    /// 车辆接口断开
    /// </summary>
    public class CHAdeMO_PT_DC_VehicleInterfaceDisconnected : BusinessBase
    {
        public CHAdeMO_PT_DC_VehicleInterfaceDisconnected(int type)
        {
            TrialType = type;
        }
        private int trlTimeOut_S = 5;

        double BMSDemandVoltage = 0;
        double BMSDemandCurrent = 0;
        double OverVoltage = 0;//过压参考值
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
                //恢复连接信号正常
                SendNoticeToUIAndTxtFile("恢复连接信号正常...");
                ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x8A, true, new string[] { "emtBMS_JP_DC" });
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

                    System.Threading.Thread.Sleep(5000);//等待电压稳定

                    //带载
                    SendNoticeToUIAndTxtFile("开启负载中...");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVoltage - 10, BMSDemandCurrent + 10, BMSDemandVoltage, BMSDemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);
                    WaitDCCurrentWithTime_JP_DC(testWorkParam.lstIDs, BMSDemandCurrent, 35);

                    System.Threading.Thread.Sleep(2000);//等待电流稳定

                    //模拟连接信号中断
                    SendNoticeToUIAndTxtFile("模拟连接信号中断...");
                    ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x8A, false, new string[] { "emtBMS_JP_DC" });
                    Thread.Sleep(100);

                    System.Threading.Thread.Sleep(2000);
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    System.Threading.Thread.Sleep(5000);

                    string stmp;
                    stmp = ChangeBMSChargeStatus_JP_DC(AllEquipStateData.DicBMS_JP_DC_StateData[testWorkParam.lstIDs.First()].SystemState) != 7 ? "是" : "否";
                    ProcessDataResult(testWorkParam.lstIDs, stmp, "是否结束充电", stmp == "是" ? true : false, "车辆接口断开");

                    d1 = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_JP_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "车辆接口断开", "输出电压(V)", "-", "20");

                    d1 = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_JP_DC_StateData[testWorkParam.lstIDs[i]].ChargingCurrent.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "车辆接口断开", "输出电流(A)", "-", "5");

                }
            }
            catch (Exception ex) { SendException(ex); }
        }

        public override void ProcessData()
        {

        }
    }

}
