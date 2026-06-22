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
    /// 欧标研测直流：确认电动汽车与电动汽车供电设备连接正常
    /// </summary>
    public class CCS2_RT_DC_VerificationEVConetcedEquipmentTest : BusinessBase
    {
        public CCS2_RT_DC_VerificationEVConetcedEquipmentTest(int type)
        {
            TrialType = type;
        }

        private int trlTimeOut_S = 5;
        Double CPVoltageMax = 10;
        Double CPVoltageMin = 8;
        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');

            if (strParams.Length >= 3)
            {
                CPVoltageMax = Convert.ToDouble(strParams[0].Split('=')[1]);
                CPVoltageMin = Convert.ToDouble(strParams[1].Split('=')[1]);
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

                    SendNoticeToUIAndTxtFile("关闭导引中，请稍候...");
                    ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);

                    SendNoticeToUIAndTxtFile("恢复互操命令中，请稍候...");
                    SetCPRersh_EUDC();

                    Thread.Sleep(2000);
                    //CountDownTimeInfo("请插枪，如果是请点击确认", TestTime, 0);

                    SendNoticeToUIAndTxtFile("判断结果中");
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage = AllEquipStateData.DicBMS_EU_DC_StateData[item].CPVoltage;
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }

                    ProcessDataTmp(dic, "充电机插枪时", "插枪连接电压(V)", CPVoltageMin.ToString(), CPVoltageMax.ToString());
                }
            }
            catch (Exception ex) { SendException(ex); }
        }

        public override void ProcessData()
        {

        }
    }
}
