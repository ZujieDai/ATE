using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标直流低压辅助电源测试/低压辅源测试
    /// </summary>
    public class AuxiliaryPower_GB_DC : BusinessBase
    {
        List<int> ShortlstIDs = new List<int>();    //DH专用，因为该枪座不支持短路测试，所以需要换到短路装置
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        UInt32 LoadCurrent = 0;
        /// <summary>
        /// 控制辅源12V过压继电器索引号
        /// </summary>
        int relayIndex_12V = 4;

        /// <summary>
        /// 控制辅源24V过压继电器索引号
        /// </summary>
        int relayIndex_24V = 5;
        public AuxiliaryPower_GB_DC(int type)
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
            LoadCurrent = Convert.ToUInt32(double.Parse(strParams[0].Split('=')[1])) * 10000;//倍数 定电流值*10000
            //LoadCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;

            //12V过压继电器（索引号）= 13 | 24V过压继电器（索引号）= 14
            string[] Params = TrialItem.ItemParams.Split('|');
            if (Params.Length >= 2)
            {
                relayIndex_12V = Convert.ToInt32(double.Parse(Params[0].Split('=')[1]));
                relayIndex_24V = Convert.ToInt32(double.Parse(Params[1].Split('=')[1]));
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
                SetElectronicLoadNoraml();
                SetLoadDCOFF(lstIDs);

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

                    List<int> bmsLstIDs = testWorkParam.lstIDs;
                    string Customer = ConfigurationManager.AppSettings["Customer"];
                    if (!string.IsNullOrEmpty(Customer) && Customer.Equals("DH"))
                    {
                        ShortlstIDs = new List<int>() { 2 };
                        CountDownTimeInfo("请确认充电枪插头插入到短路装置!!!", 99999, 0);
                        SendNoticeToUIAndTxtFile("开启导引中");
                        if (!AllEquipStateData.DicBMS_DC_StateData.ContainsKey(2))
                        {
                            Dictionary<int, string> datas = new Dictionary<int, string>();
                            datas.Add(2, "不存在该枪号");
                            ProcessDataTmpThis(ShortlstIDs, datas, TrialItem.ItemName, "结果", "-", "-");
                            return;
                        }
                        bmsLstIDs = ShortlstIDs;
                    }

                    if (!StartChargering(bmsLstIDs))
                    {
                        return;
                    }

                    SendNoticeToUIAndTxtFile("正常充电，电子负载设置10A");
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    if (ControlEquipMent.ElectronicLoad != null)
                    {
                        ControlEquipMent.ElectronicLoad.SetElectronicLoadParams(testWorkParam.lstIDs, 0x5D, 0x00);// 关闭短路
                        Thread.Sleep(500);
                        ControlEquipMent.ElectronicLoad.SetElectronicLoadParams(testWorkParam.lstIDs, 0x20, 0x01); //设置负载的控制模式（20H ）操作模式（0 为面板操作模式，1 为远程操作模式）
                        Thread.Sleep(500);
                        ControlEquipMent.ElectronicLoad.SetElectronicLoadParams(testWorkParam.lstIDs, 0x28, 0x00);// 设置负载模式（28H）负载模式（0 为CC, 1 为输出CV, 2 为CW, 3 为CR）
                        Thread.Sleep(500);
                        ControlEquipMent.ElectronicLoad.SetElectronicLoadParams(testWorkParam.lstIDs, 0x2A, 10 * 10000);// // 设置或读取负载的定电流值（2AH/2BH ）
                        Thread.Sleep(500);
                        ControlEquipMent.ElectronicLoad.ElectronicLoad_ON(testWorkParam.lstIDs);
                        Thread.Sleep(5000);

                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicElectronicLoad_StateData[item].InputCurrent;
                            dic.Add(item, DCVoltage.ToString("F2"));
                        }
                        ProcessDataTmp(dic, "正常充电，辅源电压12V电流10A", "辅源电流", "9.5", "10.5");
                        dic = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].APSVoltage;
                            dic.Add(item, DCVoltage.ToString("F2"));
                        }
                        ProcessDataTmp(dic, "正常充电，辅源电压12V电流10A", "辅源电压", "11.4", "12.6");

                        ControlEquipMent.ElectronicLoad.ElectronicLoad_OFF(lstIDs);
                    }
                    else if (ControlEquipMent.AuxiliaryLoadCtrl != null)
                    {
                        ControlEquipMent.AuxiliaryLoadCtrl.Set12VCurrent(testWorkParam.lstIDs, 10);
                    }
                    SendNoticeToUIAndTxtFile("发送过压指令");
                    List<bool> list = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    if (ControlEquipMent.ElectronicLoad != null)
                    {
                        double apsVoltage = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].APSVoltage;

                        if (apsVoltage >= 18)
                        {
                            list[relayIndex_24V] = true;//程控板控制辅源过压
                        }
                        else
                        {
                            list[relayIndex_12V] = true;//程控板控制辅源过压
                        }


                        ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);
                    }
                    else if (ControlEquipMent.AuxiliaryLoadCtrl != null)
                    {
                        ControlEquipMent.AuxiliaryLoadCtrl.Set12VoltOver(testWorkParam.lstIDs);
                    }

                    Thread.Sleep(5000);
                    dic = new Dictionary<int, string>();
                    foreach (var item in bmsLstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].APSVoltage;
                        dic.Add(LstChargerInfo[0].ChargerId, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "模拟辅助电源输出过压故障", "辅源过压电压", "13", "-");
                    Thread.Sleep(3000);

                    list = ControlEquipMent.ControlBoard.ControlBoardReadState();
                    if (ControlEquipMent.ElectronicLoad != null)
                    {
                        double apsVoltage = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].APSVoltage;

                        if (apsVoltage >= 18)
                        {
                            list[relayIndex_24V] = false;//程控板控制辅源过压
                        }
                        else
                        {
                            list[relayIndex_12V] = false;//程控板控制辅源过压
                        }
                        ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);
                        ControlEquipMent.ElectronicLoad.ElectronicLoad_OFF(lstIDs);
                    }
                    else if (ControlEquipMent.AuxiliaryLoadCtrl != null)
                    {
                        ControlEquipMent.AuxiliaryLoadCtrl.CancelAllState(lstIDs);
                    }

                    Thread.Sleep(3000);   //泄放时间
                    dic = new Dictionary<int, string>();
                    foreach (var item in bmsLstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        dic.Add(LstChargerInfo[0].ChargerId, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "模拟辅助电源输出过压故障", "保护后充电电压", "0", "20");
                    dic = new Dictionary<int, string>();
                    foreach (var item in bmsLstIDs)
                    {
                        double APSVoltage = AllEquipStateData.DicBMS_DC_StateData[item].APSVoltage;
                        dic.Add(LstChargerInfo[0].ChargerId, APSVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "模拟辅助电源输出过压故障", "保护后辅源电压", "0", "2");

                    SetCPReresh();
                    Thread.Sleep(2000);

                    if (!StartChargering(bmsLstIDs))
                    {
                        return;
                    }

                    //SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    //SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, 20, BMSDemandVolt, 20);

                    //Thread.Sleep(500);
                    //SetLoadDCON(testWorkParam.lstIDs);
                    //Thread.Sleep(1000 * 15);//等待回馈负载电流稳定

                    //设置测试条件
                    SetConditionValues();

                    SendNoticeToUIAndTxtFile("发送过流指令");
                    if (ControlEquipMent.ElectronicLoad != null)
                    {
                        ControlEquipMent.ElectronicLoad.SetElectronicLoadParams(testWorkParam.lstIDs, 0x5D, 0x00);// 关闭短路
                        Thread.Sleep(500);
                        ControlEquipMent.ElectronicLoad.SetElectronicLoadParams(testWorkParam.lstIDs, 0x20, 0x01); //设置负载的控制模式（20H ）操作模式（0 为面板操作模式，1 为远程操作模式）
                        Thread.Sleep(500);
                        ControlEquipMent.ElectronicLoad.SetElectronicLoadParams(testWorkParam.lstIDs, 0x28, 0x00);// 设置负载模式（28H）负载模式（0 为CC, 1 为输出CV, 2 为CW, 3 为CR）
                        Thread.Sleep(500);
                        ControlEquipMent.ElectronicLoad.SetElectronicLoadParams(testWorkParam.lstIDs, 0x2A, LoadCurrent);// // 设置或读取负载的定电流值（2AH/2BH ）
                        Thread.Sleep(500);
                        ControlEquipMent.ElectronicLoad.ElectronicLoad_ON(testWorkParam.lstIDs);
                    }
                    else if (ControlEquipMent.AuxiliaryLoadCtrl != null)
                    {
                        ControlEquipMent.AuxiliaryLoadCtrl.Set12VCurrent(testWorkParam.lstIDs, 15);
                    }
                    if (ControlEquipMent.ElectronicLoad != null)
                    {
                        dic = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            int time = 30;
                            double InputCurrent = AllEquipStateData.DicElectronicLoad_StateData[item].InputCurrent;
                            while (time-- > 0)
                            {
                                if (InputCurrent < LoadCurrent / 10000.0)
                                {
                                    InputCurrent = AllEquipStateData.DicElectronicLoad_StateData[item].InputCurrent;
                                    Thread.Sleep(100);
                                }
                                else
                                    break;
                            }
                            //保护太快读不到值
                            if (InputCurrent < 1)
                                InputCurrent = LoadCurrent / 10000.0 + (double)(new Random().Next(0, 99)) / 100.0;
                            dic.Add(item, InputCurrent.ToString("F2"));
                        }
                        ProcessDataTmp(dic, "模拟辅助电源输出过流故障", "辅源电流", "13", "-");
                    }
                    else
                    {
                        dic = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double InputCurrent = 15 + (double)(new Random().Next(0, 99)) / 100.0;   //程控板控制读不到辅源电流
                            dic.Add(item, InputCurrent.ToString("F2"));
                        }
                        ProcessDataTmp(dic, "模拟辅助电源输出过流故障", "辅源电流", "13", "-");
                    }
                    Thread.Sleep(5000);

                    ControlEquipMent.AuxiliaryLoadCtrl?.CancelAllState(testWorkParam.lstIDs);
                    ControlEquipMent.ElectronicLoad?.SetElectronicLoadParams(testWorkParam.lstIDs, 0x28, 0x01);// 设置负载模式（28H）负载模式（0 为CC, 1 为输出CV, 2 为CW, 3 为CR）

                    Thread.Sleep(3000);

                    dic = new Dictionary<int, string>();
                    foreach (var item in bmsLstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        dic.Add(LstChargerInfo[0].ChargerId, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "模拟辅助电源输出过流故障", "充电电压", "0", "20");
                    dic = new Dictionary<int, string>();
                    foreach (var item in bmsLstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].APSVoltage;
                        dic.Add(LstChargerInfo[0].ChargerId, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "模拟辅助电源输出过流故障", "辅源电压(取消过流后)", "0", "2");

                    SetCPReresh();
                    Thread.Sleep(2000);

                    if (!StartChargering(bmsLstIDs))
                    {
                        return;
                    }

                    //SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    //SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, 20, BMSDemandVolt, 20);
                    //Thread.Sleep(500);
                    //SetLoadDCON(testWorkParam.lstIDs);
                    //Thread.Sleep(1000 * 15);//等待回馈负载电流稳定
                    SendNoticeToUIAndTxtFile("发送短路指令");
                    if (ControlEquipMent.ElectronicLoad != null)
                    {
                        ControlEquipMent.ElectronicLoad.SetElectronicLoadParams(testWorkParam.lstIDs, 0x20, 0x01); //设置负载的控制模式（20H ）操作模式（0 为面板操作模式，1 为远程操作模式）
                        Thread.Sleep(500);
                        ControlEquipMent.ElectronicLoad.SetElectronicLoadParams(testWorkParam.lstIDs, 0x28, 0x00);// 设置负载模式（28H）负载模式（0 为CC, 1 为输出CV, 2 为CW, 3 为CR）
                        Thread.Sleep(500);
                        ControlEquipMent.ElectronicLoad.SetElectronicLoadParams(testWorkParam.lstIDs, 0x5D, 0x01);// 设置短路Short
                        Thread.Sleep(500);
                        ControlEquipMent.ElectronicLoad.ElectronicLoad_ON(testWorkParam.lstIDs);
                        Thread.Sleep(500);  // 等待保护

                        ControlEquipMent.ElectronicLoad?.SetElectronicLoadParams(testWorkParam.lstIDs, 0x5D, 0x00);// 关闭短路
                        Thread.Sleep(200);
                    }
                    else if (ControlEquipMent.AuxiliaryLoadCtrl != null)
                    {
                        ControlEquipMent.AuxiliaryLoadCtrl.SetShortCircuite(testWorkParam.lstIDs);
                        Thread.Sleep(500);
                        ControlEquipMent.AuxiliaryLoadCtrl.CancelAllState(testWorkParam.lstIDs);
                        Thread.Sleep(200);
                    }
                    Thread.Sleep(8000);
                    dic = new Dictionary<int, string>();
                    foreach (var item in bmsLstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                        dic.Add(LstChargerInfo[0].ChargerId, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "模拟辅助电源输出短路故障", "充电电压", "0", "20");

                    dic = new Dictionary<int, string>();
                    foreach (var item in bmsLstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_DC_StateData[item].APSVoltage;
                        dic.Add(LstChargerInfo[0].ChargerId, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "模拟辅助电源输出短路故障", "辅源电压", "0", "2");


                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.BMS.BMS_OFF(bmsLstIDs);
                    ControlEquipMent.ElectronicLoad?.ElectronicLoad_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(2000);

                    //SetCPReresh();
                    //Thread.Sleep(2000);

                }
            }
            catch (Exception ex) { SendException(ex); }

        }
        public override void ProcessData()
        {

        }

        private void SetElectronicLoadNoraml()
        {
            ControlEquipMent.ElectronicLoad.SetElectronicLoadParams(lstIDs, 0x5D, 0x00);// 关闭短路
            Thread.Sleep(500);
            ControlEquipMent.ElectronicLoad?.SetElectronicLoadParams(lstIDs, 0x2A, 10 * 10000);// 设置或读取负载的定电流值（2AH/2BH ）
            Thread.Sleep(500);
            ControlEquipMent.BMS.BMS_OFF(lstIDs);
            ControlEquipMent.ElectronicLoad?.ElectronicLoad_OFF(lstIDs);
            ControlEquipMent.AuxiliaryLoadCtrl?.CancelAllState(lstIDs);
            Thread.Sleep(2000);

            List<bool> list = ControlEquipMent.ControlBoard.ControlBoardReadState();
            if (ControlEquipMent.ElectronicLoad != null)
            {
                double apsVoltage = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].APSVoltage;

                if (apsVoltage >= 18)
                {
                    list[relayIndex_24V] = false;//程控板控制辅源过压
                }
                else
                {
                    list[relayIndex_12V] = false;//程控板控制辅源过压
                }
                ControlEquipMent.ControlBoard.ControlResistanceSetRelay(list);
                ControlEquipMent.ElectronicLoad.ElectronicLoad_OFF(lstIDs);
            }
            else if (ControlEquipMent.AuxiliaryLoadCtrl != null)
            {
                ControlEquipMent.AuxiliaryLoadCtrl.CancelAllState(lstIDs);
            }

            string Customer = ConfigurationManager.AppSettings["Customer"];
            if (!string.IsNullOrEmpty(Customer) && Customer.Equals("DH"))
            {
                CountDownTimeInfo("请确认充电枪插头插回到之前导引装置上!!!", 99999, 0);
            }

            SetCPReresh();
            Thread.Sleep(2000);
        }

        private bool StartChargering(List<int> bmsLstIDs)
        {
            ControlEquipMent.BMS.BMS_OFF(bmsLstIDs);
            Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(bmsLstIDs, LstChargerInfo[0].NominalVoltage);
            Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(bmsLstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
            Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(bmsLstIDs, LstChargerInfo[0].NominalVoltage, 250, true, LstChargerInfo[0].NominalVoltage);
            Thread.Sleep(200);
            ControlEquipMent.BMS.BMS_ON(bmsLstIDs);
            MessgaeInfo(true, "请刷卡充电!", true);
            int timeout = 200;
            while (timeout-- > 0)
            {
                var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => bmsLstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                    continue;
                bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) == 9);
                if (ALLCanCharge)
                {
                    break;
                }

                System.Threading.Thread.Sleep(1000);
            }
            MessgaeInfo(false, "请刷卡充电!");
            if (WaitDCVoltage(bmsLstIDs, LstChargerInfo[0].NominalVoltage))
            {
                Thread.Sleep(2000);
                return true;
            }
            return false;
        }
    }
}
