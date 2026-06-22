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
    /// 电流谐波
    /// </summary>
    public class CZ_NTGX_CN_CurrentHarmonic : BusinessBase
    {

        public CZ_NTGX_CN_CurrentHarmonic(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;//测试时间
        double BMSDemandVolt = 220;//额定电压
        double BMSDemandCurrent = 0;//额定电流
        double Power_kW = 10;
        double IntervalTime_s = 10;//数据间隔时间
        double RunTime_s = 10 * 60;
        double ErrLimit = 1;
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

                    //启动功率分析仪谐波检测
                    ControlEquipMent.PowerAnalyzer.SetHarmonicState(testWorkParam.lstIDs, 1, true);
                    Thread.Sleep(100);
                    ControlEquipMent.PowerAnalyzer.SetHarmonicState(testWorkParam.lstIDs, 2, true);
                    Thread.Sleep(100);
                    ControlEquipMent.PowerAnalyzer.SetHarmonicState(testWorkParam.lstIDs, 3, true);
                    Thread.Sleep(100);

                    //启动负载
                    BMSDemandCurrent = Math.Round(Power_kW * 1000 / BMSDemandVolt / 3, 2);
                    SendNoticeToUIAndTxtFile("负载启动中【" + BMSDemandVolt.ToString() + " V," + BMSDemandCurrent.ToString("F2") + " A】，请稍候...");
                    ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, BMSDemandVolt, BMSDemandCurrent);
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("等待负载启动...");
                    Thread.Sleep(5000);


                    CountDownTimeInfo("等待带载稳定", 20, 0);

                    //充电状态运行时间
                    DateTime dts = DateTime.Now;
                    DateTime dtIntervalT = DateTime.Now.AddSeconds(IntervalTime_s * 2);
                    int iIndex = 0;
                    SendNoticeToUIAndTxtFile("等待运行时间【"+ RunTime_s.ToString()+" s】...");
                    while (dts.AddSeconds(RunTime_s) > DateTime.Now)//目前固定3分钟
                    {
                        //if (dtIntervalT.AddSeconds(IntervalTime_s) < DateTime.Now)
                        //{
                        //    dtIntervalT = DateTime.Now;
                        //    dic = new Dictionary<int, string>();
                        //    foreach (var item in testWorkParam.lstIDs)
                        //    {
                        //        double? DCVoltage = ControlEquipMent.PowerAnalyzer.ReadCurrentHarmonicValue(testWorkParam.lstIDs);
                        //        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                        //    }
                        //    ProcessDataTmp(dic, "直流分量检测" + (iIndex++).ToString(), "直流分量(A)", (0).ToString(), (ErrLimit).ToString());
                        //}

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
                    ProcessDataTmp(dic, "放电状态", "放电电流(A)", (BMSDemandCurrent - 5).ToString(), (BMSDemandCurrent + 5).ToString());


                    List<double> lst = ControlEquipMent.PowerAnalyzer.ReadCurrentHarmonicValue_50(testWorkParam.lstIDs, 1);
                    if (lst.Count > 0)
                    {
                        for (int i = 0; i < lst.Count; i++)
                        {
                            dic = new Dictionary<int, string>();
                            foreach (var item in testWorkParam.lstIDs)
                            {
                                dic.Add(item, lst[i].ToString("F2"));
                            }
                            if (i == 0)
                            {
                                ProcessDataTmp(dic, "电流谐波" + (i + 1).ToString() + "(基波)", "谐波含量(%)", "-", "-");
                            }
                            else
                            {
                                ProcessDataTmp(dic, "电流谐波" + (i + 1).ToString(), "谐波含量(%)", (0).ToString(), (ErrLimit).ToString());
                            }
                        }
                    }
                    else
                    {
                        dic = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            dic.Add(item, (9999).ToString("F2"));//标识异常数据
                        }
                        ProcessDataTmp(dic, "电流谐波", "谐波含量(%)", (0).ToString(), (ErrLimit).ToString());
                    }

                    double dTotal= ControlEquipMent.PowerAnalyzer.ReadCurrentHarmonic_Total(testWorkParam.lstIDs, 1);
                    ProcessDataTmp(dic, "电流谐波" , "电流谐波总失真度（%）", (0).ToString(), (ErrLimit).ToString());


                    //停止负载
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);


                    //停止放电
                    CountDownTimeInfo("请停止放电动作，操作完成后点击确定", 30, 0);
                    Thread.Sleep(2000);


                    //关闭功率分析仪谐波检测
                    ControlEquipMent.PowerAnalyzer.SetHarmonicState(testWorkParam.lstIDs, 1, false);
                    Thread.Sleep(100);
                    ControlEquipMent.PowerAnalyzer.SetHarmonicState(testWorkParam.lstIDs, 2, false);
                    Thread.Sleep(100);
                    ControlEquipMent.PowerAnalyzer.SetHarmonicState(testWorkParam.lstIDs, 3, false);
                    Thread.Sleep(100);




                }
            }
            catch (Exception ex)
            {
                //停止负载
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);

                SendException(ex);
            }


        }


        public override void InitEquiMent()
        {
            SetCPRersh_EUDCALL();
        }

        public override void InitializeParams()
        {
            Init();
            //放电电压(V)=220|放电功率(kW)=10|运行时间(s)=600|谐波误差限(%)=12
            string[] strParams = TrialItem.ResultParams.Split('|');
            BMSDemandVolt = double.Parse(strParams[0].Split('=')[1]);
            Power_kW = double.Parse(strParams[1].Split('=')[1]);
            RunTime_s = double.Parse(strParams[2].Split('=')[1]);
            ErrLimit = double.Parse(strParams[3].Split('=')[1]);

        }

        public override void ProcessData()
        {

        }


    }
}
