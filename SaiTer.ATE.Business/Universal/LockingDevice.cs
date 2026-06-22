using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
/*
 *锁止装置检查	
a) 对于采用连接方式 A 或连接方式 B 的充电桩，当充电桩额定电流大于 16 A 时，检查供电插座
应安装具有位置反馈功能的电子锁止装置；

b) 充电连接装置完全连接并启动充电桩，检查检测点 1 或检测点 4 的电压值应在允许充电的范围
内，确认供电接口和/或车辆接口的机械锁止有效性；通过检查电子锁反馈信号是否与实际锁止状态对应，确认充电桩电子锁止有效性；通过检查机械锁止装置是否能被打开，确认电子锁止装置对机械锁止装置的联锁效果；

c) 当电子锁未可靠锁止时，检查充电桩不应允许充电。在整个充电过程中，检查充电桩电子锁应可靠锁止，不允许带电解锁且不应由人手直接操作解锁；

d) 正常充电结束后交流供电回路切断 100 ms 内，检查电子锁不应被解锁；

e) 检查电子锁装置应具备应急解锁功能。	
 
 */
namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 锁止装置测试/车辆插头锁止功能
    /// </summary>
    public class LockingDevice : BusinessBase
    {

        private int CheckTime = 20;//人工检测时间 秒
        public LockingDevice(int type)
        {
            TrialType = type;
        }

        public override void InitEquiMent()
        {

        }

        public override void InitializeParams()
        {
            Init();

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

                if (testWorkParam.lstIDs.Count == 0 || !CheckSwipingCard(testWorkParam.lstIDs))
                {
                    return;
                }
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
                Thread.Sleep(1000);
                SetConditionValue("供电电压(V)", d1);
                SetConditionValue("供电频率(Hz)", d2);
                SendNoticeToUIAndTxtFile("启动负载");
                SetResisLoadVolCurrAndStart(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].NominalCurrent / 2);
                //ControlEquipMent.ResistanceLoad.SetResisLoadVolCurr(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, LstChargerInfo[0].NominalCurrent);
                //Thread.Sleep(200);
                //ControlEquipMent.ResistanceLoad.ResistanceLoad_ON(testWorkParam.lstIDs);
                Thread.Sleep(3000);

                List<bool> Ks = GetKStatus16_Charging();
                Ks[9] = true;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, Ks);

                CountDownTimeInfo("请确认车辆插头电子锁应可靠锁止(勾选为PASS)\r\n倒计时结束默认PASS", CheckTime, 2);
                ProcessData();


                ControlEquipMent.ResistanceLoad.ResistanceLoad_OFF(testWorkParam.lstIDs);
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
                LstTrialData[k].ExtentData = TrialItem.ItemName + "|-|-|-|-";
                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                SendTrialDataToUI(LstTrialData[k]);
                SaveTrialData(LstTrialData[k]);
            }
        }
    }
}
