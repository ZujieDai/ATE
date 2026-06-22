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
    /// 欧标交流研测：一般要求测试
    /// </summary>
    public class CCS2_RT_AC_GeneralRequirementsTest : BusinessBase
    {
        string itemFlow = "";
        public CCS2_RT_AC_GeneralRequirementsTest(int type) { TrialType = type; }

        int CheckTime = 10;//人工检测时间（秒）

        public override void InitializeParams()
        {
            Init();
            string[] strParams = TrialItem.ResultParams.Split('|');
            CheckTime = Convert.ToInt32(double.Parse(strParams[0].Split('=')[1]));
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

                SendNoticeToUIAndTxtFile(TrialItem.ItemName + "结束---------------------->");
                SaveTrialResult();
                SendMessageEndThisTrial();
            }
        }

        public void StartItemFlow()
        {
            try
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
                                    //界面展示的数据项格式                              
                                    LstTrialData[i].ExtentData = "null|null|null|null|null";

                                    SendTrialDataToUI(LstTrialData[i]);
                                }
                            }
                        }
                        break;
                    }
                    if (testWorkParam.lstIDs.Count == 0)
                    {
                        return;
                    }

                    //设置测试条件
                    SetConditionValues();

                    //itemFlow = "The energy transfer operates safely";
                    //string info = $"【General Requirements】The EV supply equipment shall be so constructed that an EV can be connected to the EV supply equipment so that in normal conditions of use, the energy transfer operates safely, and its performance is reliable and minimises the risk of danger to the user or surroundings.";
                    itemFlow = "用户或周围环境安全";
                    string info = $"EV 充电设备的构造应使 EV 可以连接到 EV 充电设备，以便在正常使用条件下，能量传输可以安全地进行，并且其性能可靠，并且可以将对用户或周围环境的危险降至最低。\r\n请确认是否合格(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    //itemFlow = "Meeting the IEC 61851 series";
                    //info = $"【General Requirements】Compliance is checked by meeting all of the relevant requirements of the IEC 61851 series.";
                    itemFlow = "满足 IEC 61851 系列";
                    info = $"通过满足 IEC 61851 系列的所有相关要求来检查是否合格。\r\n请确认是否合格(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    //itemFlow = "This document are type tests";
                    //info = $"【General Requirements】Unless otherwise stated all tests indicated in this document are type tests.";
                    itemFlow = "所有测试均为类型测试";
                    info = $"除非另有说明，否则本文档中指示的所有测试均为类型测试。\r\n请确认是否合格(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    //itemFlow = "Separate samples";
                    //info = $"【General RequirementstemFlow】Unless otherwise stated, all tests required by this standard may be conducted on separate samples. They may be done on the same samples at the manufacturer's agreement.";
                    itemFlow = "单独的样品进行测试";
                    info = $"除非另有说明，否则本标准要求的所有测试均可在单独的样品上进行。根据制造商的协议，它们可以在相同的样品上进行。\r\n请确认是否合格(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    //itemFlow = "Each test is conducted once";
                    //info = $"【General Requirements】Unless otherwise stated, each test is conducted once.";
                    itemFlow = "每个测试都进行一次";
                    info = $"除非另有说明，否则每个测试都进行一次。\r\n请确认是否合格(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    //itemFlow = "Right location";
                    //info = $"【General Requirements】Unless otherwise specified, all tests shall be carried out in a draught-free location and at an ambient temperature of 20°± 5℃.";
                    itemFlow = "正确的环境进行测试";
                    info = $"除非另有说明，否则所有测试均应在无通风的地方以及 20°±5°C 的环境温度下进行。\r\n请确认是否合格(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    //itemFlow = "Nominal voltages and frequencies";
                    //info = $"【General Requirements】The EV supply equipment shall be rated for one or more of standard nominal voltages and frequencies as given in IEC 60038.";
                    itemFlow = "额定电压符合";
                    info = $"EV 充电设备的额定电压应符合 IEC 60038 中给出的一个或多个标准标称电压和频率。\r\n请确认是否合格(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    //itemFlow = "Comply with IEC TS 61439-7";
                    //info = $"【General Requirements】Assemblies for EV supply equipment shall comply with IEC TS 61439-7 with the exceptions or additions as indicated in Clause 13.";
                    itemFlow = "对频率有不同的要求";
                    info = $"注意在以下国家/地区，国家标准或法规对频率有不同的要求：日本。\r\n请确认是否合格(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    //itemFlow = "Altitudes 2000 m";
                    //info = $"【General Requirements】The standard applies to equipment that is designed to be used at an altitude up to 2000 m.";
                    itemFlow = "符合 IEC TS 61439-7 的规定";
                    info = $"电动汽车充电设备的组件应符合 IEC TS 61439-7 的规定，但第 13 条中规定的例外或增补除外。该标准适用于设计用于高达 2000 m 的海拔高度的设备。\r\n请确认是否合格(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();

                    //itemFlow = "Altitudes above 2000 m";
                    //info = $"【General Requirements】For equipment designed to be used at altitudes above 2000 m, it is necessary to take into account the reduction of the dielectric strength and the cooling effect of the air. Electrical equipment intended to operate under these conditions shall be designed or used in accordance with an agreement between manufacturer and user.";
                    itemFlow = "海拔高于2000m";
                    info = $"对于设计用于海拔 2 000 m 以上的设备，必须考虑绝缘强度的降低和空气的冷却效果。在这些条件下运行的电气设备应按照制造商与用户之间的协议进行设计或使用。\r\n请确认是否合格(勾选上为合格)";
                    CountDownTimeInfo(info, CheckTime, 2);
                    ProcessData();
                }
            }
            catch (Exception ex) { Log.Log.LogException(ex); }
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
                LstTrialData[k].ItemName = iIndex.ToString();

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
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);
            }
            iIndex++;
        }
    }
}
