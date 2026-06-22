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
    internal class CZ_TB_InsulationFaultCheckEU : BusinessBase
    {
        int trlTimeOut_S = 30;
        private int TestTime = 10;
        private double OutputVoltageError = 5;

        public CZ_TB_InsulationFaultCheckEU(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();

            string[] strParams = TrialItem.ResultParams.Split('|');
            //故障判断时间(S)=10|输出电压误差(%)=5
            if (strParams.Length >= 1)
            {
                //BMSVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                //BMSCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
                TestTime = Convert.ToInt32(Convert.ToDouble(strParams[0].Split('=')[1]));
                OutputVoltageError = Convert.ToDouble(strParams[1].Split('=')[1]);
            }
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
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
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


                //if (!CheckSwipingCard(testWorkParam.lstIDs))
                //{
                //    return;
                //}

                //设置测试条件
                SetConditionValues();

                double BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
                ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, LstChargerInfo[0].NominalCurrent, true, BMSDemandVolt);
                //Thread.Sleep(10*1000);//等待回馈负载电流稳定
                SendNoticeToUIAndTxtFile("正在发送DC+DC-对地22.9kΩ指令");
                bool[] Ks = new bool[24];
                Ks[0] = true;//DC+DC-控制
                Ks[1] = true;//CC信号控制
                Ks[2] = true;//CP信号控制
                Ks[4] = true;//PE信号控制
                ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, BatteryVoltage_EU, Ks.ToArray(), 1, 1, "0");
                Thread.Sleep(300);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                //检测能否刷卡
                WaitSwipingCard(testWorkParam.lstIDs, 3);
                CountDownTimeInfo("判断充电机是否能充电倒计时", TestTime, 0);

                Dictionary<int, string> dic = new Dictionary<int, string>();
                foreach (var item in testWorkParam.lstIDs)
                {
                    double DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                    dic.Add(item, DCVoltage.ToString("F2"));
                }
                ProcessDataTmp(dic, "充电前绝缘检测_22.9kΩ", "充电电压(V)", "0", "20");

                CountDownTimeInfo("请确认是否有绝缘故障报警", 999, 2);
                ProcessDataResult(testWorkParam.lstIDs, DicManualVerifyResult.First().Value ? "是" : "否", "是否绝缘故障报警", DicManualVerifyResult.First().Value, "充电前绝缘检测_22.9kΩ");

                CountDownTimeInfo("请查看App“实时数据”中正负绝缘电阻值(kΩ)，并输入到文本框内（合格值为5%误差范围内）", 999, 3);
                double insulationResistance = Convert.ToDouble(InputData);
                ProcessDataResult(testWorkParam.lstIDs, (22.9 * 0.95).ToString(), (22.9 * 1.05).ToString(), insulationResistance.ToString(), "绝缘电阻(kΩ)",
                    insulationResistance >= (22.9 * 0.95) && insulationResistance <= (22.9 * 1.05), "充电前绝缘检测_22.9kΩ");

                CountDownTimeInfo("请查看绝练故障，灯语为蓝灯快闪2次，熄灭1s，反复循环", 999, 2);
                ProcessDataConnect("充电前绝缘检测", "灯语是否正确");

                SendNoticeToUIAndTxtFile("关闭导引中...");
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                SendNoticeToUIAndTxtFile("恢复互操设置中...");
                SetCPRersh_EUDCALL();
                Thread.Sleep(2500);
                SendNoticeToUIAndTxtFile("正在发送DC+DC-对地100kΩ指令");
                ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, BatteryVoltage_EU, Ks.ToArray(), 6, 6, "0");
                Thread.Sleep(300);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                //检测能否刷卡
                WaitSwipingCard(testWorkParam.lstIDs, 3);
                CountDownTimeInfo("判断充电机是否能充电倒计时", TestTime, 0);

                dic = new Dictionary<int, string>();
                foreach (var item in testWorkParam.lstIDs)
                {
                    double DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].ChargingVoltage;
                    dic.Add(item, DCVoltage.ToString("F2"));
                }
                ProcessDataTmp(dic, "充电前绝缘检测_100kΩ", "充电电压(V)", (BMSDemandVolt * (1 - OutputVoltageError / 100)).ToString("F2"), (BMSDemandVolt * (1 + OutputVoltageError / 100)).ToString("F2"));

                CountDownTimeInfo("请确认是否有绝缘故障报警", 999, 2);
                ProcessDataResult(testWorkParam.lstIDs, DicManualVerifyResult.First().Value ? "是" : "否", "是否绝缘故障报警", !DicManualVerifyResult.First().Value, "充电前绝缘检测_100kΩ");

                CountDownTimeInfo("请查看App“实时数据”中正负绝缘电阻值(kΩ)，并输入到文本框内（合格值为5%误差范围内）", 999, 3);
                insulationResistance = Convert.ToDouble(InputData);
                ProcessDataResult(testWorkParam.lstIDs, (100 * 0.95).ToString(), (100 * 1.05).ToString(), insulationResistance.ToString(), "绝缘电阻(kΩ)",
                    insulationResistance >= (100 * 0.95) && insulationResistance <= (100 * 1.05), "充电前绝缘检测_100kΩ");

                SendNoticeToUIAndTxtFile("关闭导引中...");
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1000);
                SendNoticeToUIAndTxtFile("恢复互操设置中...");
                SetCPRersh_EUDCALL();
                Thread.Sleep(2500);


                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
                var lstKS = ControlEquipMent.BMS.BMSGetKState_EU_DC(lstIDs, out double batteryVolt).First().Value;
                ChangeKS_EU_DC(lstKS, out Ks, out int DCPlus, out int DCMinus);
                SendNoticeToUIAndTxtFile("正在发送DC+DC-对地300kΩ指令");
                ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, BatteryVoltage_EU, Ks.ToArray(), 7, 7, "0");
                CountDownTimeInfo("请查看App“实时数据”中正负绝缘电阻值(kΩ)，并输入到文本框内（合格值为5%误差范围内）", 999, 3);
                insulationResistance = Convert.ToDouble(InputData);
                ProcessDataResult(testWorkParam.lstIDs, (300 * 0.95).ToString(), (300 * 1.05).ToString(), insulationResistance.ToString(), "绝缘电阻(kΩ)",
                    insulationResistance >= (300 * 0.95) && insulationResistance <= (300 * 1.05), "充电中绝缘检测_300kΩ");

                SendNoticeToUIAndTxtFile("正在发送DC+DC-对地33kΩ指令");
                ControlEquipMent.BMS.BMSSetKState_EU_DC(lstIDs, BatteryVoltage_EU, Ks.ToArray(), 7, 7, "0");
                CountDownTimeInfo("请查看App“实时数据”中正负绝缘电阻值(kΩ)，并输入到文本框内（合格值为5%误差范围内）", 999, 3);
                insulationResistance = Convert.ToDouble(InputData);
                ProcessDataResult(testWorkParam.lstIDs, (33 * 0.95).ToString(), (33 * 1.05).ToString(), insulationResistance.ToString(), "绝缘电阻(kΩ)",
                    insulationResistance >= (33 * 0.95) && insulationResistance <= (33 * 1.05), "充电中绝缘检测_33kΩ");

                CountDownTimeInfo("请查看绝练故障，灯语为蓝灯快闪2次，熄灭1s，反复循环", 999, 2);
                ProcessDataConnect("充电中绝缘检测", "灯语是否正确");

            }

        }

        public override void ProcessData()
        {

        }
    }
}
