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
    /// JS定制：绝缘电阻试验_人工（安规）
    /// </summary>
    public class CZ_JS_InsulationResistance_Manual : BusinessBase
    {
        string FlowItemName = "";
        float trlTimeOut_S = 8;//超时时间

        string IRVolt = "1";
        string LOSETResistance = "10";
        string TestTime = "6";

        /// <summary>
        /// 小的检定点名称
        /// </summary>
        string TrialFlowName = "";
        public CZ_JS_InsulationResistance_Manual(int trialType)
        {
            TrialType = trialType;
        }
        /// <summary>
        /// 初始化当前检测项信息 
        /// </summary>
        public override void InitializeParams()
        {
            ControlEquipMent.ACSource?.ACSource_OFF(lstIDs);
            //电压值(kV)=1.000|LOSET电阻值(MΩ)=10.0|测试时间(S)=60.0
            string[] strParams = TrialItem.ResultParams.Split('|');
            IRVolt = strParams[0].Split('=')[1].Trim('\r');
            LOSETResistance = strParams[1].Split('=')[1].Trim('\r');
            TestTime = strParams[2].Split('=')[1].Trim('\r');
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
                Thread.Sleep(300);
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
            CountDownTimeInfo($"【{TrialItem.ItemName}-{FlowItemName}】请控制安规设备设置到对应配置，完成后点击确认", 999, 0);
        }

        private void StarTestItem()
        {
            //ControlEquipMent.Safety.SafetyOFF(testWorkParam.lstIDs);
            //Thread.Sleep(100);

            //ControlEquipMent.Safety.SafetySetParam(testWorkParam.lstIDs, "FUNC:TEST ON", "\r\n", "\r\n");
            //SendNoticeToUIAndTxtFile(info + ", 启动安规检测，大约需要 " + (double.Parse(TestTime) + 2.00).ToString() + "秒，等待安规测试仪结果");
            CountDownTimeInfo($"【{TrialItem.ItemName}-{FlowItemName}】请启动安规设备，等待安规测试仪检测结束后点击确认", 999, 0);

            Thread.Sleep(100);

            // Thread.Sleep((Convert.ToInt32(double.Parse(TestTime)) + 2) * 1000);
            CountDownTimeInfo($"【{TrialItem.ItemName}-{FlowItemName}】请输入测量值后点击确认", 999, 3);

            //开始判断数据
            if (string.IsNullOrEmpty(InputData))
            {
                ProcessDataResult(testWorkParam.lstIDs, LOSETResistance, "-", InputData, FlowItemName, false);
            }
            else
            {
                ProcessDataResult(testWorkParam.lstIDs, LOSETResistance, "-", InputData, FlowItemName, true);
            }
            CountDownTimeInfo($"【{TrialItem.ItemName}-{FlowItemName}】请恢复设备正常，完成后点击确认", 999, 0);
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
                                //IR电压值(KV)|HISET电阻值(MΩ)|LOSET电阻值(MΩ)| 测试时间(S)|斜坡时间(S)|IR参考值(MΩ)|测试结果
                                //LstTrialData[k].ExtentData = Result.LstData[0].ToString().TrimEnd('\0');
                                //LstTrialData[i].ExtentData = IRVolt + "|" + HISETResistance + "|" + LOSETResistance + "|" +
                                //    TestTime + "|" + RampTime + "|" + IRReferenceValue;

                                LstTrialData[i].ExtentData = TrialItem.ItemName + "|测试超时|" + LOSETResistance + "| " + "-" + "|null";

                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }
                if (testWorkParam.lstIDs.Count > 0)
                {

                    FlowItemName = "输入对地";
                    SetControlboard(0);
                    StarTestItem();

                    FlowItemName = "输出对地";
                    SetControlboard(1);
                    StarTestItem();

                    FlowItemName = "输入对输出";
                    SetControlboard(2);
                    StarTestItem();
                }
            }
        }

        public override void ProcessData()
        {
        }
    }
}
