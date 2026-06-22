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
    public class LNReverse_DIO : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        Dictionary<int, double[]> OscTime_Tmp = new Dictionary<int, double[]>();//示波器光标卡点时间
        Dictionary<int, string> dImgs = new Dictionary<int, string>();//图片存储       
        uint RelayIndex = 0;

        public LNReverse_DIO(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ItemParams.Split('|');
            if (strParams.Length > 0)
            {
                RelayIndex = Convert.ToUInt32(double.Parse(strParams[0].Split('=')[1])) - 1;
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
                SendNoticeToUIAndTxtFile("恢复正常状态");
                ControlEquipMent.ControlBoard.SetRelaySwitch(RelayIndex - 1, true);
                Thread.Sleep(500);
                ControlEquipMent.ControlBoard.SetRelaySwitch(RelayIndex, false);
                Thread.Sleep(500);
                SetCPReresh();
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
                                //测试时间|输入电压|是否停机|测试结果     
                                LstTrialData[i].ExtentData = DateTime.Now.ToString() + "|" + 220 + "|" + 220 + "|未停机";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                //目前提示手动反接
                SendNoticeToUIAndTxtFile($"模拟反接状态，控制Y{RelayIndex + 1}闭合");
                ControlEquipMent.ACSource?.ACSource_OFF(testWorkParam.lstIDs);
                //CountDownTimeInfo("请手动将L/N线反接，接好后请点击确定按钮！", 1000000, 2);

                ControlEquipMent.ControlBoard.SetRelaySwitch(RelayIndex - 1, false);
                Thread.Sleep(500);
                ControlEquipMent.ControlBoard.SetRelaySwitch(RelayIndex, true);
                Thread.Sleep(1000);


                ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);//启动BMS
                //开始检测流程
                if (testWorkParam.lstIDs.Count > 0)
                {
                    //正常情况是反接后允许充电, 这里应能有电压. 但如果桩本身不合格,反接后上不了电压,
                    //因此不能用判断电压是否起来的方式去倒计时,否则会一直到倒计时结束.
                    CountDownTimeInfo("请刷卡充电。如果刷卡后无法上电起桩，直接点击确定按钮或等待倒计时结束继续", 50, 4);
                    Thread.Sleep(2000);


                    //设置测试条件
                    SetConditionValues();

                    //采集数据
                    Data_Tmp = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        double voltage = 0;
                        if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_GB_AC ||
                            LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC ||
                            LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC)
                        {
                            voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage;
                        }
                        Data_Tmp.Add(testWorkParam.lstIDs[i], voltage.ToString("F2"));
                    }
                    ProcessDataTmp(Data_Tmp, "LN反接", "桩输出电压(V)", "80", "260");
                }
            }
        }


        public override void ProcessData()
        {
        }
    }
}
