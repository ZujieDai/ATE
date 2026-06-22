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
    /// 欧标研测直流：系统断电（短路，重复项不做）
    /// </summary>
    public class CCS2_RT_DC_SystemDe_energizationTest : BusinessBase
    {

        int trlTimeOut_S = 30;
        Double DemandVoltage = 500;
        Double DemandCurrent = 20;
        /// <summary>
        /// 继电器序号
        /// </summary>
        int mYRelayIndex = 0;



        public CCS2_RT_DC_SystemDe_energizationTest(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

            Thread.Sleep(2000);
            SetCPReresh();
        }

        public override void InitializeParams()
        {
            //断路开关号(从1开始)=
            string[] strParams = TrialItem.ItemParams.Split('|');
            if (strParams.Length >= 1)
            {
                double index = double.Parse(strParams[0].Split('=')[1]);
                mYRelayIndex = Convert.ToInt32(index);
                mYRelayIndex = mYRelayIndex - 1;
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
                List<bool> list = ControlEquipMent.ControlBoard.ControlBoardReadState();
                list[mYRelayIndex] = false;
                ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);
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
                    SetConditionValues();
                    SendNoticeToUIAndTxtFile("开启导引中");

                    if (!CheckSwipingCard(testWorkParam.lstIDs, 500))
                    {
                        return;
                    }
                    Thread.Sleep(3000);

                    SendNoticeToUIAndTxtFile("模拟闭合短路");
                    //CountDownTimeInfo("请连接短路接线!", 99999, 0);
                    List<bool> list = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    list[mYRelayIndex] = true;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);
                    Thread.Sleep(500);

                    //SendNoticeToUIAndTxtFile("启动充电中");

                    //MessgaeInfo(true, "请刷卡充电!");

                    //DateTime dts = DateTime.Now;
                    //while (true)
                    //{
                    //    int state = ChangeBMSChargeStatus_EU_DC(AllEquipStateData.DicBMS_EU_DC_StateData[LstChargerInfo[0].ChargerId].SystemState);
                    //    if (state >= 20)
                    //    {
                    //        MessgaeInfo(false, "请刷卡充电!");
                    //        break;
                    //    }
                    //    if (state < 1 || dts.AddSeconds(90) < DateTime.Now)
                    //    {
                    //        MessgaeInfo(false, "请刷卡充电!");
                    //        break;
                    //    }
                    //    System.Threading.Thread.Sleep(100);
                    //}



                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        int time = 100;
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                        while(time-- > 0)
                        {
                            if (DCVoltage.GetValueOrDefault() > 60 || DCVoltage.GetValueOrDefault() < 0)
                            {
                                DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4RMSVolt;
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    //1秒内电压下降到60V
                    ProcessDataTmp(dic, "短路故障后", "充电电压(V)", "0", "60");


                    System.Threading.Thread.Sleep(2000);
                    CountDownTimeInfo("请检查充电桩是否有故障报警!", 999, 2);
                    ProcessDataConnect("应发出告警提示", "是否有告警提示");

                    //ProcessData();

                    SendNoticeToUIAndTxtFile("关闭导引中!");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    //CountDownTimeInfo("请将短路恢复到正常（断开短接线）!", 99999, 0);


                    //ProcessData();
                    //Dictionary<int, string> dic = new Dictionary<int, string>();
                    //foreach (var item in testWorkParam.lstIDs)
                    //{
                    //    double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                    //    dic.Add(item, DCVoltage.ToString("F2"));
                    //}


                    //ProcessDataTmp(dic, TrialItem.ItemName, "充电电压(V)", "0", "20");




                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }



        public override void ProcessData()
        {
            try
            {
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
                    string State = AllEquipStateData.DicBMS_DC_StateData[LstTrialData[k].ChargerId].ChargingState;
                    int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[item].ChargingState);
                    if (state > 4 && state <= 9)
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        State = "否";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Pass;
                        State = "是";
                    }

                    //if (Status)
                    //{
                    //    LstTrialData[k].TrialResult = EmTrialResult.Pass;

                    //}
                    //else
                    //{
                    //    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    //}
                    //界面展示的数据项格式   
                    LstTrialData[i].ExtentData = "直流输出回路出现短路故障|是否停止充电过程|-|-|" + State;
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

    }
}
