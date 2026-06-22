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
    /// 有功功率控制检测
    /// </summary>
    public class CZ_NTGX_CN_ActivePowerControl : BusinessBase
    {
        public CZ_NTGX_CN_ActivePowerControl(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;//测试时间
        double BMSDemandVolt = 220;//额定电压
        double BMSDemandCurrent = 0;//额定电流
        double Power_kW = 10;
        double ErrLimit = 10;
        double RunTime_s = 10 * 60;
        int trlTimeOut_S = 0;


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
                SendNoticeToUIAndTxtFile("恢复互操作中...");
                SetCPRersh_EUDC();
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

                    //插枪完毕
                    Dictionary<int, string> dic = new Dictionary<int, string>();

                    SendNoticeToUIAndTxtFile("设备正在启动放电中，请稍候...");
                    Thread.Sleep(1000);

                    CountDownTimeInfo("请设定交流放电功率【" + Power_kW + " kW】", 30, 0);

                    //放电检测
                    if (!CheckDisCharge_NTGX_CN(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    Thread.Sleep(5000);



                    //启动负载
                    BMSDemandCurrent = Math.Round(Power_kW * 1000 / BMSDemandVolt / 3, 2);
                    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, BMSDemandVolt, BMSDemandCurrent);
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("等待负载启动...");
                    Thread.Sleep(5000);


                    CountDownTimeInfo("等待带载稳定", 20, 0);

                    //充电状态运行时间
                    SendNoticeToUIAndTxtFile("等待运行稳定时间【"+RunTime_s.ToString()+" s】，请稍候...");
                    DateTime dts = DateTime.Now;
                    while (dts.AddSeconds(RunTime_s) > DateTime.Now)//目前固定2分钟
                    {
                        Thread.Sleep(1000);
                    }

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel1RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "放电状态", "放电电压(V)", (BMSDemandVolt - 30).ToString(), (BMSDemandVolt + 30).ToString());

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel1RMSCurrent;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "放电状态", "放电电流(A)", (BMSDemandCurrent - 10).ToString(), (BMSDemandCurrent + 10).ToString());

                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double DCVoltage =(double) AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.TotalPower;
                        DCVoltage = Math.Abs(DCVoltage);
                        dic.Add(item, DCVoltage.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "放电状态", "放电功率(kW)", (Power_kW * (1 - ErrLimit / 100)).ToString(), (Power_kW * (1 + ErrLimit / 100)).ToString());



                    //停止负载
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);


                    //停止放电
                    CountDownTimeInfo("请停止放电动作，操作完成后点击确定", 30, 0);
                    Thread.Sleep(2000);




                }
            }
            catch (Exception ex) { SendException(ex); }


        }


        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
        }

        public override void InitializeParams()
        {
            try
            {
                Init();
                string[] strParams = TrialItem.ResultParams.Split('|');
                //放电电压(V)=220|放电功率(kW)=10|功率误差限(%)=10|运行时间(s)=120
                BMSDemandVolt = double.Parse(strParams[0].Split('=')[1]);
                Power_kW = double.Parse(strParams[1].Split('=')[1]);
                ErrLimit = double.Parse(strParams[2].Split('=')[1]);
                RunTime_s = double.Parse(strParams[3].Split('=')[1]);
            }
            catch(Exception ex) { SendException(ex); }

        }

        public override void ProcessData()
        {

        }


    }
}
