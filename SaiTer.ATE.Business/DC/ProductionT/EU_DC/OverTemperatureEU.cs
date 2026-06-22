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
    /// 过温保护实验
    /// </summary>
    public class OverTemperatureEU : BusinessBase
    {
        public OverTemperatureEU(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;//测试时间
        double BMSDemandVolt = 0;//额定电压
        double ResiLoadCurrent = 0;//额定电流
        double DemandVoltage = 220;//交流源电压
        double DemandCurrent = 50;//频率
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

                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, 390);
                    //ControlEquipMent.BMS.BMS_ON(lstIDs);
                    //Thread.Sleep(10 * 1000);
                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    Thread.Sleep(2000);//
                    SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");


                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent + 5, DemandVoltage-5, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);


                    CountDownTimeInfo("请模拟充电机内部温度超过过温保护值再点击是!", 9999,1);


                    CountDownTimeInfo("等待停止充电中!", 10, 0);


                    string info = "\r\n请人工检查!(枪号勾选上为PASS，否则为FAIL)";
                    SendNoticeToUIAndTxtFile(info);


                    CountDownTimeInfo("人工确认输出功率下降或者停止充电!"+info, 999, 2);


                    ProcessData();


                    SendNoticeToUIAndTxtFile("设备正在关闭负载中，请稍候...");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("恢复互操作中...");
                    SetCPRersh_EUDC();


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
                DemandVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                DemandCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
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


                if (DicManualVerifyResult[item])
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    LstTrialData[k].ExtentData = TrialItem.ItemName + "|是否停止输出|-|-|是";
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    LstTrialData[k].ExtentData = TrialItem.ItemName + "|是否停止输出|-|-|否";
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     
            
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
            }
        }


    }
}
