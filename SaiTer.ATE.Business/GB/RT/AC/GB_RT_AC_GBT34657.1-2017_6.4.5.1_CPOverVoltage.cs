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
    /// CP回路电压限值测试
    /// </summary>
    public class GB_RT_AC_CPOverVoltage : BusinessBase
    {
        private UInt16 限值内R2, 限值内R3, 超限上限值R2, 超限上限值R3, 超限下限值R2, 超限下限值R3,
                 车端电阻最大值R2, 车端电阻最大值R3, 车端电阻最小值R2, 车端电阻最小值R3;
        private double MaxVoltage = 240;
        private double MinVoltage = 180;
        private string ItemFlow;
        int num = 1;
        public GB_RT_AC_CPOverVoltage(int type)
        {
            TrialType = type;
        }

        public override void InitializeParams()
        {
            Init();
            //数据库参数格式
            //限值内R2(Ω)=1300|限值内R3(Ω)=2740|超限上限值R2(Ω)=4000|超限上限值R3(Ω)=4000|超限下限值R2(Ω)=200|超限下限值R3(Ω)=200|车端最大电阻值R2(Ω)=1339|车端最大电阻值R3(Ω)=2822|车端最小电阻值R2(Ω)=2500|车端最小R3(Ω)=1500
            string[] strParams = TrialItem.ResultParams.Split('|');
            限值内R2 = Convert.ToUInt16(double.Parse(strParams[0].Split('=')[1]));
            限值内R3 = Convert.ToUInt16(double.Parse(strParams[1].Split('=')[1]));
            超限上限值R2 = Convert.ToUInt16(double.Parse(strParams[2].Split('=')[1]));
            //超限上限值R2 = Convert.ToUInt16(1300);
            超限上限值R3 = Convert.ToUInt16(double.Parse(strParams[3].Split('=')[1]));
            超限下限值R2 = Convert.ToUInt16(double.Parse(strParams[4].Split('=')[1]));
            //超限下限值R2 = Convert.ToUInt16(1300);
            超限下限值R3 = Convert.ToUInt16(double.Parse(strParams[5].Split('=')[1]));
            车端电阻最大值R2 = Convert.ToUInt16(double.Parse(strParams[6].Split('=')[1]));
            车端电阻最大值R3 = Convert.ToUInt16(double.Parse(strParams[7].Split('=')[1]));
            车端电阻最小值R2 = Convert.ToUInt16(double.Parse(strParams[8].Split('=')[1]));
            车端电阻最小值R3 = Convert.ToUInt16(double.Parse(strParams[9].Split('=')[1]));

            MaxVoltage = LstChargerInfo[0].NominalVoltage * 1.2;
            MinVoltage = LstChargerInfo[0].NominalVoltage * 0.8;
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

                                LstTrialData[i].ExtentData = "null|null|null|null|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                num = 1;

                //导引状态为已连接
                //if (!CheckChargerIn(testWorkParam.lstIDs))
                //{
                //    return;
                //}
                SendNoticeToUIAndTxtFile(string.Format("开始限值内测试。发送导引R2电阻 = {0}, R3电阻 = {1}，", 限值内R2, 限值内R3));
                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 限值内R2, 限值内R3);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                Thread.Sleep(3000);
                //设置测试条件
                SetConditionValues();

                int sleepTime = 50;
                ControlEquipMent.Oscilloscope.OscilloscopeIDefalut(testWorkParam.lstIDs);//初始化
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 1, true, "DC", "20", Channel1, "Output_Voltage", "50", "V", false, "250", "-2.5");
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 2, false, "DC", "20", Channel2, "Output_Current", "50", "V", false, "10", "0");
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 3, true, "DC", "0.25", Channel3, "CP_Voltage", "50", "V", false, "10", "1.5");
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Channel_Set(testWorkParam.lstIDs, 4, false, "DC", "20", Channel4, "CPPwm", "50", "V", false, "10", "0");
                Thread.Sleep(sleepTime);

                ControlEquipMent.Oscilloscope.Oscilloscope_Measure_Initialize(testWorkParam.lstIDs);//清除所有测量项
                                                                                                    //添加测量值
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FREQ", 3);//频率
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "DUTY", 3);//占空比
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "TOP", 3);//高值
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "BASE", 3);//低值
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "RISE", 3);//低值
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_AddMeasure(testWorkParam.lstIDs, "FALL", 3);//低值
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_Trigger(testWorkParam.lstIDs, 0, "FALL", "DC", "EDGE", "10", 3, "0", "Auto");//通道3采样，否则截图会有残影
                Thread.Sleep(sleepTime);
                ControlEquipMent.Oscilloscope.Oscilloscope_TimeBase(testWorkParam.lstIDs, true, "0.4", "0");//低值
                Thread.Sleep(sleepTime);

                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                Thread.Sleep(1000);

                // WaitSwipingCard(testWorkParam.lstIDs, 0);
                ItemFlow = "限值内" + "|R2电阻 = " + 限值内R2 + "，R3电阻 = " + 限值内R3;
                CountDownTimeInfo(string.Format("已发送导引R2电阻 = {0}, R3电阻 = {1}，请刷卡后【即插即充无需刷卡】点击确认按钮", 限值内R2, 限值内R3), 300, 0);
                Thread.Sleep(2000);
                if (Customer != null && Customer.ToUpper().Equals("NT"))
                    CollectionCPPwm("5.4", "6.6", "-", "-", "-", "-", "-", "-", "限值内", "0.4");
                else
                    CollectionCPPwm("5.47", "6.53", "-", "-", "-", "-", "-", "-", "限值内", "0.4");
                ProcessData();


                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1000);
                SetCPReresh();

                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                Thread.Sleep(2000);

                SendNoticeToUIAndTxtFile(string.Format("开始超限上限测试。发送导引R2电阻 = {0}, R3电阻 = {1}，", 超限上限值R2, 超限上限值R3));
                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 超限上限值R2, 超限上限值R3);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
                Thread.Sleep(3000);
                CountDownTimeInfo(string.Format("已发送导引R2电阻 = {0}, R3电阻 = {1}，请刷卡后【即插即充无需刷卡】点击确认按钮", 超限上限值R2, 超限上限值R3), 300, 0);
                MaxVoltage = 30;
                MinVoltage = 0;
                ItemFlow = "上偏超限" + "|R2电阻 = " + 超限上限值R2 + "，R3电阻 = " + 超限上限值R3;
                Thread.Sleep(2000);
                if (Customer != null && Customer.ToUpper().Equals("NT"))
                    CollectionCPPwm("6.6", "-", "-", "-", "-", "-", "-", "-", "上偏超限", "0.4");
                else
                    CollectionCPPwm("6.8", "-", "-", "-", "-", "-", "-", "-", "上偏超限", "0.4");
                ProcessData();

                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                Thread.Sleep(2000);

                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 限值内R2, 限值内R3);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1000);
                //模拟CP断线、模拟插拔枪，恢复桩故障状态
                List<bool> Ks = GetKStatus16_Charging();
                Ks[3] = false;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                Thread.Sleep(3000);
                Ks[3] = true;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                Thread.Sleep(2000);


                SendNoticeToUIAndTxtFile(string.Format("开始超限下限测试。发送导引R2电阻 = {0}, R3电阻 = {1}，", 超限下限值R2, 超限下限值R3));
                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 超限下限值R2, 超限下限值R3);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs); 
                Thread.Sleep(3000);
                CountDownTimeInfo(string.Format("已发送导引R2电阻 = {0}, R3电阻 = {1}，请刷卡后【即插即充无需刷卡】点击确认按钮", 超限下限值R2, 超限下限值R3), 300, 0);
                MaxVoltage = 30;
                MinVoltage = 0;
                ItemFlow = "下偏超限" + "|R2电阻 = " + 超限下限值R2 + "，R3电阻 = " + 超限下限值R3;
                Thread.Sleep(2000);
                if (Customer != null && Customer.ToUpper().Equals("NT"))
                    CollectionCPPwm("0", "5.4", "-", "-", "-", "-", "-", "-", "下偏超限", "0.4");
                else
                    CollectionCPPwm("0", "5.2", "-", "-", "-", "-", "-", "-", "下偏超限", "0.4");
                ProcessData();


                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                Thread.Sleep(2000);

                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 限值内R2, 限值内R3);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1000);
                //模拟CP断线、模拟插拔枪，恢复桩故障状态
                Ks = GetKStatus16_Charging();
                Ks[3] = false;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                Thread.Sleep(3000);
                Ks[3] = true;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                Thread.Sleep(2000);
                SendNoticeToUIAndTxtFile(string.Format("车端电阻最大值测试。发送导引R2电阻 = {0}, R3电阻 = {1}，", 车端电阻最大值R2, 车端电阻最大值R3));
                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 车端电阻最大值R2, 车端电阻最大值R3);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs); Thread.Sleep(3000);
                CountDownTimeInfo(string.Format("已发送导引R2电阻 = {0}, R3电阻 = {1}，请刷卡后【即插即充无需刷卡】点击确认按钮", 车端电阻最大值R2, 车端电阻最大值R3), 300, 0);
                MaxVoltage = LstChargerInfo[0].NominalVoltage * 1.2;
                MinVoltage = LstChargerInfo[0].NominalVoltage * 0.8;
                ItemFlow = "车端电阻最大值" + "|R2电阻 = " + 车端电阻最大值R2 + "，R3电阻 = " + 车端电阻最大值R3;
                Thread.Sleep(2000);
                if (Customer != null && Customer.ToUpper().Equals("NT"))
                    CollectionCPPwm("5.4", "6.6", "-", "-", "-", "-", "-", "-", "车端电阻最大值", "0.4");
                else
                    CollectionCPPwm("5.47", "6.53", "-", "-", "-", "-", "-", "-", "车端电阻最大值", "0.4");
                ProcessData();

                ControlEquipMent.Oscilloscope.Oscilloscope_IsRun(testWorkParam.lstIDs, true);
                Thread.Sleep(2000);

                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 限值内R2, 限值内R3);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1000);
                //模拟CP断线、模拟插拔枪，恢复桩故障状态
                Ks = GetKStatus16_Charging();
                Ks[3] = false;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                Thread.Sleep(3000);
                Ks[3] = true;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                Thread.Sleep(2000);

                SendNoticeToUIAndTxtFile(string.Format("车端电阻最小值测试。发送导引R2电阻 = {0}, R3电阻 = {1}，", 车端电阻最小值R2, 车端电阻最小值R3));
                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 车端电阻最小值R2, 车端电阻最小值R3);
                Thread.Sleep(100);
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs); 
                Thread.Sleep(3000);
                CountDownTimeInfo(string.Format("已发送导引R2电阻 = {0}, R3电阻 = {1}，请刷卡后【即插即充无需刷卡】点击确认按钮", 车端电阻最小值R2, 车端电阻最小值R3), 300, 0);
                MaxVoltage = LstChargerInfo[0].NominalVoltage * 1.2;
                MinVoltage = LstChargerInfo[0].NominalVoltage * 0.8;
                ItemFlow = "车端电阻最小值" + "|R2电阻 = " + 车端电阻最小值R2 + "，R3电阻 = " + 车端电阻最小值R3;
                Thread.Sleep(2000);
                if (Customer != null && Customer.ToUpper().Equals("NT"))
                    CollectionCPPwm("5.4", "6.6", "-", "-", "-", "-", "-", "-", "车端电阻最小值", "0.4");
                else
                    CollectionCPPwm("5.47", "6.53", "-", "-", "-", "-", "-", "-", "车端电阻最小值", "0.4");
                ProcessData();

                SendNoticeToUIAndTxtFile("恢复导引R2、R3电阻");
                ControlEquipMent.BMS.BMS_SetResistance(testWorkParam.lstIDs, 限值内R2, 限值内R3);
                Thread.Sleep(1000);
                //模拟CP断线、模拟插拔枪，恢复桩故障状态
                Ks = GetKStatus16_Charging();
                Ks[3] = false;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                Thread.Sleep(2000);
                Ks[3] = true;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);
                Thread.Sleep(2000);

            }
        }
        public override void ProcessData()
        {
            foreach (var item in testWorkParam.lstIDs)
            {

                int k = LstTrialData.FindIndex(s => s.ChargerId == item);
                int i = LstChargerInfo.FindIndex(s => s.ChargerId == item);
                if (k < 0)
                    return;
                LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].ItemName = iIndex.ToString();
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                double volate = AllEquipStateData.DicBMS_AC_StateData[LstTrialData[k].ChargerId].PhaseA_Voltage;

                if (volate <= MaxVoltage && volate >= MinVoltage)
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                }
                LstTrialData[k].ExtentData = ItemFlow + "|" + MinVoltage + " | " + MaxVoltage + " | " + volate.ToString("F2") + "|报表(勿删)";
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                SendTrialDataToUI(LstTrialData[k]);
                SaveTrialData(LstTrialData[k]);
            }
            iIndex++;
        }
    }
}
