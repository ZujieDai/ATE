using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 计量功能：示值误差(计量模块)
    /// </summary>
    public class CCS2_PT_DC_ErrorIndication : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;

        double 充电桩等级;//充电桩等级
        double 工作误差校验圈数;//工作误差校验圈数
        double 电表常数;//电表常数
        double 起始电量;
        double 结束电量;//kWh
        double 示值误差测试时间;//分钟
        public CCS2_PT_DC_ErrorIndication(int type)
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
            //电能表常数=100|工作误差校验圈数=4|充电桩等级=1|示值误差测试时间(分钟)=3
            电表常数 = Convert.ToDouble(strParams[0].Split('=')[1]);
            工作误差校验圈数 = Convert.ToDouble(strParams[1].Split('=')[1]);
            充电桩等级 = Convert.ToDouble(strParams[2].Split('=')[1]);
            示值误差测试时间 = Convert.ToDouble(strParams[3].Split('=')[1]);
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

                    if (testWorkParam.lstIDs.Count <= 0)
                    {
                        return;
                    }
                    CountDownTimeInfo(string.Format("请确认电能表常数和工作误差测试校验圈数设置正确!\r\n常数为 {0},圈数为{1}", 电表常数, 工作误差校验圈数), 50, 0);

                    if (!CheckSwipingCard(testWorkParam.lstIDs))
                    {
                        return;
                    }

                    SendNoticeToUIAndTxtFile("设置电能表常数和工作误差校验圈数");

                    ControlEquipMent.BMS.BMSSetConstAndInspectionError(testWorkParam.lstIDs, 电表常数, 工作误差校验圈数);
                    Thread.Sleep(500);
                    SendNoticeToUIAndTxtFile("设备发送电量清零信号...");
                    ControlEquipMent.BMS.BMSClearEnergy(testWorkParam.lstIDs);
                    Thread.Sleep(500);
                    CountDownTimeInfo("请输入测试前电能(kWh)", 999, 3);
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
                    for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                    {
                        int key = testWorkParam.lstIDs[i];
                        if (AllEquipStateData.DicACSource_StateData.Count == 1)
                        {
                            key = 1;//多个桩只有一个源的时候，源实时状态字典内只有一个1号桩
                        }
                        d1.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Volt.ToString());
                        d2.Add(testWorkParam.lstIDs[i], AllEquipStateData.DicACSource_StateData[key].Freq.ToString());
                    }
                    SetConditionValue("供电电压(V)", d1);
                    SetConditionValue("供电频率(Hz)", d2);

                    st.Reset();
                    st.Start();

                    while (st.ElapsedMilliseconds < (Convert.ToInt32(示值误差测试时间 * 60) + 2) * 1000)
                    {
                        int t = (Convert.ToInt32(示值误差测试时间 * 60) + 2) - (int)st.ElapsedMilliseconds / 1000;
                        SystemEvent.SendLogMessage("正在充电中，剩余时间 " + t + "秒   \r\t  \r\t ");

                        Thread.Sleep(20 * 1000);
                    }
                    st.Stop();
                    SendNoticeToUIAndTxtFile("关闭负载");
                    SetLoadDCOFF(testWorkParam.lstIDs);
                    SendNoticeToUIAndTxtFile("读取标准电能中...");
                    double BMSEnergy = ControlEquipMent.BMS.BMSGetEnergy(testWorkParam.lstIDs); 
                    BMSEnergy = ControlEquipMent.BMS.BMSGetEnergy(testWorkParam.lstIDs);//由于计量板的机制，需要读取两次
                    CountDownTimeInfo("请输入测试后电能(kWh)", 999, 3);
                    结束电量 = Convert.ToDouble(InputData);
                    double ChargerEnergy = 结束电量 - 起始电量;
                    double error = System.Math.Abs(((ChargerEnergy - BMSEnergy) / BMSEnergy) * 100);

                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    dic.Add(LstChargerInfo[0].ChargerId, BMSEnergy.ToString());
                    ProcessDataTmp(dic, "充电电量", "标准电量", "0", "999");
                    dic.Clear();
                    dic.Add(LstChargerInfo[0].ChargerId, ChargerEnergy.ToString());
                    ProcessDataTmp(dic, "充电电量", "充电桩电量", "0", "999");

                    dic.Clear();
                    dic.Add(LstChargerInfo[0].ChargerId, error.ToString());
                    ProcessDataTmp(dic, "充电电量", "示值误差", "0", 充电桩等级.ToString());
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
