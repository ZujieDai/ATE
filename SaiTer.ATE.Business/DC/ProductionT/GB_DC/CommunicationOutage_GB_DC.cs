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
    /// 国标直流通讯中断试验
    /// </summary>
    public class CommunicationOutage_GB_DC : BusinessBase
    {
        int trlTimeOut_S = 30;

        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        public CommunicationOutage_GB_DC(int type)
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
            //BMSDemandVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            //LoadCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
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
                SetCPReresh();
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
                //ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, BMSDemandVolt, LstChargerInfo[0].NominalVoltage, LoadCurrent + 10);

                if (!CheckSwipingCard(testWorkParam.lstIDs, BMSDemandVolt, 20))
                {
                    return;
                }
                SendNoticeToUIAndTxtFile("启动负载，并等待带载电流稳定");
                SetLoadPara(testWorkParam.lstIDs, BMSDemandVolt - 20, 10, BMSDemandVolt, 10);
                Thread.Sleep(500);
               SetLoadDCON(testWorkParam.lstIDs);
                Thread.Sleep(1000 * 15);//等待回馈负载电流稳定
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

                SendNoticeToUIAndTxtFile("发送模拟通讯中断指令");
                List<bool> SBitS = GetKStatus16_Charging_DC();
                SBitS[20] = false;
                SBitS[21] = false;
                SBitS[31] = true;//停止发送报文
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());
                Thread.Sleep(1000 * 10);
                 

                Dictionary<int, string> dic = new Dictionary<int, string>();
                foreach (var item in testWorkParam.lstIDs)
                {
                    double volt = AllEquipStateData.DicBMS_DC_StateData[item].ChargingVoltage;
                    dic.Add(item, volt.ToString("F2"));
                }
                ProcessDataTmp(dic, "通讯中断", "充电桩电压", "0", "30");


                SetLoadDCOFF(testWorkParam.lstIDs);
                SBitS[31] = false;//恢复发送报文
                ControlEquipMent.BMS.BMSSetKState_DC(testWorkParam.lstIDs, 1000, 390, SBitS.ToArray());

            }

        }
        public override void ProcessData()
        {

        }
    }
}
