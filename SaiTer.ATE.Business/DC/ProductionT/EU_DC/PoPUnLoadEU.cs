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
    /// 突卸载测试欧标
    /// </summary>
    public class PoPUnLoadEU : BusinessBase
    {
        public PoPUnLoadEU(int type)
        {
            TrialType = type;
        }
        double BMSDemandVolt = 0;//额定电压
        double ResiLoadCurrent = 0;//额定电流
        int trlTimeOut_S = 0;
        double DemandVoltage = 750;
        double DemandCurrent = 60;

        double Criterion = 110; //判断准则(%)

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

                    //ControlEquipMent.BMS.BMS_ON(lstIDs);
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, 390);


                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);

                    Thread.Sleep(10*1000);//

                    SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");

                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent + 5, DemandVoltage-5, DemandCurrent);
                    SetLoadDCON(testWorkParam.lstIDs);


                    Double DemandCurrent2 = MaxOutputPower / MaxAllowChargeVoltage;
                    DemandCurrent2 = DemandCurrent * 0.1;






                    Thread.Sleep(5 * 1000);
                    SendNoticeToUIAndTxtFile("正在设置示波器触发中...");
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(lstIDs, 0, "RISE", "DC", "", "", 1,(DemandVoltage+20).ToString(), "Single");
                    Thread.Sleep(10 * 1000);
                    SendNoticeToUIAndTxtFile("设备改变负载至10%，请稍候...");




                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent2 + 5, DemandVoltage-5, DemandCurrent2);






                    SendNoticeToUIAndTxtFile("判断结果中...");
                    CountDownTimeInfo("判断过冲延时", 20, 0);

                    Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);

                    Dictionary<int,string>  Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "MAX", 1);

             


                    Double MAXVoDC = (MaxAllowChargeVoltage * Criterion) / 100+ MaxAllowChargeVoltage;






                    ProcessDataTmp(Data_Tmp, "电压卸载中", "电压过冲值(V)", "0", MAXVoDC.ToString(),dImgs);


            
                 

                    SendNoticeToUIAndTxtFile("关闭负载中...");

                    SetLoadDCOFF(testWorkParam.lstIDs);




                }
            }
            catch (Exception ex) { SendException(ex); }


        }


        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
            OscilloscopeInputCurrentOvershootE();
        }
        /// <summary>
        /// 突卸载测试
        /// </summary>
        public void OscilloscopeInputCurrentOvershootE()
        {


            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 1, true, "DC", "20M", Channel1, "Output_DC_V", "1M", "V", false, "300", "0");


            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 2, true, "DC", "20M",Channel2, "Output_DC_I", "1M", "A", false, "50", "-2");//通道2设置

            ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(lstIDs, 3, false, "AC", "20M", Channel3, "Input_AC_I", "1M", "A", false, "100", "0");//通道2设置


            ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(lstIDs, true, "1000", "0");//设置滚动，时基和触发延时



            ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(lstIDs);//初始化测量

            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "RMS", 1);
            System.Threading.Thread.Sleep(100);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "MAX", 1);
            System.Threading.Thread.Sleep(100);
            ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(lstIDs, "TOP", 1);
            System.Threading.Thread.Sleep(100);
            ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(lstIDs,true);

        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
            if (strParams.Length >= 1)
            {
                //DemandVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                //DemandCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
                Criterion = Convert.ToDouble(strParams[0].Split('=')[1]);
            }
            DemandVoltage = LstChargerInfo[0].NominalVoltage;
            DemandCurrent = MaxOutputPower / LstChargerInfo[0].NominalVoltage * 1000;
        }

        public override void ProcessData()
        {

        }


    }
}
