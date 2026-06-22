using SaiTer.ATE.DataModel;
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
    /// 紧急停机保护测试（急停按钮/开门保护） 不测停机时间
    /// </summary>
    public class EmergencyStop : BusinessBase
    {
        float trlTimeOut_S = 100;//超时时间
       
        public EmergencyStop(int type) { TrialType = type; }


        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');           
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

                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    ControlEquipMent.ACSource.ACSource_ON(testWorkParam.lstIDs);//打开交流源
                    ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);//启动BMS
                }

                SendNoticeToUIAndTxtFile("启动充电");

                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }

                #region 产测简单判断电压降为0，不计算断开电压的时间
              

             
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
                #endregion

                string info1 = "", info2 = "", info3 = "", sState = "";
                if ((EmTrialType)TrialType == EmTrialType.紧急停机保护测试)
                {
                    info1 = "请按下急停按钮";
                    info2 = "请按下充电桩急停按钮,然后点击确认或倒计时结束后自动判断";
                    info3 = "请恢复急停按钮";
                    sState = "紧急停机保护测试";
                }
                else if ((EmTrialType)TrialType == EmTrialType.开门保护测试)
                {
                    info1 = "请模拟开门";
                    info2 = "请模拟开门操作,然后点击确认或倒计时结束后自动判断";
                    info3 = "请关门";
                    sState = "开门保护测试";
                }
                SendNoticeToUIAndTxtFile(info1);
               

                CountDownTimeInfo(info2, 60, 0);
                Thread.Sleep(5000);
               



                Dictionary<int, string> dicData = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    string voltage = "0";
                    if (AllEquipStateData.DicBMS_AC_StateData != null && AllEquipStateData.DicBMS_AC_StateData.Count > 0)
                    {
                        voltage = AllEquipStateData.DicBMS_AC_StateData[testWorkParam.lstIDs[i]].PhaseA_Voltage.ToString();
                    }
                    else if (AllEquipStateData.DicBMS_DC_StateData != null && AllEquipStateData.DicBMS_DC_StateData.Count > 0)
                    {
                        voltage = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString();
                    }
                    if (dicData.ContainsKey(testWorkParam.lstIDs[i]))
                    {
                        dicData[testWorkParam.lstIDs[i]] = voltage;
                    }
                    else
                    {
                        dicData.Add(testWorkParam.lstIDs[i], voltage);
                    }
                }
                ProcessDataTmp(dicData, sState, "电压值", "0", "20");
                CountDownTimeInfo(info3, 60, 0);
            }

        }
        public override void ProcessData()
        {

        }
    }
}
