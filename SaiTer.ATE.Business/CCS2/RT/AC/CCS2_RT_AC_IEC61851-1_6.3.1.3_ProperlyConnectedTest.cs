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
    /// 欧标研测交流：验证EV是否正确连接到EV充电设备
    /// </summary>
    public class CCS2_RT_AC_ProperlyConnectedTest : BusinessBase
    {
        public CCS2_RT_AC_ProperlyConnectedTest(int type)
        {
            TrialType = type;
        }
        public override void InitializeParams()
        {

        }
        public override void InitEquiMent()
        {

        }
        public override void ExecuteMethod()
        {
            try
            {
                Init();
                SetCPReresh();
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
                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(lstIDs);
                ControlEquipMent.BMS.BMS_OFF(lstIDs);

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
                Thread.Sleep(1000);
                //设置测试条件
                SetConditionValues();


                if (!CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
                //等待电压稳定
                Thread.Sleep(2000);

                SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].NominalCurrent);
                SendNoticeToUIAndTxtFile("已发送负载正常电流值：" + LstChargerInfo[0].NominalCurrent + "A，等待负载稳定。");
                Thread.Sleep(8000);

                d1 = new Dictionary<int, string>();
                foreach(int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Voltage.ToString());
                }
                ProcessDataTmp(d1, "验证充电连接", "充电电压(V)", (LstChargerInfo[0].NominalVoltage * 0.9).ToString(), (LstChargerInfo[0].NominalVoltage * 1.1).ToString());
                Thread.Sleep(200);
                d1 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, AllEquipStateData.DicBMS_AC_StateData[item].PhaseA_Current.ToString());
                }
                ProcessDataTmp(d1, "验证充电连接", "充电电流(A)", (LstChargerInfo[0].NominalCurrent * 0.9).ToString(), (LstChargerInfo[0].NominalCurrent * 1.1).ToString());
                Thread.Sleep(200);
                d1 = new Dictionary<int, string>();
                foreach (int item in testWorkParam.lstIDs)
                {
                    d1.Add(item, AllEquipStateData.DicBMS_AC_StateData[item].CPVoltage.ToString());
                }
                ProcessDataTmp(d1, "验证充电连接", "CP电压(V)", "5.47", "6.53");
            }
        }
        public override void ProcessData()
        {

        }
    }
}
