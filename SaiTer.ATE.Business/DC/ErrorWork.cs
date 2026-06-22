using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using SaiTer.ATE.InterFace;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标直流  工作误差   功率分析仪
    /// </summary>
    public class ErrorWork : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        double 起始电量, 测试时间, 充电桩等级;

        public ErrorWork(int type)
        {
            TrialType = type;
        }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            //测试时间(min)=2|充电桩等级=1
            string[] strParams = TrialItem.ResultParams.Split('|');
            测试时间 = Convert.ToDouble(strParams[0].Split('=')[1]);
            充电桩等级 = Convert.ToDouble(strParams[1].Split('=')[1]);
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;

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

        public void StartItemFlow()
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


                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }


                    SendNoticeToUIAndTxtFile("设备发送电量清零、并启动计算...");
                    ControlEquipMent.PowerAnalyzer.IntegralClear(testWorkParam.lstIDs);
                    Thread.Sleep(500);



                    ControlEquipMent.PowerAnalyzer.IntegralStart(testWorkParam.lstIDs);
                    CountDownTimeInfo("请输入测试前桩当前电能(kWh)", 999, 3);
                    起始电量 = Convert.ToDouble(InputData);
                  

                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, 40, BMSDemandVolt, 40);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    Stopwatch st = new Stopwatch();
                    st.Start();
                    while (st.ElapsedMilliseconds / 1000 <= 30)
                    {
                        double CheckCurrent = AllEquipStateData.DicPowerAnalyzer_StateData[testWorkParam.lstIDs[0]].Channel4RMSCurrent;

                        if (CheckCurrent >= 40 * 0.9 && CheckCurrent <= 40 * 1.1)
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                    st.Stop();
                    //设置测试条件
                    
                    SetConditionValues();

                    st.Reset();
                    st.Start();
                    SystemEvent.SendLogMessage("\r\n  ");
                    while (st.ElapsedMilliseconds < (Convert.ToInt32(测试时间 * 60) + 2) * 1000)
                    {
                        int t = (Convert.ToInt32(测试时间 * 60) + 2) - (int)st.ElapsedMilliseconds / 1000;
                        if (t % 20 == 0)
                        {
                            SystemEvent.SendLogMessage("正在充电中，剩余时间 " + t + "秒   \r\t  \r\t ");
                        }
                        Thread.Sleep(1000);
                    }
                    st.Stop();
                    SendNoticeToUIAndTxtFile("关闭负载");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    ControlEquipMent.PowerAnalyzer.IntegralStop(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("读取标准电能中...");
                   double Energy = ControlEquipMent.PowerAnalyzer.ReadIntegralValue(testWorkParam.lstIDs);
                    int Count = 1;
                    while (Count++ <= 5)
                    {
                        if (Energy == 0 || Energy == -999)
                        {
                            Thread.Sleep(1000);
                            SendNoticeToUIAndTxtFile(string.Format("第{0}次读取结束标准电能为{1},错误数据,重新读取数据", Count, Energy.ToString()));
                            Energy = ControlEquipMent.PowerAnalyzer.ReadIntegralValue(testWorkParam.lstIDs);
                        }
                        else
                        {
                            SendNoticeToUIAndTxtFile(string.Format("读取结束标准电能为{0}", Energy.ToString()));

                            break;
                        }
                    }
                    CountDownTimeInfo("请输入测试后电能(kWh)", 999, 3);
                   double 结束电量 = Convert.ToDouble(InputData);
                  

                    double ChargerEnergy = 结束电量 - 起始电量;
                    double DisPlayError = System.Math.Abs(((ChargerEnergy - Energy) / Energy) * 100);
                
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    dic.Add(LstChargerInfo[0].ChargerId, Energy.ToString());
                    ProcessDataTmp(dic, "充电电量", "标准电量(kWh)", "0", "999");
                    dic.Clear();
                    dic.Add(LstChargerInfo[0].ChargerId, ChargerEnergy.ToString("F2"));
                    ProcessDataTmp(dic, "充电电量", "充电桩电量(kWh)", "0", "999");

                    dic.Clear();
                    dic.Add(LstChargerInfo[0].ChargerId, DisPlayError.ToString("F2"));
                    ProcessDataTmp(dic, "充电电量", "误差(%)", "0", 充电桩等级.ToString());

                  
                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }
        public override void ProcessData()
        {

        }
    }
}
