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
    /// 输出电压偏差
    /// </summary>
    public class CZ_NTGX_CN_VoltageDeviation : BusinessBase
    {
        public CZ_NTGX_CN_VoltageDeviation(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;//测试时间
        double BMSDemandVolt = 0;//额定电压
        double BMSDemandCurrent = 0;//额定电流
        double ExceedBattery = 390;//超过的电压值
        double VoltUp = 230;
        double VoltDown = 210;
        double ErrLimit = 5;//允许偏差百分比
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

                    CountDownTimeInfo("请设定交流放电电压：【"+VoltUp.ToString()+" V】", 30, 0);

                    //放电检测
                    if (!CheckDisCharge_NTGX_CN(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    Thread.Sleep(5000);


                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel1RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "放电电压最大值", "放电电压(V)", (VoltUp * (1 - ErrLimit / 100)).ToString(), (VoltUp * (1 + ErrLimit / 100)).ToString());


                    CountDownTimeInfo("请设定交流放电电压：【" + VoltDown.ToString() + " V】", 30, 0);

                    //放电检测
                    if (!CheckDisCharge_NTGX_CN(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    Thread.Sleep(5000);


                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel1RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "放电电压最小值", "放电电压(V)", (VoltDown * (1 - ErrLimit / 100)).ToString(), (VoltDown * (1 + ErrLimit / 100)).ToString());



                    //停止负载
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);


                    //停止放电
                    CountDownTimeInfo("请停止放电动作，操作完成后点击确定", 30, 0);
                    Thread.Sleep(1000);


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
            Init();
            try
            {
                string[] strParams = TrialItem.ResultParams.Split('|');
                //电压上限(V)=240|电压下限(V)=200|误差偏差(%)=10
                VoltUp = double.Parse(strParams[0].Split('=')[1]);
                VoltDown = double.Parse(strParams[1].Split('=')[1]);
                ErrLimit = double.Parse(strParams[2].Split('=')[1]);
                ErrLimit = ErrLimit > 100 ? 100 : ErrLimit;
            }
            catch(Exception ex)
            {

            }
        }

        public override void ProcessData()
        {

        }


    }
}
