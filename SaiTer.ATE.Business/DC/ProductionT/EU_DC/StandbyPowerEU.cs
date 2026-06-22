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
    /// 欧标待机功耗试验
    /// </summary>
    public class StandbyPowerEU : BusinessBase
    {
        public StandbyPowerEU(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;
        private double CheckPower = 1;//单位为kw
        private double BMSMeasureVoltage = 390;//过压参考值
        private int trlTimeOut_S = 5;

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
                    SendNoticeToUIAndTxtFile("关闭导引中");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(2000);//等待回馈负载电流稳定
                



                    CountDownTimeInfo("等待功率稳定中", TestTime, 0);
                    SendNoticeToUIAndTxtFile("判断结果中");
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? Power = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel4Power;
                        dic.Add(item, Power.GetValueOrDefault().ToString("F2"));
                    }


                    ProcessDataTmp(dic, "待机中", "待机功率(KW)", "0", (CheckPower/1000).ToString());


                    SendNoticeToUIAndTxtFile("恢复互操命令中，请稍候...");


                    SetCPRersh_EUDC();



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

            if (strParams.Length >=2)
            {
                //BMSVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                //BMSCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
                //BMSVoltage2 = Convert.ToDouble(strParams[2].Split('=')[1]);
                //BMSCurrent2 = Convert.ToDouble(strParams[3].Split('=')[1]);
                //ErrorVoltageRate = Convert.ToDouble(strParams[4].Split('=')[1]) / 100;
                //ErrorCurrentRate = Convert.ToDouble(strParams[5].Split('=')[1]) / 100;
                TestTime = Convert.ToInt32(Convert.ToDouble(strParams[0].Split('=')[1]));
                CheckPower = Convert.ToDouble(strParams[1].Split('=')[1]);

            }
        }

        public override void ProcessData()
        {

        }
    }
}
