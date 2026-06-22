using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.WaveRecoder;
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
    /// 国标研测直流：连接检测信号断开试验(录波板)
    /// </summary>
    public class GB_RT_DC_CC1DisConnection_WaveRecoder : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储
        string disConnectionTime = "3000";//断线上限时间(mS)=3000
        public GB_RT_DC_CC1DisConnection_WaveRecoder(int trialType) { TrialType = trialType; }

        public override void InitializeParams()
        {
            Data_Tmp = new Dictionary<int, string>();//临时测试数据
            OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
            dImgs = new Dictionary<int, string>();//图片存储
            string[] strParams = TrialItem.ResultParams.Split('|');
            disConnectionTime = strParams[0].Split('=')[1];
        }

        public override void InitEquiMent()
        {
            ControlEquipMent.WaveRecoderCtrl.WaveRecoder_SetSamplingRate(testWorkParam.lstIDs, 1);//设置录波板采样率为1k/s
        }

        public override void ExecuteMethod()
        {
            try
            {
                SetCPReresh();
                Init();
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
                SetLoadDCOFF(testWorkParam.lstIDs);

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
                //设置测试条件
                SetConditionValues();



                //ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
                //Thread.Sleep(2000);


                //Thread.Sleep(5000);
                //ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                //Thread.Sleep(1000);

                //WaveData r1 = new WaveData();
                //WaveData r2 = new WaveData();
                //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref r1, "ss1");
                //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref r2, "998");

                //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, 2000);//设置光标1
                //ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, 2200);//设置光标2
                ////Dictionary<int, double> dTime = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs);
                //Dictionary<int, string> gg = new Dictionary<int, string>();
                //gg.Add(testWorkParam.lstIDs[0], (10).ToString());
                //Dictionary<int, string> dImgsaa = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
                //ProcessDataTmp(gg, "连接检测信号断开", "K1K2断开时间(ms)", "0", disConnectionTime, dImgsaa);
                //return;



                //启动示波器
                //ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                if (!CheckSwipingCard(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 20))
                {
                    return;
                }


                SendNoticeToUIAndTxtFile("带载中...");
                SetLoadPara(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage - 20, 25, LstChargerInfo[0].NominalVoltage, 20);
                Thread.Sleep(2000);
                SetLoadDCON(testWorkParam.lstIDs);
                WaitDCCurrent(testWorkParam.lstIDs, 20);
                Thread.Sleep(3000);



                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Start(testWorkParam.lstIDs);
                Thread.Sleep(2000);

                //模拟CC1断线
                List<bool> Ks = GetKStatus16_Charging_DC();

                Ks[22] = false;
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, Ks.ToArray());

                Thread.Sleep(5000);
                ControlEquipMent.WaveRecoderCtrl.WaveRecoder_Stop(testWorkParam.lstIDs);
                Thread.Sleep(1000);


                //读取录波板数据
                double Time_CC1 = 0;
                double Time_K1K2 = 0;
                WaveData CH_CC1 = new WaveData();
                WaveData CH_K1K2 = new WaveData();
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 4, ref CH_CC1, "CC1");
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_ReadChannelData(testWorkParam.lstIDs, 8, ref CH_K1K2, "K1K2");
                DataAnalysis_WaveRecoder.GetDCSingleTime(CH_CC1, true, 5, ref Time_CC1);
                double K1K2_Tmp = DataAnalysis_WaveRecoder.GetWavePointVave(CH_K1K2, 5);
                if (K1K2_Tmp > 2)
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, false, 6, ref Time_K1K2);
                }
                else
                {
                    DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref Time_K1K2);
                }
                //DataAnalysis_WaveRecoder.GetDCSingleTime(CH_K1K2, true, 6, ref Time_K1K2);
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 1, Time_CC1);//设置光标1
                ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_SetCursor(testWorkParam.lstIDs, 2, Time_K1K2);//设置光标2
                double Time_Stop = Math.Abs(Time_CC1 - Time_K1K2);


                //读取卡点时间
                Dictionary<int, double> dTime = ControlEquipMent.WaveRecoderCtrl?.WaveRecoder_GetCursorData(testWorkParam.lstIDs);
                Dictionary<int, string> dd = new Dictionary<int, string>();
                foreach (var itmp in dTime)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                }
                Dictionary<int, string> dImgs = ControlEquipMent.WaveRecoderCtrl?.WaveRecoderSaveScreen(testWorkParam.lstIDs);//录波板截图
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
