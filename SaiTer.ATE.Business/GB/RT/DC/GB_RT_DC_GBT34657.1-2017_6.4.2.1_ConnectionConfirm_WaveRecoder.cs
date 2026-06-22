using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
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
    /// 研发测试:连接确认测试(不使用录波仪)
    /// </summary>
    public class GB_RT_DC_ConnectionConfirm_WaveRecoder : BusinessBase
    {
        int trlTimeOut_S = 30;
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 500;
        /// <summary>
        /// 需求电流最少大于30A
        /// </summary>
        Double DemandCurrent = 60;

        public GB_RT_DC_ConnectionConfirm_WaveRecoder(int type)
        {
            TrialType = type;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void InitEquiMent()
        {


            SendNoticeToUIAndTxtFile("设备初始化中...");


            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);



            SetCPReresh();
        }


        public override void InitializeParams()
        {
            Init();

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


                    double CC1 = 0;
                    //string CC1Value2 = "0";


                    SendNoticeToUIAndTxtFile("发送CC1、CC2断线！");

                    List<bool> Ks = GetKStatus16_Charging_DC();
                    Ks[22] = false;
                    Ks[23] = false;
                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, Ks.ToArray());
                    Thread.Sleep(1000);

                    SendNoticeToUIAndTxtFile("读取CC1电压中");


                    Thread.Sleep(1000);
                    string CC1Value1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage.ToString("F2");
                    string CC2Value1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC2Voltage.ToString("F2");

                    Dictionary<int, string> dCC1Value1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dCC1Value1.Add(item, CC1Value1);
                    }
                    //var dImgs = ControlEquipMent.Oscillograph.OscillographSaveScreen();
                    ProcessDataTmp(dCC1Value1, "空闲未插枪，断线模拟", "CC1(V)", "5.2", "6.8");

                    Dictionary<int, string> dCC2Value1 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dCC2Value1.Add(item, CC2Value1);
                    }
                    ProcessDataTmp(dCC2Value1, "空闲未插枪，断线模拟", "CC2(V)", "11.2", "12.8");

                    SystemEvent.MessageInfo(true, "请按住枪锁不松开!");
                    int timeOut = 0;
                    while (timeOut < 100)
                    {
                        CC1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage;
                        if (CC1 >= 11.2 && CC1 <= 12.8)
                        {

                            break;

                        }
                        timeOut++;
                        Thread.Sleep(100);
                    }

                    CC1Value1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage.ToString("F2");
                    CC2Value1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC2Voltage.ToString("F2");

                    Dictionary<int, string> dCC1Value2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dCC1Value2.Add(item, CC1Value1);
                    }
                    ProcessDataTmp(dCC1Value2, "断线模拟并按下枪锁", "CC1(V)", "11.2", "12.8");
                    Dictionary<int, string> dCC2Value2 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dCC2Value2.Add(item, CC2Value1);
                    }
                    ProcessDataTmp(dCC2Value2, "断线模拟并按下枪锁", "CC2(V)", "11.2", "12.8");


                    SendNoticeToUIAndTxtFile("恢复CC1断线中");
                    Ks = GetKStatus16_Charging_DC();
                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, Ks.ToArray());//有时候无响应，这里发两次
                    Thread.Sleep(1000);
                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, 390, Ks.ToArray());
                    Thread.Sleep(1000);

                    SendNoticeToUIAndTxtFile("读取CC1和CC2电压中");
                    CC1Value1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage.ToString("F2");
                    CC2Value1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC2Voltage.ToString("F2");
                    Dictionary<int, string> dCC1Value3 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        int timeout = 30;
                        while (timeout-- > 0)
                        {
                            double cc1 = Convert.ToDouble(CC1Value1);
                            if (cc1 >= 5.2 && cc1 <= 6.8)
                                break;
                            CC1Value1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage.ToString("F2");
                            Thread.Sleep(200);
                        }
                        dCC1Value3.Add(item, CC1Value1);
                    }
                    ProcessDataTmp(dCC1Value3, "无断线按下枪锁", "CC1(V)", "5.2", "6.8");
                    Dictionary<int, string> dCC2Value3 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dCC2Value3.Add(item, CC2Value1);
                    }
                    ProcessDataTmp(dCC2Value3, "无断线按下枪锁", "CC2(V)", "5.2", "6.8");
                    SystemEvent.MessageInfo(false, "");     //关闭按住枪锁提示
                    Thread.Sleep(1000);


                    //这里需要启动导引，但不充电
                    ControlEquipMent.BMS.BMS_ON(lstIDs);
                    SystemEvent.MessageInfo(true, "请松开枪锁!");
                    Thread.Sleep(1000);
                    timeOut = 0;
                    while (timeOut < 100)
                    {
                        CC1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage;
                        if (CC1 >= 3.7 && CC1 <= 4.3)
                        {
                            break;

                        }
                        timeOut++;
                        Thread.Sleep(100);
                    }
                    SystemEvent.MessageInfo(false, "");

                    CC1Value1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC1Voltage.ToString("F2");
                    CC2Value1 = AllEquipStateData.DicBMS_DC_StateData.First().Value.CC2Voltage.ToString("F2");

                    Dictionary<int, string> dCC1Value4 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dCC1Value4.Add(item, CC1Value1);
                    }
                    ProcessDataTmp(dCC1Value4, "松开枪锁插枪", "CC1(V)", "3.65", "4.37");
                    Dictionary<int, string> dCC2Value4 = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {

                        dCC2Value4.Add(item, CC2Value1);
                    }
                    ProcessDataTmp(dCC2Value4, "松开枪锁插枪", "CC2(V)", "5.2", "6.8");


                    //SendNoticeToUIAndTxtFile("关闭负载中!");
                    //SetLoadDCOFF(testWorkParam.lstIDs);


                    //SendNoticeToUIAndTxtFile("关闭导引中!");

                    //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                }

            }
            catch (Exception ex) { Log.Log.LogException(ex); }

        }






        public override void ProcessData()
        {

        }

    }
}
