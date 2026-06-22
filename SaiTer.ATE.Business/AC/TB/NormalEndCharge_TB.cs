using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 正常充电结束测试
    /// </summary>
    public class NormalEndCharge_TB : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        bool IsCardCharg = false;   //是否为刷卡充电

        double CPDutyMin, CPDutyMax;

        double CPPWMUpMax = 10;
        double CPPWMDownMax = 13;

        public NormalEndCharge_TB(int trialType) { TrialType = trialType; }
        public override void InitializeParams()
        {
            Init();           
            string[] strParams = TrialItem.ResultParams.Split('|');
            CPDutyMin = Convert.ToDouble(strParams[0].Split('=')[1]);
            CPDutyMax = Convert.ToDouble(strParams[1].Split('=')[1]);
            IsCardCharg = double.Parse(strParams[2].Split('=')[1]) == 1;
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
                                //CP占空比测量值1(%)|CP占空比测量值2(%)|CP占空比测量值3(%)|CP占空比下限|CP占空比上限|测试结果|查看示波器截图
                                //LstTrialData[i].ExtentData = "null|null|null|" + PwmMin + "|" + PwmMax;
                                LstTrialData[i].ExtentData = "null|null|null|null|null";

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

                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
                SetConditionValues();

                //是否为三相充电桩
                bool isSanX = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseA_Voltage > 20
                    && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseB_Voltage > 20
                    && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseC_Voltage > 20;

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                // TB欧标桩需要刷卡才能结束充电，并且等待CP波纹和充电电压为0
                if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC && IsCardCharg)
                {
                    CountDownTimeInfo("请刷卡终止充电，并等待充电桩结算后点击确认", 999, 0);
                    int i = 500;
                    while (i-- > 0)
                    {
                        if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].CPDutyCycle == 0 && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage < 20)
                        {
                            //双重判断
                            Thread.Sleep(100);
                            if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].CPDutyCycle == 0 && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage < 20)
                                break;
                        }
                        Thread.Sleep(100);
                    }
                }

                //Thread.Sleep(2000);
                int timeOut = 10;
                while (timeOut > 0)
                {
                    if (AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].CPDutyCycle == 0
                        && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseA_Voltage > 0
                        && AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseA_Voltage < 20)
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                    timeOut--;
                }



                SendNoticeToUIAndTxtFile("读取测量数据并计算结果");
                //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                //ProcessDataTmp(Data_Tmp, "充电状态", "CP正电压(V)", "8.4", "9.6");

                //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                //ProcessDataTmp(Data_Tmp, "充电状态", "CP负电压(V)", "-12.8", "-11.2");

                //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                //ProcessDataTmp(Data_Tmp, "充电状态", "CP频率(Hz)", "970", "1030");

                //Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比

                //ProcessDataTmp(Data_Tmp, "充电状态", "CP占空比(%)", CPDutyMin.ToString(), CPDutyMax.ToString());

                Dictionary<int, string> dd = new Dictionary<int, string>();

                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "RISE", 3);//上升时间
                dd = new Dictionary<int, string>();
                foreach (var itmp in Data_Tmp)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                }
                ProcessDataTmp(dd, "充电状态", "CP上升时间(us)", "0", CPPWMUpMax.ToString());

                Data_Tmp = ControlEquipMent.Oscilloscope.Oscilloscope_ReadMeasure(testWorkParam.lstIDs, "FALL", 3);//下降时间
                dd = new Dictionary<int, string>();
                foreach (var itmp in Data_Tmp)
                {
                    dd.Add(itmp.Key, (Convert.ToDouble(itmp.Value) * 1000000).ToString());
                }
                ProcessDataTmp(dd, "充电状态", "CP下降时间(us)", "0", CPPWMDownMax.ToString());


                Data_Tmp.Clear();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString());
                }
                ProcessDataTmp(Data_Tmp, "充电状态", "输出电压(V)", "0", "20");

                //此时充电已经停止，可以设置单三相切换（TB的交流源都是要用DIO继电器控制单三相输出）
                ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1500);
                if (isSanX)
                {
                    ControlEquipMent.ControlBoard.SetRelaySwitch(2, true);
                    Thread.Sleep(300);
                    ControlEquipMent.ControlBoard.SetRelaySwitch(3, false);
                    Thread.Sleep(300);
                }
                else
                {
                    ControlEquipMent.ControlBoard.SetRelaySwitch(2, false);
                    Thread.Sleep(300);
                    ControlEquipMent.ControlBoard.SetRelaySwitch(3, true);
                    Thread.Sleep(300);
                }
                ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                Thread.Sleep(1000);
            }
        }
        public override void ProcessData()
        {
           
        }
    }
}
