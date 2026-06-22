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
    /// JS定制：漏电测试（人工）
    /// </summary>
    public class CZ_JS_LeakageCurrent_Manual : BusinessBase
    {
        List<int> ChargerIndexLst = new List<int>();
        float trlTimeOut_S = 8;//超时时间
        string GBCurrent, HISETResistance, TestTime;
        string info = "";
        int index = -1;//不同测量点需要控制的继电器索引号
        public CZ_JS_LeakageCurrent_Manual(int trialType)
        {
            TrialType = trialType;
        }
        /// <summary>
        /// 初始化当前检测项信息 
        /// </summary>
        public override void InitializeParams()
        {
            //输入电压值(V)=242.00|HISET电阻值(mA)=3.50|测试时间(S)=60.00
            string[] strParams = TrialItem.ResultParams.Split('|');
            GBCurrent = strParams[0].Split('=')[1].Trim('\r');
            HISETResistance = strParams[1].Split('=')[1].Trim('\r');
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
                CountDownTimeInfo($"【{TrialItem.ItemName}】请恢复设备正常，完成后点击确认", 999, 0);
                //保存试验结果
                SaveTrialResult();
                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                //发送试验结束刷新UI
                SendMessageEndThisTrial();
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
                                //GB电流值(A)|HISET电阻值(MΩ)|LOSET电阻值(MΩ)| 测试时间(S)|斜坡时间(S)|GB参考值(MΩ)|测试频率(Hz)
                                //LstTrialData[k].ExtentData = Result.LstData[0].ToString().TrimEnd('\0');
                                LstTrialData[i].ExtentData = "漏电测试|-|-|" + HISETResistance + "|null";
                                SendTrialDataToUI(LstTrialData[i]);
                            }
                        }
                    }
                    break;
                }

                CountDownTimeInfo($"【{TrialItem.ItemName}】请启动安规设备，等待安规测试仪检测结束后点击确认", 999, 0);

                Thread.Sleep(100);

                // Thread.Sleep((Convert.ToInt32(double.Parse(TestTime)) + 2) * 1000);
                CountDownTimeInfo($"【{TrialItem.ItemName}】请输入测量值后点击确认", 999, 3);

                //开始判断数据
                if (string.IsNullOrEmpty(InputData))
                {
                    ProcessDataResult(testWorkParam.lstIDs, "-", HISETResistance, InputData, "-", false);
                }
                else
                {
                    ProcessDataResult(testWorkParam.lstIDs, "-", HISETResistance, InputData, "-", true);
                }

            }
        }



        public override void ProcessData()
        {
        }

    }
}
