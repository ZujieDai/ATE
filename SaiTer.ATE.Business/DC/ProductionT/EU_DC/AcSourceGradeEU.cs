using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 交流源输入等级欧标
    /// </summary>
    public class AcSourceGradeEU : BusinessBase
    {
        public AcSourceGradeEU(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;//测试时间
        double BMSDemandVolt = 0;//额定电压
        double ResiLoadCurrent = 0;//额定电流
        double AcSourceVolt = 220;//交流源电压
        double Freq = 50;//频率
        int trlTimeOut_S = 0;

         
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

                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 500, 250, true, LstChargerInfo[0].NominalVoltage);

                    //Thread.Sleep(10*1000);//


                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, 500, LstChargerInfo[0].ChargerType, 0);

                    Thread.Sleep(2000);//



                    SendNoticeToUIAndTxtFile("正在发送调整交流变频电源的输出电压-10%...");

                    

                    ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, AcSourceVolt*0.9);









                    SendNoticeToUIAndTxtFile("判断结果中...");
                    CountDownTimeInfo("判断充电延时", 5, 0);





                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }


                    ProcessDataTmp(dic, "交流源电压-10%", "充电电压(V)", "480", "520");


                    SendNoticeToUIAndTxtFile("恢复交流源电压中...");
                    ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, AcSourceVolt );


                    SendNoticeToUIAndTxtFile("关闭导引中...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("恢复互操作中...");
                    Thread.Sleep(25000);
                    SetCPRersh_EUDC();



                    #region 
                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");


                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);




                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, 500, LstChargerInfo[0].ChargerType, 0);

                    Thread.Sleep(2000);//



                    SendNoticeToUIAndTxtFile("正在发送调整交流变频电源的输出电压+10%...");



                    ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, AcSourceVolt * 1.1);









                    SendNoticeToUIAndTxtFile("判断结果中...");
                    CountDownTimeInfo("判断充电延时", 5, 0);





                    Dictionary<int, string> dic2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                        dic2.Add(item, DCVoltage.ToString("F2"));
                    }


                    ProcessDataTmp(dic2, "交流源电压+10%", "充电电压(V)", "480", "520");



                    SendNoticeToUIAndTxtFile("恢复交流源电压中...");
                    ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, AcSourceVolt);

                    SendNoticeToUIAndTxtFile("关闭导引中...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);



                    SendNoticeToUIAndTxtFile("恢复互操作中...");
                    Thread.Sleep(25000);
                    SetCPRersh_EUDC();



                    #endregion



                    #region 
                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");


                    ControlEquipMent.BMS.BMS_ON(lstIDs);





                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, 500, LstChargerInfo[0].ChargerType, 0);

                    Thread.Sleep(2000);//



                    SendNoticeToUIAndTxtFile("正在发送交流源调整输出频率+1%...");



                    ControlEquipMent.ACSource.ACSource_SetFreq(lstIDs, Freq * 1.01);









                    SendNoticeToUIAndTxtFile("判断结果中...");
                    CountDownTimeInfo("判断充电延时", 5, 0);





                    Dictionary<int, string> dic3 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                        dic3.Add(item, DCVoltage.ToString("F2"));
                    }


                    ProcessDataTmp(dic3, "交流源频率+1%", "充电电压(V)", "480", "520");



                    SendNoticeToUIAndTxtFile("恢复交流源频率中...");
                    ControlEquipMent.ACSource.ACSource_SetVolt(lstIDs, Freq);

                    SendNoticeToUIAndTxtFile("关闭导引中...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("恢复互操作中...");
                    Thread.Sleep(25000);
                    SetCPRersh_EUDC();



                    #endregion

                    #region 
                    SendNoticeToUIAndTxtFile("设备正在启动充电中，请稍候...");


                    ControlEquipMent.BMS.BMS_ON(lstIDs);





                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, 500, LstChargerInfo[0].ChargerType, 0);

                    Thread.Sleep(2000);//



                    SendNoticeToUIAndTxtFile("正在发送交流源调整输出频率-1%...");



                    ControlEquipMent.ACSource.ACSource_SetFreq(lstIDs, Freq * 0.99);









                    SendNoticeToUIAndTxtFile("判断结果中...");
                    CountDownTimeInfo("判断充电延时", 5, 0);





                    Dictionary<int, string> dic4 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                        dic4.Add(item, DCVoltage.ToString("F2"));
                    }


                    ProcessDataTmp(dic4, "交流源频率-1%", "充电电压(V)", "480", "520");



                    SendNoticeToUIAndTxtFile("恢复交流源频率中...");
                    ControlEquipMent.ACSource.ACSource_SetVolt(lstIDs, Freq);


                    SendNoticeToUIAndTxtFile("关闭导引中...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);



                    SendNoticeToUIAndTxtFile("恢复互操作中...");
                    Thread.Sleep(25000);
                    SetCPRersh_EUDC();



                    #endregion

                }
            }
            catch (Exception ex) { SendException(ex); }


        }


        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
            if (strParams.Length >= 2)
            {
                AcSourceVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
                Freq= Convert.ToDouble(strParams[1].Split('=')[1]);
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

                LstTrialData[k].ItemName = iIndex.ToString();
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                if (DicManualVerifyResult[item])
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     
                LstTrialData[k].ExtentData = TrialItem.ItemName + "|-|-|-|-";
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
            }
        }


    }

}
