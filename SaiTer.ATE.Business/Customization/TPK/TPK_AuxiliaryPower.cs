using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{

    public class TPK_AuxiliaryPower : BusinessBase
    {

        public TPK_AuxiliaryPower(int type)
        {
            TrialType = type;
        }

        private double AgingVolt, AgingCurr, AgingTime, IntervalTime, VoltError, CurrError;


        public override void InitializeParams()
        {
            Init();
            //
            //老化电压(V)=750|老化电流(A)=100|老化时间(分)=30|监测数据间隔时间(秒)=30|电压误差(%)=5|电流误差(%)=5
            string[] strParams = TrialItem.ResultParams.Split('|');
            AgingVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            AgingCurr = 20;
            //AgingCurr = Convert.ToDouble(strParams[1].Split('=')[1]);
            //AgingTime = Convert.ToDouble(strParams[2].Split('=')[1]);
            //IntervalTime = Convert.ToDouble(strParams[3].Split('=')[1]);
            //VoltError = Convert.ToDouble(strParams[4].Split('=')[1]);
            //CurrError = Convert.ToDouble(strParams[5].Split('=')[1]);
        }
        public override void InitEquiMent()
        {

        }
        public override void ExecuteMethod()
        {
            try
            {
                InitializeParams();
                byte[] x = new byte[0];
                InitEquiMent();
                StartItemFlow();
            }
            catch (Exception ex)
            {
                SendException(ex);
            }
            finally
            {
                // ControlEquipMent.FeedbackLoad.FeedbackLoad_NoParallel(lstIDs);
                // SetCPReresh();

                ControlEquipMent.BMS.BMS_OFF(lstIDs);
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "关闭导引BMS");
                SetLoadDCOFF(lstIDs);
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "关闭导引负载");
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


                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }
                //设置测试条件
                SetConditionValues();

                CountDownTimeInfo("请将充电桩辅源电压设置到12V ", 60, 2);
                // SetCPReresh();
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, AgingVolt, 200, true, AgingVolt);  //
                //ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                AgingVolt = AgingVolt >= MaxAllowChargeVoltage ? MaxAllowChargeVoltage : AgingVolt;
                if (!CheckSwipingCard(testWorkParam.lstIDs, AgingVolt, AgingCurr + 10, MaxAllowChargeVoltage, false))
                {
                    return;
                }
                Thread.Sleep(2000);

                d1 = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    //外侧电压具体是模拟过压后的导引充电电压
                    d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].APSVoltage.ToString("F2"));
                }
                //SetConditionValue("K1K2外侧电压(V)", d1);
                ProcessDataTmp(d1, "12V辅源", "辅源电压", "11.5", "12.5");

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);


                CountDownTimeInfo("请将充电桩辅源电压设置到24V ", 60, 2);
                // SetCPReresh();
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, AgingVolt, 200, true, AgingVolt);  //
                                                                                                           //  ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                AgingVolt = AgingVolt + 20 >= MaxAllowChargeVoltage ? MaxAllowChargeVoltage : AgingVolt + 20;
                if (!CheckSwipingCard(testWorkParam.lstIDs, AgingVolt, AgingCurr, MaxAllowChargeVoltage, false))
                {
                    return;
                }
                Thread.Sleep(2000);

                d1 = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    //外侧电压具体是模拟过压后的导引充电电压
                    d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].APSVoltage.ToString("F2"));
                }
                //SetConditionValue("K1K2外侧电压(V)", d1);
                ProcessDataTmp(d1, "24V辅源", "辅源电压", "23.5", "24.5");

                //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);




            }
        }
        public override void ProcessData()
        {

        }
    }
}
