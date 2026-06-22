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
    /// 企标研测直流：充电设定方式检查
    /// </summary>
    public class QB_RT_DC_SlowStart : BusinessBase
    {
        int trlTimeOut_S = 30;
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        double BMSDemandVolt = 0;
        double BMSDemandCurrent = 30;
        public QB_RT_DC_SlowStart(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            Data_Tmp = new Dictionary<int, string>();//临时测试数据
            OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
            dImgs = new Dictionary<int, string>();//图片存储
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            BMSDemandCurrent = LstChargerInfo[0].NominalCurrent;
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
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    //添加测试条件
                    SetConditionValues();

                    int waitTime = 50;
                    //初始化示波器
                    SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "output_DC_V", "1", "V", false, "200", "-2");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "A", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "DC", "20", Channel3, "CP_Voltage", "50", "V", false, "5", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");
                    //设置时基
                    ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "1000", "0");
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Time(testWorkParam.lstIDs, 1, 0, false);//
                    Thread.Sleep(waitTime);
                    ControlEquipMent.Oscilloscope.Oscilloscope_CursorsSet(testWorkParam.lstIDs, "X");
                    Thread.Sleep(waitTime);
                    //启动示波器
                    ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                    Thread.Sleep(1000);
                    //设置触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "RISE", "DC", "EDGE", "40", 1, ((BMSDemandVolt + MinAllowChargeVoltage) / 2).ToString(), "Auto");//下降边沿触发，单次文档上未60V
                    Thread.Sleep(waitTime);


                    SetCPReresh();  // 模拟插拔枪
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ControlEquipMent.BMS.SetParameter(lstIDs, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 250, true, LstChargerInfo[0].NominalVoltage);
                    Thread.Sleep(200);
                    ControlEquipMent.BMS.BMS_ON(lstIDs);
                    //ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");


                    MessgaeInfo(true, "请刷卡充电!", true);
                    int timeout = 3000;
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) >= 3 && ChangeBMSChargeStatus(c.Value.ChargingState) <= 9);
                        if (ALLCanCharge && AllEquipStateData.DicBMS_DC_StateData[LstChargerInfo[0].ChargerId].ChargingVoltage > 380)   //避免提前触发
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(100);
                    }
                    MessgaeInfo(false, "");
                    // 等待进入充电状态
                    //Thread.Sleep(5500);     //避免提前触发
                    ControlEquipMent.Oscilloscope.Oscilloscope_TriggerTypeSet(testWorkParam.lstIDs, "Single");
                    timeout = 60;
                    MessgaeInfo(true, "等待充电中...", true);
                    while (timeout-- > 0)
                    {
                        var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                        if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                            continue;
                        bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) == 9);
                        if (ALLCanCharge)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }
                    MessgaeInfo(false, "");

                    Thread.Sleep(12000);//有的模块启动较慢，这里加长一点
                    SendNoticeToUIAndTxtFile("回读示波器数据并计算结果");


                    ACUpTime(testWorkParam.lstIDs, 1, 30, 1);
                    ACUpTime(testWorkParam.lstIDs, 1, BMSDemandVolt - 20, 2);

                    ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);//读取卡点时间
                    Data_Tmp = GetOSCTime(OscTime_Tmp);
                    Dictionary<int, string> dd = new Dictionary<int, string>();
                    foreach (var itmp in Data_Tmp)
                    {
                        dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                    }
                    dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                    ProcessDataTmp(dd, "缓启动", "直流输出上升时间(ms)", "1000", "8000", dImgs);
                }
            }
            catch (Exception ex) { SendException(ex); }
        }

        public override void ProcessData()
        {

        }
    }
}
