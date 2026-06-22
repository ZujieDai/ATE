using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SaiTer.ATE.InterFace;
using Newtonsoft.Json;
using SaiTer.ATE.MES;

namespace SaiTer.ATE.Business
{
    /// <summary>
    /// 枪头RC电阻测试    惠州TB客户定制项  交流国标
    /// </summary>
    public class RcResistance_GB_TBAuto : BusinessBase
    {
        bool IsCardCharg = false;   //是否为刷卡充电

        double maxp = 1.1;

        double minp = 1.1;

        /// <summary>
        /// 是否需要测试电子锁解锁状态   1=需要测   
        /// </summary>
        bool isCheckUnLock = true;

        double MinOpenResistance, MaxOpenResistance, MinCloseResistance, MaxCloseResistance;


        public RcResistance_GB_TBAuto(int type)
        {
            TrialType = type;
        }
        public override void InitializeParams()
        {
            Init();

            //数据库参数格式
            //误差(%)=8|机械锁解锁电阻值(Ω)=220|机械锁锁止电阻值(Ω)=3500|是否有机械锁(1代表有)=0|桩类型（1为刷卡桩，否则为0）=0
            string[] strParams = TrialItem.ResultParams.Split('|');
            double OpenResistance = 0;
            double CloseResistance = 0;

            maxp = (100 + double.Parse(strParams[0].Split('=')[1])) / 100;
            minp = (100 - double.Parse(strParams[0].Split('=')[1])) / 100;
            if (strParams.Length >= 3)
            {
                CloseResistance = double.Parse(strParams[1].Split('=')[1]);
                OpenResistance = double.Parse(strParams[2].Split('=')[1]);
                IsCardCharg = double.Parse(strParams[4].Split('=')[1]) == 1;
            }
            MinOpenResistance = OpenResistance * minp;
            MaxOpenResistance = OpenResistance * maxp;
            MinCloseResistance = CloseResistance * minp;
            MaxCloseResistance = CloseResistance * maxp;

            if (strParams.Length >= 4)
            {
                isCheckUnLock = Convert.ToInt32(double.Parse(strParams[3].Split('=')[1])) == 1;
            }
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
                // TB欧标桩需要刷卡才能结束充电，并且等待CP波纹和充电电压为0
                if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_EUR_AC && IsCardCharg)
                {
                    CountDownTimeInfo("请刷卡终止充电，并等待充电桩结算后点击确认", 999, 0);
                    int i = 500;
                    while (i-- > 0)
                    {
                        if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].CPDutyCycle == 0 && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage < 20)
                        {
                            //双重判断
                            Thread.Sleep(100);
                            if (AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].CPDutyCycle == 0 && AllEquipStateData.DicBMS_AC_StateData[lstIDs[0]].PhaseA_Voltage < 20)
                                break;
                        }
                        Thread.Sleep(100);
                    }
                }
                //SetCPReresh();

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
                //if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC)
                //{
                //    //因为要测试电阻，强行改变为国标用来检测电阻
                //    ControlEquipMent.BMS.BMSSetHCAC(testWorkParam.lstIDs, EmChargerType.Charger_GB_AC);
                //    Thread.Sleep(2000);
                //}
                ControlEquipMent.BMS.BMSSetHCAC(testWorkParam.lstIDs, LstChargerInfo[0].ChargerType);
                Thread.Sleep(2000);

                List<bool> lst = GetKStatus16_Charging();
                lst[0] = false;
                lst[9] = false;
                ControlEquipMent.BMS.BMS_SetKState(testWorkParam.lstIDs, lst);
                Thread.Sleep(1000);
                SetConditionValues();

                Dictionary<int, string> dic = new Dictionary<int, string>();
                if (isCheckUnLock)
                {
                    string json = JsonConvert.SerializeObject(new
                    {
                        MasterMaterialBarCode = TBHttpMESAutomation.GetInstance().MasterMaterialBarCode,
                        Data = new
                        {
                            ActionType = "2",
                            Action = "1"
                        }
                    });
                    bool isOK = false;
                    string res = HttpBase.HttpPost(TBHttpMESAutomation.GetInstance().PostUrl + "autoline/action", json);
                    SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 已发送按下枪头按钮动作");
                    //Thread.Sleep(2000);
                    TBHttpMESAutomation.GetInstance().actionresultDo = (ActionType, ActionResult) =>
                    {
                        if (ActionType == "2" && ActionResult == "1")
                        {
                            isOK = true;
                        }
                    };
                    int timeout = 50;
                    while (timeout-- > 0)
                    {
                        if (isOK)
                            break;
                        Thread.Sleep(100);
                    }
                    TBHttpMESAutomation.GetInstance().actionresultDo = null;

                    foreach (var item in testWorkParam.lstIDs)
                    {
                        //double CCResistance = AllEquipStateData.DicBMS_AC_StateData[item].CCResistance;
                        double CCResistance = GetCCResistance(item);
                        int time = 10;
                        while (time-- > 0)
                        {
                            if(CCResistance >= MinOpenResistance && CCResistance <= MaxOpenResistance)
                            {
                                break;
                            }
                            Thread.Sleep(300);
                            CCResistance = GetCCResistance(item);
                        }
                        dic.Add(item, CCResistance.ToString("F2"));
                    }
                    ProcessDataTmp(dic, "枪头状态", "枪头解锁", MinOpenResistance.ToString(), MaxOpenResistance.ToString());//解锁状态

                    isOK = false;
                    json = JsonConvert.SerializeObject(new
                    {
                        MasterMaterialBarCode = TBHttpMESAutomation.GetInstance().MasterMaterialBarCode,
                        Data = new
                        {
                            ActionType = "2",
                            Action = "2"
                        }
                    });
                    res = HttpBase.HttpPost(TBHttpMESAutomation.GetInstance().PostUrl + "autoline/action", json);
                    SystemEvent.SendLogMessage("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 已发送松开枪头按钮动作");
                    //Thread.Sleep(5000);
                    TBHttpMESAutomation.GetInstance().actionresultDo = (ActionType, ActionResult) =>
                    {
                        if (ActionType == "2" && ActionResult == "1")
                        {
                            isOK = true;
                        }
                    };
                    timeout = 50;
                    while (timeout-- > 0)
                    {
                        if (isOK)
                            break;
                        Thread.Sleep(100);
                    }
                    TBHttpMESAutomation.GetInstance().actionresultDo = null;
                }
                dic.Clear();
                foreach (var item in testWorkParam.lstIDs)
                {
                    //double CCResistance = AllEquipStateData.DicBMS_AC_StateData[item].CCResistance;
                    double CCResistance = GetCCResistance(item);
                    int time = 10;
                    while (time-- > 0)
                    {
                        if (CCResistance >= MinOpenResistance && CCResistance <= MaxOpenResistance)
                        {
                            break;
                        }
                        Thread.Sleep(300);
                        CCResistance = GetCCResistance(item);
                    }
                    dic.Add(item, CCResistance.ToString("F2"));
                }
                ProcessDataTmp(dic, "枪头状态", "枪头锁止", MinCloseResistance.ToString(), MaxCloseResistance.ToString());//锁止状态
                //if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC)
                //{                                                                                   ////恢复导引类型来读取数据
                //    ControlEquipMent.BMS.BMSSetHCAC(testWorkParam.lstIDs, LstChargerInfo[0].ChargerType);
                //    Thread.Sleep(2000);
                //}
            }
        }

        private double GetCCResistance(int item)
        {
            double CCResistance = AllEquipStateData.DicBMS_AC_StateData[item].CCResistance;
            if (LstChargerInfo[0].ChargerType == EmChargerType.Charger_USA_AC)
            {
                double It = (5 - CCResistance) / 330;
                double Rt = CCResistance / It;
                double R5 = 3000;
                double Rq = R5 * Rt / (R5 - Rt);
                CCResistance = Rq;
            }
            return CCResistance;
        }

        public override void ProcessData()
        {
        }
    }
}
