using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 输出过压
    /// </summary>
    public class CHAdeMO_PT_DC_OutputOverVoltage : BusinessBase
    {
        public CHAdeMO_PT_DC_OutputOverVoltage(int type)
        {
            TrialType = type;
        }
        private int trlTimeOut_S = 5;

        double BMSDemandVoltage = 0;
        double BMSDemandCurrent = 0;
        double OverVoltage = 0;//过压参考值
        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVoltage = LstChargerInfo[0].NominalVoltage;
            //BMSDemandCurrent = LstChargerInfo[0].NominalCurrent;
            BMSDemandCurrent = 20;
            OverVoltage = LstChargerInfo[0].NominalVoltage + 100;

            if (strParams.Length >= 3)
            {
                OverVoltage = Convert.ToDouble(strParams[0].Split('=')[1]);
                BMSDemandVoltage = Convert.ToDouble(strParams[1].Split('=')[1]);
                BMSDemandCurrent = Convert.ToDouble(strParams[2].Split('=')[1]);
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
                //保存试验结果               
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
                //关闭日标过压输出
                SendNoticeToUIAndTxtFile("关闭过压输出...");
                ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x8C, false, new string[] { "emtBMS_JP_DC" });
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


                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    System.Threading.Thread.Sleep(5000);//等待电压稳定

                    //模拟过压值
                    SendNoticeToUIAndTxtFile("模拟过压输出...");
                    ControlEquipMent.BMS.BMSSetBatteryVoltage(lstIDs, OverVoltage);
                    Thread.Sleep(100);
                    ControlEquipMent.BMS.BMS_DC_SetControl(lstIDs, 0x8C, true, new string[] { "emtBMS_JP_DC" });

                    System.Threading.Thread.Sleep(5000);

                    d1 = new Dictionary<int, string>();
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        d1.Add(testWorkParam.lstIDs[i], OverVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(d1, "输出过压测试", "输出过压值(V)", "-", "-");

                    string stmp;
                    stmp = ChangeBMSChargeStatus_JP_DC(AllEquipStateData.DicBMS_JP_DC_StateData[testWorkParam.lstIDs.First()].SystemState) != 7 ? "是" : "否";
                    ProcessDataResult(testWorkParam.lstIDs, stmp, "是否结束充电", stmp == "是" ? true : false, "输出过压测试");

                }
            }
            catch (Exception ex) { SendException(ex); }
        }

        public override void ProcessData()
        {

        }
    }
}
