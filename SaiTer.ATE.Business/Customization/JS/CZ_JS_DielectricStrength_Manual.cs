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
    /// JS定制：介电强度试验（安规：交流、直流耐压）
    /// </summary>
    public class CZ_JS_DielectricStrength_Manual : BusinessBase
    {
        string sState;
        string FlowItemName;
        float trlTimeOut_S = 8;//超时时间

        string VoltageValue1, VoltageValue2, VoltageValue3, VoltageValue4, HISETCurrent, TestTime;

        public CZ_JS_DielectricStrength_Manual(int trialType)
        {
            TrialType = trialType;
        }
        /// <summary>
        /// 初始化当前检测项信息 
        /// </summary>
        public override void InitializeParams()
        {
            ControlEquipMent.ACSource?.ACSource_OFF(lstIDs);
            //交流耐压值(kV)=2|直流耐压值_输入对地(kV)=2.8|直流耐压值_输出对地(kV)=4.2|直流耐压值_输入对输出(kV)=4.2|HISET电流值(mA)=10.00| 测试时间(S)=60.00
            string[] strParams = TrialItem.ResultParams.Split('|');
            VoltageValue1 = strParams[0].Split('=')[1].Trim('\r');
            VoltageValue2 = strParams[1].Split('=')[1].Trim('\r');
            VoltageValue3 = strParams[2].Split('=')[1].Trim('\r');
            VoltageValue4 = strParams[3].Split('=')[1].Trim('\r');
            HISETCurrent = strParams[4].Split('=')[1].Trim('\r');
            TestTime = strParams[5].Split('=')[1].Trim('\r');
        }
        /// <summary>
        /// 设备初始化
        /// </summary>
        public override void InitEquiMent()
        {
        }
        public override void ExecuteMethod()
        {
            try
            {
                Init();
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

        /// <summary>
        /// 根据类型切换继电器状态
        /// </summary>
        /// <param name="type">0-输入对地，1-输出对地，2-输入对输出</param>
        private void SetControlboard(int type)
        {
            if (sState == "交流耐压")
                CountDownTimeInfo($"【{sState}】请控制安规设备设置到对应配置，完成后点击确认", 999, 0);
            else
                CountDownTimeInfo($"【{sState}-{FlowItemName}】请控制安规设备设置到对应配置，完成后点击确认", 999, 0);
        }

        private void StarTestItem()
        {
            if (sState == "交流耐压")
                CountDownTimeInfo($"【{sState}】请启动安规设备，等待安规测试仪检测结束后点击确认", 999, 0);
            else
                CountDownTimeInfo($"【{sState}-{FlowItemName}】请启动安规设备，等待安规测试仪检测结束后点击确认", 999, 0);

            Thread.Sleep(100);

            // Thread.Sleep((Convert.ToInt32(double.Parse(TestTime)) + 2) * 1000);
            if (sState == "交流耐压")
                CountDownTimeInfo($"【{sState}】请输入测量值后点击确认", 999, 0);
            else
                CountDownTimeInfo($"【{sState}-{FlowItemName}】请输入测量值后点击确认", 999, 3);

            //开始判断数据
            if (string.IsNullOrEmpty(InputData))
            {
                ProcessDataResult(testWorkParam.lstIDs, "-", HISETCurrent, InputData, FlowItemName, false, sState);
            }
            else
            {
                ProcessDataResult(testWorkParam.lstIDs, "-", HISETCurrent, InputData, FlowItemName, true, sState);
            }
        }

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
                                //交流耐压值(kV)|HISET电流值(mA)|LOSET电流值(mA)| 测试时间(S)|斜坡时间(S)|ACW参考值(mA)|ARC电流值(mA)|测试频率(Hz)|测试结果
                                //LstTrialData[k].ExtentData = Result.LstData[0].ToString().TrimEnd('\0');
                                //LstTrialData[i].ExtentData = VoltageValue + "|" + HISETCurrent + "|" + LOSETCurrent + "|" +
                                //    TestTime + "|" + RampTime + "|" + ACW_DCW + "|" + ARC;
                                //if (TrialType == (int)EmTrialType.交流耐压_输入对地
                                //      || TrialType == (int)EmTrialType.交流耐压_输出对地
                                //      || TrialType == (int)EmTrialType.交流耐压_输入对输出)
                                //{
                                //    LstTrialData[i].ExtentData += "|" + TestFreq;
                                //}
                                LstTrialData[i].ExtentData = TrialItem.ItemName + "|测试超时|" + "-" + "|" + HISETCurrent + "|null";
                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                if (testWorkParam.lstIDs.Count > 0)
                {
                    #region 直流耐压
                    sState = "直流耐压";

                    FlowItemName = "输入对地";
                    SetControlboard(0);
                    StarTestItem();
                    CountDownTimeInfo($"【{TrialItem.ItemName}-{FlowItemName}】请恢复设备正常，完成后点击确认", 999, 0);

                    FlowItemName = "输出对地";
                    SetControlboard(1);
                    StarTestItem();
                    CountDownTimeInfo($"【{TrialItem.ItemName}-{FlowItemName}】请恢复设备正常，完成后点击确认", 999, 0);

                    FlowItemName = "输入对输出";
                    SetControlboard(2);
                    StarTestItem();
                    CountDownTimeInfo($"【{TrialItem.ItemName}-{FlowItemName}】请恢复设备正常，完成后点击确认", 999, 0);

                    #endregion

                    #region 交流耐压
                    sState = "交流耐压";

                    FlowItemName = "-";
                    SetControlboard(0);
                    StarTestItem();
                    CountDownTimeInfo($"【{TrialItem.ItemName}】请恢复设备正常，完成后点击确认", 999, 0);
                    #endregion
                }
            }
        }

        public override void ProcessData()
        {
        }

    }
}
