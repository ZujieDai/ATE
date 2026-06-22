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
    /// 企标研测直流：蓄电池反接试验
    /// </summary>
    public class QB_RT_DC_BatteryReverseConnect : BusinessBase
    {
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 750;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储

        public QB_RT_DC_BatteryReverseConnect(int trialType) { TrialType = trialType; }

        public override void InitializeParams()
        {
            Init();
            Data_Tmp = new Dictionary<int, string>();//临时测试数据
            OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
            dImgs = new Dictionary<int, string>();//图片存储
            string[] strParams = TrialItem.ResultParams.Split('|');
            //disConnectionTime = strParams[0].Split('=')[1];
            if (strParams.Length >= 1 && strParams[0].Split('=').Length > 1)
            {
                DemandVoltage = double.Parse(strParams[0].Split('=')[1]);
            }
        }

        public override void InitEquiMent()
        {

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
                SendNoticeToUIAndTxtFile("恢复蓄电池电压反接故障");
                List<bool> Ks = GetKStatus16_Charging_DC();
                Ks[0] = false;
                Ks[16] = false;
                Ks[26] = false;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());
                ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                Thread.Sleep(8000); //等待充电桩关闭
                ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                Thread.Sleep(3000);
                SetCPReresh();

                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }


        /// <summary>
        /// 测试流程
        /// </summary>
        public void StartItemFlow()
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
                if (_StopWatch.ElapsedMilliseconds / 1000 > 10)
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

                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }


                //设置测试条件
                SetConditionValues();

                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(500);

                SendNoticeToUIAndTxtFile("模拟蓄电池电压反接故障");
                //模拟蓄电池反接
                List<bool> Ks = GetKStatus16_Charging_DC();

                Ks[16] = true;
                Ks[26] = true;//26输出过压控制，XJ现场不输出电压无法检测到电池反接
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());
                Thread.Sleep(300);
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());
                Thread.Sleep(300);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage);
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, DemandVoltage, MaxAllowChargeCurrent);
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, MaxAllowChargeCurrent, true, 390);
                Thread.Sleep(500);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                Thread.Sleep(500);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);


                double cc1Value = 0;
                MessgaeInfo(true, "请刷卡充电!", true);
                while (true)
                {
                    int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState);
                    if (state > 2 && state <= 9)
                    {
                        CountDownTimeInfo("请确认充电中充电枪插头可靠被锁止。\r\n(注:勾选上为可靠锁止)", 5, 2);
                        ProcessDataConnect("绝缘检测阶段前", "是否可靠锁止");

                        MessgaeInfo(false, "请刷卡充电!");
                        break;
                    }
                    System.Threading.Thread.Sleep(100);
                }
                MessgaeInfo(false, "请刷卡充电!");
                int timeout = 100;
                while (timeout-- > 0)
                {
                    //int state = ChangeBMSChargeStatus(AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingState);
                    //if (state > 3)
                    //{
                    //if (state < 5)
                    //{
                    double newCC1Value = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo.First().ChargerId].CC1Voltage;
                    cc1Value = newCC1Value >= 3.6 && newCC1Value <= 4.4 ? newCC1Value : cc1Value;
                    //    }
                    //    break;
                    //}

                    System.Threading.Thread.Sleep(100);
                }
                d1 = new Dictionary<int, string>();
                foreach (var item in testWorkParam.lstIDs)
                {
                    d1.Add(item, cc1Value.ToString("F2"));
                }
                ProcessDataTmp(d1, "绝缘检测阶段前", "CC1电压(V)", "-", "-");

                Thread.Sleep(5 * 1000);

                CountDownTimeInfo("请确认充电中充电枪插头是否正常解锁。\r\n(注:勾选上为正常解锁)", 20, 2);
                ProcessDataConnect("充电机保护后", "是否正常解锁");

                d1 = new Dictionary<int, string>();
                cc1Value = AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo.First().ChargerId].CC1Voltage;
                foreach (var item in testWorkParam.lstIDs)
                {
                    d1.Add(item, cc1Value.ToString("F2"));
                }
                ProcessDataTmp(d1, "充电机保护后", "CC1电压(V)", "-", "-");

                CountDownTimeInfo("请判定充电桩充电状态，蓄电池反接后充电桩应无法充电。\r\n 请判断桩是否【无法正常充电】。\r\n勾选代表无法充电", 30, 2);
                ProcessData();

                CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有故障报警", 30, 2);
                ProcessDataConnect("应发出告警提示", "是否有告警提示");
            }
        }



        public override void ProcessData()
        {
            try
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
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|反接后是否能充电|-|-|不能";
                    }
                    else
                    {
                        LstTrialData[k].TrialResult = EmTrialResult.Fail;
                        LstTrialData[k].ExtentData = TrialItem.ItemName + "|反接后是否能充电|-|-|能";
                    }
                    LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                    //界面展示的数据项格式
                    //状态|测试结果     

                    LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                    LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                    SendTrialDataToUI(LstTrialData[k]);
                    //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                    SaveTrialData(LstTrialData[k]);


                }
            }
            catch (Exception ex) { SendException(ex); }
        }


    }
}
