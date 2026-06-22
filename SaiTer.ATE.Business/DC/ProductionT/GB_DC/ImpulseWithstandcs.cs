using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using SaiTer.ATE.InterFace;


namespace SaiTer.ATE.Business
{

    /// <summary>
    ///冲击耐压测试
    /// </summary>
    public class ImpulseWithstandcs : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        UInt32 LoadCurrent = 0;
        public ImpulseWithstandcs(int type)
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
                ControlEquipMent.ACSource?.ACSource_ON(lstIDs);

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
                    SendNoticeToUIAndTxtFile("关闭交流源负载以及导引中");

                    ControlEquipMent.ACSource?.ACSource_OFF(testWorkParam.lstIDs);

                    SetLoadDCOFF(testWorkParam.lstIDs);

                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);



                    CountDownTimeInfo("请断开充电桩输入线缆与测试设备的连接，确认与冲击耐压测试仪的连接,符合【输出】的接线，打正极电压。！", 99999,1);


                    CountDownTimeInfo("请确认测试结果是否PASS！\r\n注：勾选上为PASS，否则为FAIL", 99, 2);
                    ProcessData("正极电压");



                    CountDownTimeInfo("请断开充电桩输入线缆与测试设备的连接，确认与冲击耐压测试仪的连接,符合【输入对地】的接线，打负极电压。！", 99999, 1);



                    CountDownTimeInfo("请确认测试结果是否PASS！\r\n注：勾选上为PASS，否则为FAIL", 99, 2);
                    ProcessData("正极电压");





                    CountDownTimeInfo("请断开充电桩输入线缆与测试设备的连接，确认与冲击耐压测试仪的连接,符合【输出对地】的接线，打正极电压。！", 99999, 1);







                    CountDownTimeInfo("请确认测试结果是否PASS！\r\n注：勾选上为PASS，否则为FAIL", 99, 2);
                    ProcessData("正极电压");




                    CountDownTimeInfo("请断开充电桩输入线缆与测试设备的连接，确认与冲击耐压测试仪的连接,符合【输出对地】的接线，打负极电压。！", 99999, 1);

                    CountDownTimeInfo("请确认测试结果是否PASS！\r\n注：勾选上为PASS，否则为FAIL", 99, 2);
                    ProcessData("正极电压");


                    CountDownTimeInfo("请断开充电桩输入线缆与测试设备的连接，确认与冲击耐压测试仪的连接,符合【输入对输出】的接线，打正极电压。！", 99999, 1);

                    CountDownTimeInfo("请确认测试结果是否PASS！\r\n注：勾选上为PASS，否则为FAIL", 99, 2);
                    ProcessData("正极电压");



                    CountDownTimeInfo("请断开充电桩输入线缆与测试设备的连接，确认与冲击耐压测试仪的连接,符合【输入对输出】的接线，打负极电压。！", 99999, 1);

                    CountDownTimeInfo("请确认测试结果是否PASS！\r\n注：勾选上为PASS，否则为FAIL", 99, 2);
                    ProcessData("正极电压");


                    CountDownTimeInfo("请恢复充电桩正常连接！\r\n（确认或倒计时结束后恢复交流源供电）", 999, 0);
                }
            }
            catch (Exception ex) { SendException(ex); }

        }
        public override void ProcessData()
        {

        }

        public  void ProcessData(string ItemName)
        {

            foreach (var item in DicManualVerifyResult)
            {

                int k = LstTrialData.FindIndex(s => s.ChargerId == item.Key);
                int i = LstChargerInfo.FindIndex(s => s.ChargerId == item.Key);
                if (k < 0)
                    return;
                LstTrialData[k].ItemName = iIndex.ToString();
                LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                if (item.Value)
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    LstTrialData[k].ExtentData = ItemName + "|测试结果|-|-|PASS";
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    LstTrialData[k].ExtentData = ItemName + "|测试结果|-|-|FAIL";
                    testWorkParam.lstIDs.Remove(item.Key);
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
