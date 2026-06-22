using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 欧标交流研测：断开插接式EV充电设备的连接
    /// </summary>
    public class CCS2_RT_AC_EVPlugConnectedDisconnectionTest : BusinessBase
    {
        string itemFlow = "";
        private int CheckTime = 20;//人工检测时间 秒
        public CCS2_RT_AC_EVPlugConnectedDisconnectionTest(int type)
        {
            TrialType = type;
        }

        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            string[] ItemParams = TrialItem.ItemParams.Split('|')[0].Split('=');
            if (ItemParams.Length == 2)
            {
                CheckTime = Convert.ToInt32(ItemParams[1]);
            }
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
                List<bool> Ks = GetKStatus16_Charging();
                Ks[0] = false;
                Ks[9] = false;
                ControlEquipMent.BMS.BMS_SetKState(lstIDs, Ks);

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

                if (testWorkParam.lstIDs.Count == 0 || !CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
                //设置测试条件
                SetConditionValues();

                itemFlow = "插接式EV断开连接";
                CountDownTimeInfo("请确定拔出插头后即可触及连接插针，从标准插座上拔下标准插头一秒钟后，标准插头可触及触点的任意组合之间的电压应小于或等于60V DC或可用的存储电荷应小于50 µC。(勾选为PASS)\r\n倒计时结束默认PASS", CheckTime, 2);
                ProcessData();
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
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                double volate = AllEquipStateData.DicBMS_AC_StateData[LstTrialData[k].ChargerId].PhaseA_Voltage;
                if (DicManualVerifyResult[item])
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     
                LstTrialData[k].ExtentData = $"{itemFlow}|-|-|-|-";
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                LstTrialData[k].Data3 = TrialItem.TrialOrder.ToString();
                SendTrialDataToUI(LstTrialData[k]);
                SaveTrialData(LstTrialData[k]);
            }
        }
    }
}
