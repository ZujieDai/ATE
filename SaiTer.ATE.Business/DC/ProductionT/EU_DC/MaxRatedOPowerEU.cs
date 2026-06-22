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
    /// 最大额定输出功率测试欧标
    /// </summary>
    public class MaxRatedOPowerEU : BusinessBase
    {
        public MaxRatedOPowerEU(int type)
        {
            TrialType = type;
        }
        double BMSDemandVolt = 0;//额定电压
        double ResiLoadCurrent = 0;//额定电流
        int trlTimeOut_S = 0;
        double DemandVoltage = 750;//电压上限
        double DemandCurrent = 60;//电流上限

        int TestTime;

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




                    ControlEquipMent.BMS.BMS_ON(lstIDs);
                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }
                    ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, DemandVoltage, DemandCurrent, true, 390);

                    SystemEvent.SendWaitSwipingCard(testWorkParam.lstIDs, DemandVoltage, LstChargerInfo[0].ChargerType, 0);


                    Thread.Sleep(10*1000);//

                    SendNoticeToUIAndTxtFile("设备正在开启负载中，请稍候...");
                    SetLoadPara(testWorkParam.lstIDs, DemandVoltage - 20, DemandCurrent + 5, DemandVoltage-5, DemandCurrent );
                   SetLoadDCON(testWorkParam.lstIDs);











                    CountDownTimeInfo("等待电流稳定中", TestTime, 0);



                    SendNoticeToUIAndTxtFile("判断结果中...");


                    Dictionary<int, string> dicP = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData[item].Channel4Power;
                        dicP.Add(item, DCVoltage.ToString("F2"));
                    }


                    ProcessDataTmp(dicP, "测试最大功率", "运行时功率(KW)", "0", MaxOutputPower.ToString());





                    SendNoticeToUIAndTxtFile("关闭负载中...");

                    SetLoadDCOFF(testWorkParam.lstIDs);




                }
            }
            catch (Exception ex) { SendException(ex); }


        }


        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
            //OscilloscopeInputCurrentOvershootE();
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
                TestTime = Convert.ToInt32(Convert.ToDouble(strParams[0].Split('=')[1]));
            }
            DemandVoltage = LstChargerInfo[0].NominalVoltage;
            DemandCurrent = LstChargerInfo[0].NominalCurrent;
        }

        public override void ProcessData()
        {

        }


    }
}
