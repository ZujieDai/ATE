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
    /// 欧标研测交流：可选测试支持电网的电动汽车充电设备
    /// </summary>
    public class CCS2_RT_AC_EquipmentThatSupportGridTest : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储

        public CCS2_RT_AC_EquipmentThatSupportGridTest(int trialType) { TrialType = trialType; }



        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
        }

        public override void ExecuteMethod()
        {
            try
            {
                Init();
                SetCPReresh();
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
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(lstIDs);
                ControlEquipMent.BMS.BMS_SetResistance(lstIDs, 1300, 2740);


                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }

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
                                //测试时间|电压|电流|CP电压|CP频率|CP占空比
                                LstTrialData[i].ExtentData = DateTime.Now.ToString()
                                    + "|" + ""
                                    + "|" + ""
                                    + "|" + ""
                                    + "|" + ""
                                    + "|" + "";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                //开始检测流程
                if (testWorkParam.lstIDs.Count <= 0)
                {
                    return;
                }
                SetConditionValues();

                try
                {
                    //ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 909, 1870);
                    //TrialMethon($"R2R3最小电阻", -0.8);

                    //ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 1723, 4610);
                    //TrialMethon($"R2R3最大电阻", 0.8);
                    TrialMethon(0);
                }
                catch (Exception ex)
                {
                    SendException(ex);
                }
            }
        }

        private void TrialMethon(double diff)
        {
            //1.1 --> 3.1 --> 4 --> 9.1 --> 10.1 --> 8.2 --> 3.1 --> 4 --> 7 --> 8.1 --> 2.1--> 9.3
            //断开CP,S2
            List<bool> Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = false;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);

            //GetResistanceValues("A1->B1(1.1)");

            #region A1->B1(1.1)
            TimingChangeTime_AC(testWorkParam.lstIDs, "A1", "B1");

            //闭合CP,断开S2
            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = true;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);

            CPPWMUpTime(testWorkParam.lstIDs, 3, 11.7, 1, 1);
            CPPWMDownTime(testWorkParam.lstIDs, 3, 9.2 + diff, 2, 2);
            //读取卡点时间
            ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

            Data_Tmp = GetOSCTime(OscTime_Tmp);
            var dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            }
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(dd, "A1->B1(1.1)", "变化时间(ms)", "-", "-", dImgs);
            #endregion

            //GetResistanceValues("B1->B2(3.1)");

            #region B1->B2(3.1)
            TimingChangeTime_AC(testWorkParam.lstIDs, "B1", "B2");

            //检测是否刷卡（有PWM波即可）
            WaitSwipingCard(testWorkParam.lstIDs, 2);

            CPPWMDownTime(testWorkParam.lstIDs, 3, 7.6 + diff, 1, 2);
            CPPWMDownTime(testWorkParam.lstIDs, 3, -9.2 + diff, 2, 2);
            //读取卡点时间
            ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

            Data_Tmp = GetOSCTime(OscTime_Tmp);
            dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            }
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(dd, "B1->B2(3.1)", "变化时间(ms)", "-", "-", dImgs);
            #endregion

            //GetResistanceValues("B2->C2(4)");

            #region B2->C2(4)
            TimingChangeTime_AC(testWorkParam.lstIDs, "B2", "C2");

            //闭合开关S2，启动充电
            if (!CheckSwipingCard(testWorkParam.lstIDs))
            {
                return;
            }
            Thread.Sleep(1000);

            CPPWMUpTime(testWorkParam.lstIDs, 3, 8.4 + diff, 1, 1);
            CPPWMUpTime(testWorkParam.lstIDs, 3, 6.8 + diff, 2, 1);
            //读取卡点时间
            ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

            Data_Tmp = GetOSCTime(OscTime_Tmp);
            dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            }
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(dd, "B2->C2(4)", "变化时间(ms)", "-", "-", dImgs);
            #endregion

            //GetResistanceValues("C2->C1(9.1)");

            #region C2->C1(9.1)
            TimingChangeTime_AC(testWorkParam.lstIDs, "C2", "C1");

            //闭合开关S2，启动充电

            CountDownTimeInfo("请手动停充后点击确认，或等待倒计时结束。", 60, 0);
            Thread.Sleep(1000);

            CPPWMDownTime(testWorkParam.lstIDs, 3, -11.2 + diff, 1, 1);
            CPPWMDownTime(testWorkParam.lstIDs, 3, 4.0 + diff, 2, 1);
            //读取卡点时间
            ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

            Data_Tmp = GetOSCTime(OscTime_Tmp);
            dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            }
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(dd, "C2->C1(9.1)", "变化时间(ms)", "-", "-", dImgs);
            #endregion

            //GetResistanceValues("C1->B1(10.1),(8.2)");

            #region C1->B1(10.1),(8.2)
            TimingChangeTime_AC(testWorkParam.lstIDs, "C1", "B1");

            //闭合CP,断开S2
            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = true;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);

            CPPWMUpTime(testWorkParam.lstIDs, 3, 8.2 + diff, 1, 2);
            CPPWMDownTime(testWorkParam.lstIDs, 3, 6.8 + diff, 2, 1);
            //读取卡点时间
            ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

            Data_Tmp = GetOSCTime(OscTime_Tmp);
            dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            }
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(dd, "C1->B1(10.1),(8.2)", "变化时间(ms)", "-", "-", dImgs);
            #endregion

            //GetResistanceValues("B1->B2(3.1)");

            #region B1->B2(3.1)
            TimingChangeTime_AC(testWorkParam.lstIDs, "B1", "B2");

            //检测是否刷卡（有PWM波即可）
            WaitSwipingCard(testWorkParam.lstIDs, 2);

            CPPWMDownTime(testWorkParam.lstIDs, 3, 7.6 + diff, 1, 2);
            CPPWMDownTime(testWorkParam.lstIDs, 3, -9.2 + diff, 2, 2);
            //读取卡点时间
            ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

            Data_Tmp = GetOSCTime(OscTime_Tmp);
            dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            }
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(dd, "B1->B2(3.1)", "变化时间(ms)", "-", "-", dImgs);
            #endregion

            //GetResistanceValues("B2->C2(4)");

            #region B2->C2(4)
            TimingChangeTime_AC(testWorkParam.lstIDs, "B2", "C2");

            //闭合开关S2，启动充电
            if (!CheckSwipingCard(testWorkParam.lstIDs))
            {
                return;
            }
            Thread.Sleep(1000);

            CPPWMUpTime(testWorkParam.lstIDs, 3, 8.4 + diff, 1, 1);
            CPPWMUpTime(testWorkParam.lstIDs, 3, 6.8 + diff, 2, 1);
            //读取卡点时间
            ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

            Data_Tmp = GetOSCTime(OscTime_Tmp);
            dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            }
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(dd, "B2->C2(4)", "变化时间(ms)", "-", "-", dImgs);
            #endregion

            //GetResistanceValues("C2->B2(7),(8.1)");

            #region C2->B2(7),(8.1)
            TimingChangeTime_AC(testWorkParam.lstIDs, "C2", "B2");

            //闭合CP,断开S2
            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = true;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);

            CPPWMUpTime(testWorkParam.lstIDs, 3, 8.6 + diff, 1, 2);
            CPPWMUpTime(testWorkParam.lstIDs, 3, 6.8 + diff, 2, 2);
            //读取卡点时间
            ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

            Data_Tmp = GetOSCTime(OscTime_Tmp);
            dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            }
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(dd, "C2->B2(7),(8.1)", "变化时间(ms)", "-", "-", dImgs);
            #endregion

            //GetResistanceValues("B2->A2(2.1)");

            #region B2->A2(2.1)
            //闭合CP,断开S2
            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = false;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);
            //模拟插拔枪
            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = true;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);
            //检测是否刷卡（有PWM波即可）
            WaitSwipingCard(testWorkParam.lstIDs, 2);

            TimingChangeTime_AC(testWorkParam.lstIDs, "B2", "A2");

            //闭合CP,断开S2
            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = false;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);

            CPPWMUpTime(testWorkParam.lstIDs, 3, 9.4 + diff, 1, 2);
            CPPWMUpTime(testWorkParam.lstIDs, 3, 11.7, 2, 2);
            //读取卡点时间
            ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

            Data_Tmp = GetOSCTime(OscTime_Tmp);
            dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            }
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(dd, "B2->A2(2.1)", "变化时间(ms)", "-", "-", dImgs);
            #endregion

            //GetResistanceValues("A2->A1(9.3)");

            #region A2->A1(9.3)
            //闭合CP,断开S2
            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = false;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);
            //模拟插拔枪
            Ks = GetKStatus16_Charging();
            Ks[0] = false;
            Ks[3] = true;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
            Thread.Sleep(1000);
            //检测是否刷卡（有PWM波即可）
            WaitSwipingCard(testWorkParam.lstIDs, 2);

            TimingChangeTime_AC(testWorkParam.lstIDs, "A2", "A1");

            Ks[3] = false;
            ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

            Thread.Sleep(2000);

            CPPWMDownTime(testWorkParam.lstIDs, 3, -11.2, 1, 1);
            CPPWMDownTime(testWorkParam.lstIDs, 3, 11.2, 2, 1);
            //读取卡点时间
            ControlEquipMent.Oscilloscope.Oscilloscope_ReadCursors(testWorkParam.lstIDs, 1, ref OscTime_Tmp);

            Data_Tmp = GetOSCTime(OscTime_Tmp);
            dd = new Dictionary<int, string>();
            foreach (var itmp in Data_Tmp)
            {
                dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value)).ToString());
            }
            dImgs = ControlEquipMent.Oscilloscope.OscilloscopeSaveScreen(testWorkParam.lstIDs);
            ProcessDataTmp(dd, "A2->A1(9.3)", "变化时间(ms)", "-", "-", dImgs);
            #endregion
        }

        private void GetResistanceValues(string sState)
        {
            List<double> values = new List<double>();
            int time = 30;
            while (time-- > 0)
            {
                values.Add(AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs.First()].CCResistance);
                Thread.Sleep(100);
            }
            ProcessDataResult(testWorkParam.lstIDs, values.Max().ToString(), "CC电阻最大值", true, sState);
            ProcessDataResult(testWorkParam.lstIDs, values.Min().ToString(), "CC电阻最小值", true, sState);
        }

        public override void ProcessData()
        {

        }

    }
}
