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
    /// 国标直流  蓄电池反接
    /// </summary>
    public class BatteryReverseConnect_GB_DC : BusinessBase
    {
        /// <summary>
        /// 需求电压
        /// </summary>
        Double DemandVoltage = 750;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        string disConnectionTime = "3000";//断线上限时间(mS)=3000
        public BatteryReverseConnect_GB_DC(int trialType) { TrialType = trialType; }

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
            else
                DemandVoltage = MaxAllowChargeVoltage;
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
                ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, BatteryVoltage, Ks.ToArray());
                ControlEquipMent.ACSource.ACSource_OFF(lstIDs);
                Thread.Sleep(8000); //等待充电桩关闭
                ControlEquipMent.ACSource.ACSource_ON(lstIDs);
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

                SendNoticeToUIAndTxtFile("模拟蓄电池电压反接故障");
                //模拟蓄电池反接
                List<bool> Ks = GetKStatus16_Charging_DC();

                if (DemandVoltage < 390 && !(Customer != null && Customer.Contains("DH")))
                    BatteryVoltage = DemandVoltage - 10;
                else
                    BatteryVoltage = 390;

                Ks[16] = true;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, BatteryVoltage, Ks.ToArray());
                Thread.Sleep(300);
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, BatteryVoltage, Ks.ToArray());
                Thread.Sleep(300);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage);
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BatteryVoltage, DemandVoltage, MaxAllowChargeCurrent);
                Thread.Sleep(500);
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, MaxAllowChargeCurrent, true, DemandVoltage);
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

                CountDownTimeInfo("请尝试给桩充电，蓄电池反接后充电桩应无法充电。\r\n 请判断桩是否【无法正常充电】。\r\n勾选代表无法充电", 300, 2);
                ProcessData();

                CountDownTimeInfo("请检查充电桩是否有故障报警!\r\n注：勾选上为有故障报警", 999, 2);
                ProcessDataConnect("应发出告警提示", "是否有告警提示");

                //if (!CheckSwipingCard(testWorkParam.lstIDs))
                //{
                //    return;
                //}
                #region =====  发送蓄电池反接指令，再提示上电。 能上电FAIL，不能上电PASS===
                /*
                int waitTime = 50;
                //初始化示波器
                SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", "500", "Output_Voltage_DC", "1", "V", false, "300", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", "400", "Output_Current_DC", "1", "A", false, "50", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20", "400", "Input_Current_AC", "1", "V", false, "10", "2.5");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "AC", "20", "500", "Input_Voltage_AC", "1", "A", false, "10", "0");
                Thread.Sleep(waitTime);

                //设置时基400ms
                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "1", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");


                //添加测量值
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MEAN", 1);//1通道平均值
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MEAN", 2);//2通道平均值
                Thread.Sleep(waitTime);



                //设置触发
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "100", 1, "600", "Single");
                Thread.Sleep(waitTime);

                //启动示波器
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                */
                #endregion

                /* 

                //读取分析数据
                Thread.Sleep(1000);
                //CPPWMUpTime(testWorkParam.lstIDs, 1, LstChargerInfo[0].NominalVoltage - 30, 1);
                ACDownTime(testWorkParam.lstIDs, 1, LstChargerInfo[0].NominalVoltage - 50, 1);//AC回路动作时间
                ACDownTime(testWorkParam.lstIDs, 1, 20, 2);//AC回路动作时间

                //读取卡点时间
                ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);
                Data_Tmp = GetOSCTime(OscTime_Tmp);
                Dictionary<int, string> dd = new Dictionary<int, string>();
                foreach (var itmp in Data_Tmp)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                }
                dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                ProcessDataTmp(dd, "蓄电池反接", "K1K2断开时间(ms)", "0", disConnectionTime, dImgs);

                //断线后的电压
                Data_Tmp = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString());
                }
                ProcessDataTmp(Data_Tmp, "蓄电池反接", "反接后输出电压(V)", "0", "20");
                */
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
