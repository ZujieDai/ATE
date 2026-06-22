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
    public class CustomTest_TB : BusinessBase
    {
        Dictionary<int, string> Data_Tmp = new Dictionary<int, string>();//临时测试数据
        bool IsCardCharg = false;   //是否为刷卡充电

        double DemandCurrent = 30;//不动作电流
        int ChargeTime = 10;
        string TipText1 = "";
        string TipText2 = "";
        string SNameText1 = "";
        string SNameText2 = "";

        public CustomTest_TB(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();

            //充电前提示1(文本)=|充电前判断依据1(文本)=|充电中提示2(文本)=|充电中判断依据2(文本)=|充电电流(A)=10|充电时间(s)=10|桩类型（1为刷卡桩，否则为0=0
            string[] strParams = TrialItem.ResultParams.Split('|');
            TipText1 = strParams[0].Split('=')[1];
            SNameText1 = strParams[1].Split('=')[1];
            TipText2 = strParams[2].Split('=')[1];
            SNameText2 = strParams[3].Split('=')[1];
            DemandCurrent = Convert.ToDouble(strParams[4].Split('=')[1]);
            ChargeTime = (int)Convert.ToDouble(strParams[5].Split('=')[1]);
            IsCardCharg = double.Parse(strParams[6].Split('=')[1]) == 1;
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
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(lstIDs);
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
                                LstTrialData[i].ExtentData = "-|-|-|-|null";
                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                //开始检测流程
                if (testWorkParam.lstIDs.Count < 0)
                {

                    return;
                }

                if (!string.IsNullOrWhiteSpace(TipText1))
                {
                    CountDownTimeInfo(TipText1 + "\r\n（勾选为合格，否则为不合格）", 999, 2);
                    ProcessDataConnect(null, SNameText1);
                }

                SendNoticeToUIAndTxtFile("开始启动充电。");
                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }

                SetConditionValues();

                SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, DemandCurrent);
                SendNoticeToUIAndTxtFile("已发送负载不动作电流值：" + DemandCurrent + "A，等待负载稳定。");
                Thread.Sleep(1000 * ChargeTime);

                //采集数据
                Data_Tmp = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString("F2"));
                }
                ProcessDataTmp(Data_Tmp, TrialItem.ItemName, "输出电压(V)", "-", "-");

                Data_Tmp = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    Data_Tmp.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Current.ToString("F2"));
                }
                ProcessDataTmp(Data_Tmp, TrialItem.ItemName, "输出电流(A)", "-", "-");

                if (!string.IsNullOrWhiteSpace(TipText2))
                {
                    CountDownTimeInfo(TipText2 + "\r\n（勾选为合格，否则为不合格）", 999, 2);
                    ProcessDataConnect(null, SNameText2);
                }
            }
        }


        public override void ProcessData()
        {

        }
    }
}
