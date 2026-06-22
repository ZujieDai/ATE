using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

/*
 * 1、开启交流源，断开S2，通过调整R3阻值，让CP电压达到6V后刷卡开启充电
 * 
 * 2、调整R3阻值设置CP电压到下降起始值，按照设定的步进值调整，每调节一次等待5S，判断是否停充
 * 
 * 3、如果未停充，则继续步进调整(最低调整到3V)，直到停充
 * 
 * 4、如果停充，则记录当前CP电压作为允许充电最低电压值
 * 
 * 5、刷新CP模拟拔插枪
 * 
 * 6、调整R3阻值设置CP电压到上升起始值，按照设定的步进值调整，每调节一次等待5S，判断是否停充
 * 
 * 7、同步骤3（最高调整到11V）
 * 
 * 8、同步骤4，改为记录成最高电压值
 * 
 * 9、设置R2、R3为默认值  1300、2740
 * 
 * 10、刷新CP模拟拔插枪
 */


namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 测量CP电压允许充电的极限值/CP允许充电电压极限值测试
    /// </summary>
    public class CPVoltageLimit : BusinessBase
    {
        public CPVoltageLimit(int trialType) { TrialType = trialType; }

        private double CP下降起始值, CP下降步进值, CP上升起始值, CP上升步进值;
        private int 等待停充时间;
        private double 占空比下限, 占空比上限;
        private double 占空比误差范围;


        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            CP下降起始值 = Convert.ToDouble(strParams[0].Split('=')[1]);
            CP下降步进值 = Convert.ToDouble(strParams[1].Split('=')[1]);
            CP上升起始值 = Convert.ToDouble(strParams[2].Split('=')[1]);
            CP上升步进值 = Convert.ToDouble(strParams[3].Split('=')[1]);
            等待停充时间 = Convert.ToInt32(Convert.ToDouble(strParams[4].Split('=')[1]));
            占空比误差范围 = double.Parse(strParams[5].Split('=')[1]);

            double 默认占空比 = 0.533;

            占空比下限 = 默认占空比 - 占空比误差范围 / 100;
            占空比上限 = 默认占空比 + 占空比误差范围 / 100;
        }
        public override void InitEquiMent()
        {

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
                SetCPReresh();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }
        /// <summary>
        /// 测试流程
        /// </summary>
        public void StartItemFlow()
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
                if (_StopWatch.ElapsedMilliseconds / 1000 > 10)
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
                                //CP占空比测量值1(%)|CP占空比测量值2(%)|CP占空比测量值3(%)|CP占空比下限|CP占空比上限|测试结果|查看示波器截图
                                //LstTrialData[i].ExtentData = "null|null|null|" + PwmMin + "|" + PwmMax;
                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }


                if (testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }
                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
                SendNoticeToUIAndTxtFile("断开S2，设置CP电压到下降起始值，刷卡起桩");
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);
                SetCPVolt(CP下降起始值);
                Thread.Sleep(1000);
                WaitSwipingCard(testWorkParam.lstIDs, 0);
                SendNoticeToUIAndTxtFile("带2A负载，防止桩空载时间过长自动断电");
                SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 2);

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

                SendNoticeToUIAndTxtFile("开始测允许充电的CP电压最低值");
                Dictionary<int, string> dd = new Dictionary<int, string>();
                while (CP下降起始值 > 3)
                {
                    SetCPVolt(CP下降起始值 - CP下降步进值);
                    Thread.Sleep(等待停充时间 * 1000);
                    //客户要求改成CP占空比判断
                    // double volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseA_Voltage;
                    //if (volt < 20)
                    double dutyCycle = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].CPDutyCycle / 100;
                    SendNoticeToUIAndTxtFile(string.Format("占空比为{0}", dutyCycle.ToString("F2")));
                    if (dutyCycle < 占空比下限 || dutyCycle > 占空比上限)
                    {
                        dd.Clear();
                        foreach (var itmp in testWorkParam.lstIDs)
                        {
                            double voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].CPVoltage + CP下降步进值;
                            dd.Add(itmp, voltage.ToString());
                        }
                        ProcessDataTmp(dd, "允许充电CP电压值", "最小电压", "-", "-");
                        break;
                    }
                    else
                    {
                        CP下降起始值 -= CP下降步进值;
                    }
                }
              

                SendNoticeToUIAndTxtFile("关闭负载,模拟拔插枪");
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
                SetCPReresh();
                SendNoticeToUIAndTxtFile("断开S2，设置CP电压到上升起始值，刷卡起桩");
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                ControlEquipMent.ACSource?.ACSource_ON(testWorkParam.lstIDs);
                SetCPVolt(CP上升起始值);
                Thread.Sleep(1000);
                WaitSwipingCard(testWorkParam.lstIDs, 0);
                SendNoticeToUIAndTxtFile("带2A负载，防止桩空载时间过长自动断电");
                SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 2);

                SendNoticeToUIAndTxtFile("开始测允许充电的CP电压最高值");

                while (CP上升起始值 < 11)
                {
                    SetCPVolt(CP上升起始值 + CP上升步进值);

                    //客户要求改成CP占空比判断
                    // double volt = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].PhaseA_Voltage;
                    //if (volt < 20)
                    double dutyCycle = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].CPDutyCycle / 100;
                    SendNoticeToUIAndTxtFile(string.Format("占空比为{0}", dutyCycle.ToString("F2")));
                    if (dutyCycle < 占空比下限 || dutyCycle > 占空比上限)
                    {
                        dd.Clear();
                        foreach (var itmp in testWorkParam.lstIDs)
                        {
                            double voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[0]].CPVoltage - CP上升步进值;
                            dd.Add(itmp, voltage.ToString());
                        }
                        ProcessDataTmp(dd, "允许充电CP电压值", "最大电压", "-", "-");
                        break;
                    }
                    else
                    {
                        CP上升起始值 += CP上升步进值;
                    }
                }               

                SendNoticeToUIAndTxtFile("关闭负载,模拟拔插枪，恢复导引R3阻值");
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 1300, 2740);
                Thread.Sleep(1000);

            }
        }
        public override void ProcessData()
        {

        }
        private void SetCPVolt(double CPVolt)
        {
            try
            {
                SendNoticeToUIAndTxtFile(string.Format("调整CP电压到{0}V", CPVolt));
                double R3 = (CPVolt - 0.7) / ((12 - CPVolt) / 1000);

                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 1300, Convert.ToUInt16(R3));
                SendNoticeToUIAndTxtFile(string.Format("等待{0}秒", 等待停充时间));
                Thread.Sleep(等待停充时间 * 1000);
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
        }
    }
}
