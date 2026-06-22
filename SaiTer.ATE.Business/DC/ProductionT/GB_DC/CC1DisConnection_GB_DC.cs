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
    /// 国标直流  连接检测信号断开（CC1断线测试）
    /// </summary>
    public class CC1DisConnection_GB_DC : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        string disConnectionTime = "3000";//断线上限时间(mS)=3000
        public CC1DisConnection_GB_DC(int trialType) { TrialType = trialType; }

        public override void InitializeParams()
        {
            Init();
            Data_Tmp = new Dictionary<int, string>();//临时测试数据
            OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
            dImgs = new Dictionary<int, string>();//图片存储
            string[] strParams = TrialItem.ResultParams.Split('|');
            disConnectionTime = strParams[0].Split('=')[1];
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
                //List<bool> Ks = GetKStatus16_Charging();
                //// Ks[5] = false;
                //Ks[0] = false;
                //ControlEquipMent.BMS.BMS_SetKState(lstIDs, Ks);

                var lstRelay = ControlEquipMent.ControlBoard.ControlBoardReadState(lstIDs);
                string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                if (Customer != null && Customer.Contains("YTZL_ACDC"))
                {
                    lstRelay[4] = false;
                    lstRelay[9] = false;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstRelay);
                }

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



                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }

                //启动示波器
                //ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                if (!CheckSwipingCard(testWorkParam.lstIDs, MaxAllowChargeVoltage, RatedCurrent))
                {
                    return;
                }


                var lstRelay = ControlEquipMent.ControlBoard.ControlBoardReadState(lstIDs);
                string Customer = ConfigurationManager.AppSettings["Customer"].ToString().ToUpper();
                if (Customer != null && Customer.Contains("YTZL_ACDC"))
                {
                    lstRelay[4] = true;
                    lstRelay[9] = true;
                    ControlEquipMent.ControlBoard.ControlResistanceSetRelay(lstRelay);
                }

                double DemandCurrent = RatedCurrent - 10 > 50 ? 50 : RatedCurrent - 10;
                SendNoticeToUIAndTxtFile("带载中...");
                SetLoadPara(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage - 20, DemandCurrent, LstChargerInfo[0].NominalVoltage, DemandCurrent);
                Thread.Sleep(2000);
                SetLoadDCON(testWorkParam.lstIDs);

                int waitTime = 50;
                //初始化示波器
                SendNoticeToUIAndTxtFile("初始化示波器1、2、3、4号通道");
                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, false, "DC", "20", Channel1, "Output_Voltage_DC", "1", "V", false, "300", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20", Channel2, "Output_Current_DC", "1", "A", false, "10", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20", Channel3, "Input_Current_AC", "1", "V", false, "10", "2.5");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "AC", "20", Channel4, "Input_Voltage_AC", "1", "A", false, "10", "0");
                Thread.Sleep(waitTime);

                //设置时基400ms
                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "100", "0");
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_StorageDepth(testWorkParam.lstIDs, "250k");


                //添加测量值
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MEAN", 1);//1通道平均值
                Thread.Sleep(waitTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "MEAN", 2);//2通道平均值
                Thread.Sleep(waitTime);

                //设置触发
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "100", 2, "10", "Single");
                Thread.Sleep(waitTime);

                //启动示波器
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                Thread.Sleep(1000);

                //设置测试条件
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    int key = testWorkParam.lstIDs[i];
                    if (AllEquipStateData.DicACSource_StateData.Count == 1)
                    {
                        key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                    }
                    d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                    d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                }
                SetConditionValue("供电电压(V)", d1);
                SetConditionValue("供电频率(Hz)", d2);

                //模拟CC1断线
                List<bool> Ks = GetKStatus16_Charging_DC();

                Ks[22] = false;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());
             
                //读取分析数据
                Thread.Sleep(1000);
                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, false);
                //CPPWMUpTime(testWorkParam.lstIDs, 1, LstChargerInfo[0].NominalVoltage - 30, 1);
                ACDownTime(testWorkParam.lstIDs,2, DemandCurrent - 10, 1);//AC回路动作时间
                ACDownTime(testWorkParam.lstIDs, 2, 7, 2);//AC回路动作时间

                //读取卡点时间
                ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 2, ref OscTime_Tmp);
                Data_Tmp = GetOSCTime(OscTime_Tmp);
                Dictionary<int, string> dd = new Dictionary<int, string>();
                foreach (var itmp in Data_Tmp)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                }
                dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                ProcessDataTmp(dd, "连接检测信号断开", "K1K2断开时间(ms)", "0", disConnectionTime, dImgs);

                //断线后的电压
                Data_Tmp = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString());
                }
                ProcessDataTmp(Data_Tmp, "连接检测信号断开", "断线后输出电压(V)", "0", "20");

                Ks[22] = true;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());


            }
        }



        public override void ProcessData()
        {

        }


    }
}
