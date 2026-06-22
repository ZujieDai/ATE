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
    /// 频率适应性检测
    /// </summary>
    public class CZ_NTGX_CN_FrequencyAdaptabilityCheck : BusinessBase
    {
        public CZ_NTGX_CN_FrequencyAdaptabilityCheck(int type)
        {
            TrialType = type;
        }
        //放电电压(V)=220|起始频率(Hz)=49.55|终止频率(Hz)=50.15|频率变化步进(Hz)=0.1|运行时间(s)=5
        private int TestTime = 0;//测试时间
        double BMSDemandVolt = 0;//额定电压
        double BMSDemandCurrent = 0;//额定电流
        double FreqStart = 49.55;
        double FreqEnd = 50.15;
        double Bj = 0.1;
        double RunTime_s = 5;
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

                    CountDownTimeInfo("请设定交流放电电压：【" + BMSDemandVolt.ToString() + " V】", 30, 0);

                    //放电检测
                    if (!CheckDisCharge_NTGX_CN(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    Thread.Sleep(5000);

                    CountDownTimeInfo("等待充电运行稳定", 20, 0);

                    //充电状态运行时间
                    DateTime dts = DateTime.Now;
                    int iCnt = 0;
                    double dFreq = FreqStart;
                    while (true)//目前固定3分钟
                    {
                        dFreq = FreqStart + (Bj * iCnt);
                        if (dFreq > FreqEnd) dFreq = FreqEnd;

                        //调节电网模拟器频率
                        SendNoticeToUIAndTxtFile("模拟电网侧频率【" + dFreq.ToString() + " Hz】...");
                        ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, BMSDemandVolt);
                        ControlEquipMent.ACSource.ACSource_SetFreq(testWorkParam.lstIDs, dFreq);
                        ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);
                        Thread.Sleep(3000);//启动前设置频率会恢复为50，这里再发一次
                        ControlEquipMent.ACSource.ACSource_SetFreq(testWorkParam.lstIDs, dFreq);

                        Thread.Sleep((int)RunTime_s * 1000);

                        dic = new Dictionary<int, string>();
                        foreach (var item in testWorkParam.lstIDs)
                        {
                            double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel1RMSVolt;
                            dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                        }
                        ProcessDataTmp(dic, "电网侧频率 " + dFreq, "充电电压(V)", (BMSDemandVolt - 30).ToString(), (BMSDemandVolt + 30).ToString());


                        iCnt++;
                        if (dFreq >= FreqEnd) break;
                    }


                    //停止负载
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);

                    //停止交流源
                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    Thread.Sleep(3000);

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
                //放电电压(V)=220|起始频率(Hz)=49.55|终止频率(Hz)=50.15|频率变化步进(Hz)=0.1|运行时间(s)=5
                string[] strParams = TrialItem.ResultParams.Split('|');
                BMSDemandVolt = double.Parse(strParams[0].Split('=')[1]);
                FreqStart = double.Parse(strParams[1].Split('=')[1]);
                FreqEnd = double.Parse(strParams[2].Split('=')[1]);
                Bj = double.Parse(strParams[3].Split('=')[1]);
                RunTime_s = double.Parse(strParams[4].Split('=')[1]);
            }
            catch(Exception ex) { SendException(ex); }


        }

        public override void ProcessData()
        {

        }


    }
}
