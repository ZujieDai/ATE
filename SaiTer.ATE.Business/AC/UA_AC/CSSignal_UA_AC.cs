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
    /// 美标交流：CS信号测试
    /// </summary>
    public class CSSignal_UA_AC : BusinessBase
    {
        public CSSignal_UA_AC(int type)
        {
            TrialType = type;
        }
        public override void InitializeParams()
        {
            Init();

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
                // SetCPReresh();

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


                //因为要测试电阻，强行改变为国标用来检测电阻
                ControlEquipMent.BMS.BMSSetHCAC(testWorkParam.lstIDs, EmChargerType.Charger_USA_AC);
                Thread.Sleep(5000);

                List<bool> lst = GetKStatus16_Charging();
                lst[0] = false;
                lst[2] = false;//CC
                lst[9] = false;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, lst);
                Thread.Sleep(500);
                Dictionary<int, string> dic = new Dictionary<int, string>();
                foreach (var item in testWorkParam.lstIDs)
                {
                    double CCResistance = AllEquipStateData.DicBMS_AC_StateData[item].CCResistance;
                    dic.Add(item, CCResistance.ToString("F2"));
                }
                ProcessDataTmp(dic, "CS电压", "未连接", "4.13", "4.78");//解锁状态

                //CountDownTimeInfo("请判断桩是否有电子锁！", 10, 2);
                //ProcessData();
                //if (testWorkParam.lstIDs.Count == 0)
                //{
                //    return;
                //}
                lst = GetKStatus16_Charging();
                lst[9] = false;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, lst);
                Thread.Sleep(500);
                CountDownTimeInfo("请按住机械锁！", 10, 0);
                dic = new Dictionary<int, string>();
                foreach (var item in testWorkParam.lstIDs)
                {
                    double CCResistance = AllEquipStateData.DicBMS_AC_StateData[item].CCResistance;
                    dic.Add(item, CCResistance.ToString("F2"));
                }
                ProcessDataTmp(dic, "CS电压", "按下机械锁", "2.38", "3.16");//解锁状态



                CountDownTimeInfo("请松开机械锁！", 5, 0);
                dic.Clear();
                foreach (var item in testWorkParam.lstIDs)
                {
                    double CCResistance = AllEquipStateData.DicBMS_AC_StateData[item].CCResistance;
                    dic.Add(item, CCResistance.ToString("F2"));
                }
                ProcessDataTmp(dic, "CS电压", "松开机械锁", "1.23", "1.82");//锁止状态

                ////恢复导引类型来读取数据
                ControlEquipMent.BMS.BMSSetHCAC(testWorkParam.lstIDs, LstChargerInfo[0].ChargerType);
                Thread.Sleep(2000);

            }
        }
        public override void ProcessData()
        {

        }
    }
}
