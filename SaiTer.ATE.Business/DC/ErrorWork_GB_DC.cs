using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 国标直流  工作误差
    /// </summary>
    public class ErrorWork_GB_DC : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;

        double 充电桩等级;//充电桩等级
        double 工作误差校验圈数;//工作误差校验圈数
        double 电表常数;//电表常数

        public ErrorWork_GB_DC(int type)
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
            //电能表常数=50|工作误差校验圈数=4|充电桩等级=1
            电表常数 = Convert.ToDouble(strParams[0].Split('=')[1]);
            工作误差校验圈数 = Convert.ToDouble(strParams[1].Split('=')[1]);
            充电桩等级 = Convert.ToDouble(strParams[2].Split('=')[1]);
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


                    SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, 40, BMSDemandVolt, 40);
                    Thread.Sleep(500);
                    SetLoadDCON(testWorkParam.lstIDs);
                    Thread.Sleep(1000 * 15);//等待回馈负载电流稳定
                                            //设置测试条件
            
                    SetConditionValues();

                    TrialMethod(1);
                    SendNoticeToUIAndTxtFile("设置负载" + BMSDemandVolt + "V、20A，并等待带载电流稳定");

                    SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, 20, BMSDemandVolt, 20);
                    TrialMethod(2);


                    SetLoadDCOFF(testWorkParam.lstIDs);

                }
            }
            catch (Exception ex)
            {
                SendException(ex);
            }

        }

        private void TrialMethod(int index)
        {
            SendNoticeToUIAndTxtFile("设置电能表常数和工作误差校验圈数");

            ControlEquipMent.BMS.BMSSetConstAndInspectionError(testWorkParam.lstIDs, 电表常数, 工作误差校验圈数);
            Thread.Sleep(500);
            SendNoticeToUIAndTxtFile("设备发送误差清零信号...");
            ControlEquipMent.BMS.BMSClearError(testWorkParam.lstIDs);
            Thread.Sleep(500);
            double[] WorkError = new double[3];
            int count = 0;
            int errorcount = 0;
            double time = 1000;

            // 发送清零指令转发给计量板至少需要再读一次值清除缓存，否则就会读取到上一次的值
            while (++count <= time)
            {
                WorkError = ControlEquipMent.BMS.BMSGetError(testWorkParam.lstIDs, Convert.ToInt32(电表常数).ToString("X"), Convert.ToInt32(工作误差校验圈数).ToString("X"))[LstChargerInfo[0].ChargerId];
                if(WorkError[1] != 0 || WorkError[2] != 0)
                {
                    Thread.Sleep(500);
                    WorkError = ControlEquipMent.BMS.BMSGetError(testWorkParam.lstIDs, Convert.ToInt32(电表常数).ToString("X"), Convert.ToInt32(工作误差校验圈数).ToString("X"))[LstChargerInfo[0].ChargerId];
                    if (WorkError[1] != 0 || WorkError[2] != 0)
                    {
                        ControlEquipMent.BMS.BMSClearError(testWorkParam.lstIDs);
                        Thread.Sleep(500);
                    }
                }
                else
                {
                    break;
                }
            }
            SendNoticeToUIAndTxtFile("读第" + index + "次误差");
            count = 0;
            while (++count <= time)
            {
                WorkError = ControlEquipMent.BMS.BMSGetError(testWorkParam.lstIDs, Convert.ToInt32(电表常数).ToString("X"), Convert.ToInt32(工作误差校验圈数).ToString("X"))[LstChargerInfo[0].ChargerId];

                if (WorkError[1] != 0 && WorkError[2] != 0 && WorkError[1] < 100 && WorkError[2] < 100)
                {
                    if (System.Math.Abs(WorkError[1]) <= 充电桩等级 && System.Math.Abs(WorkError[2]) <= 充电桩等级)
                    {
                        break;
                    }
                    if (++errorcount >= 100)
                    {
                        break;
                    }
                }
                if (count >= 990)
                {
                    break;
                }
                System.Threading.Thread.Sleep(500);
            }
            Dictionary<int, string> dic = new Dictionary<int, string>();
            dic.Add(LstChargerInfo[0].ChargerId, System.Math.Abs(WorkError[1]).ToString());
            ProcessDataTmp(dic, "第" + index + "次误差", "误差值1", "0", 充电桩等级.ToString());
            dic = new Dictionary<int, string>();
            dic.Add(LstChargerInfo[0].ChargerId, System.Math.Abs(WorkError[2]).ToString());
            ProcessDataTmp(dic, "第" + index + "次误差", "误差值2", "0", 充电桩等级.ToString());
        }
        public override void ProcessData()
        {

        }
    }
}
