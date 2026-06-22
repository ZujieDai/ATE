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
    /// 电压异常保护检测（过欠压测试）
    /// </summary>
    public class CZ_NTGX_CN_ACVoltageAnomalyProtection : BusinessBase
    {
        public CZ_NTGX_CN_ACVoltageAnomalyProtection(int type)
        {
            TrialType = type;
        }
        private int TestTime = 0;//测试时间
        double BMSDemandVolt = 220;//额定电压
        double BMSDemandCurrent = 0;//额定电流
        double Power_kW = 10;
        double VoltUp = 240;
        double VoltDown = 200;
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

                    #region 过频保护

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
                    //BMSDemandCurrent = Power_kW * 1000 / BMSDemandVolt;
                    //ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, BMSDemandVolt, BMSDemandCurrent);
                    //ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);
                    //SendNoticeToUIAndTxtFile("等待负载启动...");
                    //Thread.Sleep(5000);

                    //CountDownTimeInfo("等待带载稳定", 10, 0);


                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel1RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "正常放电", "放电电压(V)", (BMSDemandVolt - 20).ToString(), (BMSDemandVolt + 20).ToString());



                    //模拟电网侧频率
                    SendNoticeToUIAndTxtFile("模拟电网侧电压【" + VoltUp.ToString() + " V】...");
                    ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, VoltUp);
                    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);

                    Thread.Sleep(15000);//等待响应

                    //停止负载
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);

                    //停止交流源
                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, BMSDemandVolt);
                    Thread.Sleep(5000);

                    //读取数据判断结果
                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel1RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "过压保护" + VoltUp.ToString(), "放电电压(V)", (0).ToString(), (20).ToString());//需要停止充电

                    //停止放电
                    CountDownTimeInfo("请停止放电动作，操作完成后点击确定", 30, 0);
                    Thread.Sleep(2000);

                    #endregion



                    #region 欠频保护

                    SendNoticeToUIAndTxtFile("设备正在启动放电中，请稍候...");
                    Thread.Sleep(1000);

                    CountDownTimeInfo("请设定交流放电功率【" + Power_kW + " kW】", 30, 0);

                    //放电检测
                    if (!CheckDisCharge_NTGX_CN(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    Thread.Sleep(5000);



                    ////启动负载
                    //BMSDemandCurrent = Power_kW * 1000 / BMSDemandVolt;
                    //ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, BMSDemandVolt, BMSDemandCurrent);
                    //ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);
                    //SendNoticeToUIAndTxtFile("等待负载启动...");
                    //Thread.Sleep(5000);

                    //CountDownTimeInfo("等待带载稳定", 10, 0);


                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel1RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "正常放电", "放电电压(V)", (BMSDemandVolt - 20).ToString(), (BMSDemandVolt + 20).ToString());



                    //模拟电网侧频率
                    SendNoticeToUIAndTxtFile("模拟电网侧电压【" + VoltDown.ToString() + " V】...");
                    ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, VoltDown);
                    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);

                    Thread.Sleep(15000);//等待响应

                    //停止负载
                    ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);

                    //停止交流源
                    ControlEquipMent.ACSource.ACSource_OFF(testWorkParam.lstIDs);
                    ControlEquipMent.ACSource.ACSource_SetVolt(testWorkParam.lstIDs, BMSDemandVolt);
                    Thread.Sleep(5000);

                    //读取数据判断结果
                    dic = new Dictionary<int, string>();
                    foreach (var item in testWorkParam.lstIDs)
                    {
                        double? DCVoltage = AllEquipStateData.DicPowerAnalyzer_StateData.FirstOrDefault().Value?.Channel1RMSVolt;
                        dic.Add(item, DCVoltage.GetValueOrDefault().ToString("F2"));
                    }
                    ProcessDataTmp(dic, "欠压保护" + VoltDown, "放电电压(V)", (0).ToString(), (20).ToString());//需要停止充电

                    //停止放电
                    CountDownTimeInfo("请停止放电动作，操作完成后点击确定", 30, 0);
                    Thread.Sleep(2000);

                    #endregion

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
            string[] strParams = TrialItem.ResultParams.Split('|');
            //放电电压(V)=220|放电电流(A)=10|电压上限(V)=240|电压下限(V)=200
            BMSDemandVolt = double.Parse(strParams[0].Split('=')[1]);
            BMSDemandCurrent = double.Parse(strParams[1].Split('=')[1]);
            VoltUp = double.Parse(strParams[2].Split('=')[1]);
            VoltDown = double.Parse(strParams[3].Split('=')[1]);

        }

        public override void ProcessData()
        {

        }


    }
}
