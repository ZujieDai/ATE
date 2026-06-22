using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 开关S断开
    /// </summary>
    public class SSwitchOFFEU : BusinessBase
    {
        public SSwitchOFFEU(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;
        private double BMSVoltage = 500;//电压
        private double BMSCurrent = 20;//
        private double BMSVoltage2 = 500;//电压
        private double BMSCurrent2 = 20;//电流
        private double BMSMeasureVoltage = 390;//过压参考值
        private int trlTimeOut_S = 5;
        private double ErrorVoltageRate = 0.05;
        private double ErrorCurrentRate = 0.05;
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
                    Thread.Sleep(2000);//等待回馈负载电流稳定
                    SendNoticeToUIAndTxtFile("正在发送开关S断开指令");


                    List<bool> Ks = GetKStatus16_Charging_DC();
                    Ks[27] = false;//PE


                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, BMSMeasureVoltage, Ks.ToArray());



                    CountDownTimeInfo("判断充电机是否能充电倒计时", TestTime, 0);
                    SendNoticeToUIAndTxtFile("判断结果中");
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }


                    ProcessDataTmp(dic, "被动终止充电", "充电电压(V)", "0", "20");


                    SendNoticeToUIAndTxtFile("关闭负载中...");
                    SetLoadDCOFF(testWorkParam.lstIDs);


                    SendNoticeToUIAndTxtFile("恢复导引互操的设置中...");

                    Ks = GetKStatus16_Charging_DC();
                    ControlEquipMent.BMS.BMSSetKState_DC(lstIDs, 1000, BMSMeasureVoltage, Ks.ToArray());


                    SendNoticeToUIAndTxtFile("关闭BMS中...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


                }




            }
            catch (Exception ex) { SendException(ex); }


        }

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;

            if (strParams.Length >=1)
            {
                //BMSVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                //BMSCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
                //BMSVoltage2 = Convert.ToDouble(strParams[2].Split('=')[1]);
                //BMSCurrent2 = Convert.ToDouble(strParams[3].Split('=')[1]);
                //ErrorVoltageRate = Convert.ToDouble(strParams[4].Split('=')[1]) / 100;
                //ErrorCurrentRate = Convert.ToDouble(strParams[5].Split('=')[1]) / 100;
                TestTime = Convert.ToInt32(Convert.ToDouble(strParams[0].Split('=')[1]));

            }
        }

        public override void ProcessData()
        {

        }
    }
}
