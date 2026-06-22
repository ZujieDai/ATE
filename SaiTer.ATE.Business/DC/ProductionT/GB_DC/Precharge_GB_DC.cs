using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 预充电测试
    /// </summary>
    public class Precharge_GB_DC : BusinessBase
    {
        double BMSDemandVolt = 0;
        double ResiLoadCurrent = 0;
        float trlTimeOut_S = 100;//超时时间
        public Precharge_GB_DC(int type) { TrialType = type; }

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            //BMSDemandVolt = Convert.ToDouble(strParams[0].Split('=')[1]);
            //LoadCurrent = Convert.ToDouble(strParams[1].Split('=')[1]);
            BMSDemandVolt = LstChargerInfo[0].NominalVoltage;
            ResiLoadCurrent = LstChargerInfo[0].NominalCurrent;
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
                //ControlEquipMent.BMS.SetParams(testWorkParam.lstIDs, BMSDemandVolt, LoadCurrent + 10, true);

                //ControlEquipMent.BMS.BMS_ON(lstIDs);

                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }

                //设置测试条件
                SetConditionValues();

                Thread.Sleep(2000);
                Dictionary<int, string> dic = new Dictionary<int, string>();
                for (int i = 0; i < testWorkParam.lstIDs.Count; i++)
                {
                    string strVolt = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingVoltage.ToString();
                    dic.Add(testWorkParam.lstIDs[i], strVolt);
                }
                ProcessDataTmp(dic, "预充电", "充电电压", (BMSDemandVolt - 20).ToString(), (BMSDemandVolt + 20).ToString());

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(300);
            }

        }


        public override void ProcessData()
        {

        }
    }
}
