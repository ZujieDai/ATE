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
    /// 急停欧标
    /// </summary>
    public class CrashStopEU : BusinessBase
    {
        public CrashStopEU(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;//测试时间
        double BMSDemandVolt = 0;//额定电压
        double ResiLoadCurrent = 0;//额定电流
        double ExceedBattery = 390;//超过的电压值
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


                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 500, 40, true, LstChargerInfo[0].NominalVoltage);
                    //Thread.Sleep(10*1000);//

                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, 500, LstChargerInfo[0].ChargerType, 0);

                    Thread.Sleep(2000);//
                    SetLoadPara(testWorkParam.lstIDs,480, 45, 495,40);
                    SetLoadDCON(testWorkParam.lstIDs);
                    ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs,0, "FALL", "DC", "", "", 1, (250).ToString(), "Single");
                    Thread.Sleep(2000);//
                    CountDownTimeInfo("请按下急停后点击确认", 999, 1);
                    SendNoticeToUIAndTxtFile("等待触发中...");

                    CountDownTimeInfo("判断触发延时", 10, 0);




                    SendNoticeToUIAndTxtFile("判断结果中...");
                    CountDownTimeInfo("判断充电延时", 5, 0);





                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }


                    ProcessDataTmp(dic, "急停", "充电电压(V)", "0", "20");



                    OscilloscopeCursorPosition_CrashStop(2,40);


                    CountDownTimeInfo("请恢复急停后点击确认", 999, 1);


                    SendNoticeToUIAndTxtFile("恢复互操作中...");
                    SetCPRersh_EUDC();

                    SendNoticeToUIAndTxtFile("关闭负载中...");

                    SetLoadDCOFF(testWorkParam.lstIDs);



                }
            }
            catch (Exception ex) { SendException(ex); }


        }
        public void OscilloscopeCursorPosition_CrashStop(int chnelNum, double tCompareValue)
        {
            try
            {

                Dictionary<int, double[]> datas = ControlEquipMent.Oscilloscope.OscilloscopeCursorData(testWorkParam.lstIDs, chnelNum);

                Dictionary<int, double> Position = new Dictionary<int, double>();
                ControlEquipMent.Oscilloscope.Oscilloscope_Points_Single(testWorkParam.lstIDs, datas, 20, 1, false, false, ref Position);


                ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(testWorkParam.lstIDs, 1, Position, true);


                Dictionary<int, double> Position2 = new Dictionary<int, double>();
                ControlEquipMent.Oscilloscope.Oscilloscope_Points_Single(testWorkParam.lstIDs, datas, 5, 1, false, false, ref Position2);


                ControlEquipMent.Oscilloscope.Oscilloscope_CursorPosition_X_Single(testWorkParam.lstIDs, 1, Position2, false);

                Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();
                ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, chnelNum, ref OscTime_Tmp);

                Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();

                Data_Tmp = GetOSCTime(OscTime_Tmp);
                Dictionary<int, string>  dd = new Dictionary<int, string>();
                foreach (var itmp in Data_Tmp)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
                }
                Dictionary<int, string> dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
                ProcessDataTmp(dd, "急停", "急停下降时间(ms)", "0", "3000", dImgs);


            }
            catch
            {


            }


        }
        /// <summary>
        /// 获取示波器光标之间的时间差
        /// </summary>
        /// <param name="dtmp"></param>
        /// <returns></returns>
        public Dictionary<int, string> GetOSCTime(Dictionary<int, double[]> dtmp)
        {

            Dictionary<int, string> ds = new Dictionary<int, string>();
            try
            {
                foreach (var item in dtmp)
                {
                    if (item.Value != null)
                    {
                        ds.Add(item.Key, Math.Abs(item.Value[0]).ToString());
                    }

                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }

            return ds;
        }
        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
            OscilloscopeCrashStop();
        }
        /// <summary>
        /// 急停输出电流停止速率
        /// </summary>
        public void OscilloscopeCrashStop()
        {

            try
            {

         
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20M", Channel1, "Output_DC_V", "1M", "V", false, "300", "0");//通道1设置2
                
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, true, "DC", "20M", Channel2, "Output_DC_I", "1M", "A", false, "50", "-1");//通道2设置
                
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, false, "AC", "20M", Channel3, "Input_AC_I", "1M", "A", false, "100", "0");//通道2设置
                

                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "500", "0");//设置滚动，时基和触发延时

                


                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                


            }
            catch
            {
                System.Threading.Thread.Sleep(1000);
    
            }



        }
        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;


        }

        public override void ProcessData()
        {

        }


    }
}
