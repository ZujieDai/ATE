using SaiTer.ATE.Business.ProtocolSummary;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.CAN;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Protocol;
using SaiTer.ATE.InterFace;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public partial class BusinessBase
    {
        #region Params
        private const int DCWaitTime = 100;
        public int ProtocolNum = -1;   // 协议一致性编号
        bool IsCard = false;
        #endregion

        #region
        public void InitializationProtocol()
        {
            byte byte0 = 0, byte1 = 0, byte2 = 0, byte3 = 0, byte4 = 0, byte5 = 0, byte6 = 0, byte7 = 0;
            switch (TrialType)
            {
                case (int)EmTrialType.DP1001:
                    byte0 = 1 << 7;
                    ProtocolNum = 0;
                    break;
                case (int)EmTrialType.DP1002:
                    byte0 = 1 << 6;
                    ProtocolNum = 1;
                    break;
                case (int)EmTrialType.DP1003:
                    byte0 = 1 << 5;
                    ProtocolNum = 2;
                    break;
                case (int)EmTrialType.DN1001:
                    byte0 = 1 << 4;
                    ProtocolNum = 3;
                    break;
                case (int)EmTrialType.DN1002:
                    byte0 = 1 << 3;
                    ProtocolNum = 4;
                    break;
                case (int)EmTrialType.DN1003:
                    byte0 = 1 << 2;
                    ProtocolNum = 5;
                    break;
                case (int)EmTrialType.DN1004:
                    byte0 = 1 << 1;
                    ProtocolNum = 6;
                    break;
                case (int)EmTrialType.DP2001:
                    byte0 = 1;
                    ProtocolNum = 7;
                    break;
                case (int)EmTrialType.DP2002:
                    byte1 = 1 << 7;
                    ProtocolNum = 8;
                    break;
                case (int)EmTrialType.DP2003:
                    byte1 = 1 << 6;
                    ProtocolNum = 9;
                    break;
                case (int)EmTrialType.DN2001:
                    byte1 = 1 << 5;
                    ProtocolNum = 10;
                    break;
                case (int)EmTrialType.DN2002:
                    byte1 = 1 << 4;
                    ProtocolNum = 11;
                    break;
                case (int)EmTrialType.DN2003:
                    byte1 = 1 << 3;
                    ProtocolNum = 12;
                    break;
                case (int)EmTrialType.DN2004:
                    byte1 = 1 << 2;
                    ProtocolNum = 13;
                    break;
                case (int)EmTrialType.DN2005:
                    byte1 = 1 << 1;
                    ProtocolNum = 14;
                    break;
                case (int)EmTrialType.DN2006:
                    byte1 = 1;
                    ProtocolNum = 15;
                    break;
                case (int)EmTrialType.DN2007:
                    byte2 = 1 << 7;
                    ProtocolNum = 16;
                    break;
                case (int)EmTrialType.DN2008:
                    byte2 = 1 << 6;
                    ProtocolNum = 17;
                    break;
                case (int)EmTrialType.DN2009:
                    byte2 = 1 << 5;
                    ProtocolNum = 18;
                    break;
                case (int)EmTrialType.DN2010:
                    byte2 = 1 << 4;
                    ProtocolNum = 19;
                    break;
                case (int)EmTrialType.DP3001:
                    byte2 = 1 << 3;
                    ProtocolNum = 20;
                    break;
                case (int)EmTrialType.DP3002:
                    byte2 = 1 << 2;
                    ProtocolNum = 21;
                    break;
                case (int)EmTrialType.DP3003:
                    byte2 = 1 << 1;
                    ProtocolNum = 22;
                    break;
                case (int)EmTrialType.DP3004:
                    byte2 = 1;
                    ProtocolNum = 23;
                    break;
                case (int)EmTrialType.DP3005:
                    byte3 = 1 << 7;
                    ProtocolNum = 24;
                    break;
                case (int)EmTrialType.DP3006:
                    byte3 = 1 << 6;
                    ProtocolNum = 25;
                    break;
                case (int)EmTrialType.DP3007:
                    byte3 = 1 << 5;
                    ProtocolNum = 26;
                    break;
                case (int)EmTrialType.DN3001:
                    byte3 = 1 << 4;
                    ProtocolNum = 27;
                    break;
                case (int)EmTrialType.DN3002:
                    byte3 = 1 << 3;
                    ProtocolNum = 28;
                    break;
                case (int)EmTrialType.DN3003:
                    byte3 = 1 << 3;
                    ProtocolNum = 29;
                    break;
                case (int)EmTrialType.DN3004:
                    byte3 = 1 << 3;
                    ProtocolNum = 30;
                    break;
                case (int)EmTrialType.DN3005:
                    byte3 = 1 << 3;
                    ProtocolNum = 31;
                    break;
                case (int)EmTrialType.DN3006:
                    byte3 = 1 << 3;
                    ProtocolNum = 32;
                    break;
                case (int)EmTrialType.DN3007:
                    byte3 = 1 << 3;
                    ProtocolNum = 33;
                    break;
                case (int)EmTrialType.DN3008:
                    byte3 = 1 << 3;
                    ProtocolNum = 34;
                    break;
                case (int)EmTrialType.DN3009:
                    byte3 = 1 << 3;
                    ProtocolNum = 35;
                    break;
                case (int)EmTrialType.DN3010:
                    byte3 = 1 << 3;
                    ProtocolNum = 36;
                    break;
                case (int)EmTrialType.DP4001:
                    byte3 = 1 << 3;
                    ProtocolNum = 37;
                    break;
                case (int)EmTrialType.DP4002:
                    byte3 = 1 << 3;
                    ProtocolNum = 38;
                    break;
                case (int)EmTrialType.DN4001:
                    byte3 = 1 << 3;
                    ProtocolNum = 39;
                    break;
                case (int)EmTrialType.DN4002:
                    byte3 = 1 << 3;
                    ProtocolNum = 40;
                    break;
                case (int)EmTrialType.DN4003:
                    byte3 = 1 << 3;
                    ProtocolNum = 41;
                    break;
                case (int)EmTrialType.DN4004:
                    byte3 = 1 << 3;
                    ProtocolNum = 42;
                    break;
            }
            //国标DC协议一致性测试设置
            ControlEquipMent.BMS.BMSProtocolConsistency(testWorkParam.lstIDs, byte0, byte1, byte2, byte3, byte4, byte5, byte6, byte7);
        }

        /// <summary>
        /// 全部置为0，恢复0x70设置的异常
        /// </summary>
        public void RecoverProtocolConsist()
        {
            byte byte0 = 0, byte1 = 0, byte2 = 0, byte3 = 0, byte4 = 0, byte5 = 0, byte6 = 0, byte7 = 0;
            //国标DC协议一致性测试设置
            ControlEquipMent.BMS.BMSProtocolConsistency(testWorkParam.lstIDs, byte0, byte1, byte2, byte3, byte4, byte5, byte6, byte7);
        }
        #endregion
        /// <summary>
        /// 国标指令启动充电桩充电
        /// </summary>
        public void Charger_Start_DC()
        {
            IsCard = false;
            ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage);
            Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, 390, LstChargerInfo[0].NominalVoltage, 250);
            Thread.Sleep(200);
            ControlEquipMent.BMS.SetParameter(testWorkParam.lstIDs, LstChargerInfo[0].NominalVoltage, 250, true, LstChargerInfo[0].NominalVoltage);
            Thread.Sleep(200);
            ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);


            MessgaeInfo(true, "请刷卡充电!", true);
            int timeout = 3000;
            while (timeout-->0)
            {
                var bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(c => testWorkParam.lstIDs.Contains(c.Value.ChargerID)).ToDictionary(bms => bms.Key, bms => bms.Value);
                if (bmsData.Count() < 1 || bmsData.Values.FirstOrDefault() == null)
                    continue;
                bool ALLCanCharge = bmsData.All(c => ChangeBMSChargeStatus(c.Value.ChargingState) >2 && ChangeBMSChargeStatus(c.Value.ChargingState) <= 9);
                if (ALLCanCharge)
                {
                    MessgaeInfo(false, "请刷卡充电!");
                    IsCard = true;
                    break;
                }
                bool Visible = SystemEvent.GetMessageInfo();
                if(!Visible)
                {
                    IsCard = true;
                    break;
                }

                System.Threading.Thread.Sleep(100);
            }
            MessgaeInfo(false, "请刷卡充电!");
            System.Threading.Thread.Sleep(2000);
        }

        /// <summary>
        /// 国标发送充电含插拔枪
        /// </summary>
        public void ChargingStart()
        {
            IsCard = false;
            SetCPReresh();
            ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);
        }

        /// <summary>
        /// 停止充电桩充电
        /// </summary>
        public void Charger_Stop(bool IsChargerAuto = false)
        {

            if (IsChargerAuto)
            {

            }
            else
            {
                CountDownTimeInfo("请操作充电桩主动中止充电，待操作完成后，点击确定！！！", 99999, 0);
                IsCard = false;
            }
        }

        #region 协议一致性函数
        public void CreatTestContent()
        {
            switch (ProtocolNum)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                    DP_1001_Test();
                    break;
                case 18:
                    DN_2009_Test();
                    break;
                case 19:
                case 20:
                case 21:
                    DP_1001_Test();
                    break;
                case 22:
                case 23:
                    DP_3006_Test();
                    break;
                case 24:
                    DP_3005_Test();
                    break;
                case 25:
                    DP_3006_Test();
                    break;
                case 26://充电机主动终止充电
                    DP_3007_Test();
                    break;
                case 27:
                case 28:
                case 29:
                case 30:
                case 31:
                case 32:
                case 33:
                case 34:
                    DP_1001_Test();
                    break;
                case 35://充电机主动终止充电
                case 36:
                    DP_3007_Test();
                    break;
                case 37:
                    DP_1001_Test();
                    break;
                case 38:
                    DP_4002_Test();
                    break;
                case 39:
                case 40:
                    DP_1001_Test();
                    break;
                case 41:
                case 42:
                    DP_3007_Test();
                    break;
            }
        }

        /// <summary>
        /// //判断提交结果
        /// </summary>
        public void CacleConsistRslt()
        {
            // 关闭报文捕捉
            ControlEquipMent.BMS.BMS_DC_SetControl(testWorkParam.lstIDs, 0x50, false);
            System.Threading.Thread.Sleep(500);

            #region 计算并提交结果
            var dicCANData = ControlEquipMent.BMS.GetCANDATA(testWorkParam.lstIDs);
            var keys = dicCANData.Keys.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                int index = keys[i];
                var CANData = dicCANData[index];
                // 设置起始报文（以第一帧CHM报文为开始）
                if (CANData != null && CANData.Count > 0 && !CANData.FirstOrDefault().MsgText.Contains("CHM"))
                {
                    int startIndex = CANData.FindIndex(can => can.MsgText.Contains("CHM"));
                    CANData = CANData.Skip(startIndex).ToList();
                    dicCANData[index] = CANData;
                }
            }
            // 判断测试结果
            CheckProtocol(TrialType, dicCANData);
            #endregion

            // 打开报文捕捉
            ControlEquipMent.BMS.BMS_DC_SetControl(testWorkParam.lstIDs, 0x50, true);
        }

        public void CheckProtocol(int TrialType, Dictionary<int, List<DataModel.CAN.CanMsgRich>> DicCANData)
        {
           
            switch (TrialType)
            {
                case (int)EmTrialType.DP1001:
                    Consist_DP1001 DP1001 = new Consist_DP1001(this);
                    DP1001.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DP1002:
                    Consist_DP1002 DP1002 = new Consist_DP1002(this);
                    DP1002.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DP1003:
                    Consist_DP1003 DP1003 = new Consist_DP1003(this);
                    DP1003.CacleConsistRslt(DicCANData);

                    break;
                case (int)EmTrialType.DN1001:
                case (int)EmTrialType.DN1002:
                case (int)EmTrialType.DN1003:
                    Consist_DN1001to03 DN1001to03 = new Consist_DN1001to03(this);
                    DN1001to03.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DN1004:
                    Consist_DN1004 DN1004 = new Consist_DN1004(this);
                    DN1004.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DP2001:
                    Consist_DP2001 DP2001 = new Consist_DP2001(this);
                    DP2001.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DP2002:
                    Consist_DP2002 DP2002 = new Consist_DP2002(this);
                    DP2002.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DP2003:
                    Consist_DP2003 DP2003 = new Consist_DP2003(this);
                    DP2003.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DN2001:
                case (int)EmTrialType.DN2002:
                    Consist_DN2001to02 DN2001to02 = new Consist_DN2001to02(this);
                    DN2001to02.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DN2003:
                case (int)EmTrialType.DN2004:
                    Consist_DN2003to04 DN2003To04 = new Consist_DN2003to04(this);
                    DN2003To04.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DN2005:
                    Consist_DN2005 DN2005= new Consist_DN2005(this);
                    DN2005.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DN2006:
                    Consist_DN2006 DN2006 = new Consist_DN2006(this);
                    DN2006.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DN2007:
                case (int)EmTrialType.DN2008:
                    Consist_DN2007to08 DN2007To08 = new Consist_DN2007to08(this);
                    DN2007To08.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DN2009:
                    Consist_DN2009 DN2009 = new Consist_DN2009(this);
                    DN2009.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DN2010:
                    Consist_DN2010 DN2010 = new Consist_DN2010(this);
                    DN2010.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DP3001:
                    Consist_DP3001 DP3001 = new Consist_DP3001(this);
                    DP3001.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DP3002:
                    Consist_DP3002 DP3002 = new Consist_DP3002(this);
                    DP3002.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DP3003:
                    Consist_DP3003 DP3003 = new Consist_DP3003(this);
                    DP3003.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DP3004:
                    Consist_DP3004 DP3004 = new Consist_DP3004(this);
                    DP3004.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DP3005:
                    Consist_DP3005 DP3005 = new Consist_DP3005(this);
                    DP3005.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DP3006:
                    Consist_DP3006 DP3006 = new Consist_DP3006(this);
                    DP3006.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DP3007:
                    Consist_DP3007 DP3007 = new Consist_DP3007(this);
                    DP3007.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DN3001:
                    Consist_DN3001 DN3001 = new Consist_DN3001(this);
                    DN3001.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DN3002:       
                case (int)EmTrialType.DN3004:
                    Consist_DN3002_04 DN3002_04 = new Consist_DN3002_04(this);
                    DN3002_04.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DN3003:
                    Consist_DN3003 DN3003 = new Consist_DN3003(this);
                    DN3003.CacleConsistRslt(DicCANData);
                    break;

                case (int)EmTrialType.DN3005:
                case (int)EmTrialType.DN3007:
                    Consist_DN3005_07 DN3005_07 = new Consist_DN3005_07(this);
                    DN3005_07.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DN3006:
                case (int)EmTrialType.DN3008:
                    Consist_DN3006_08 DN3006_08 = new Consist_DN3006_08(this);
                    DN3006_08.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DN3009:
                case (int)EmTrialType.DN3010:
                    Consist_DN3009to10 DN3009To10 = new Consist_DN3009to10(this);
                    DN3009To10.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DP4001:
                    Consist_DP4001 DP4001 = new Consist_DP4001(this);
                    DP4001.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DP4002:
                    Consist_DP4002 DP4002 = new Consist_DP4002(this);
                    DP4002.CacleConsistRslt(DicCANData);
                    break;
                case (int)EmTrialType.DN4001:
                case (int)EmTrialType.DN4002:
                case (int)EmTrialType.DN4003:
                case (int)EmTrialType.DN4004:
                    Consist_DN4001to04 DN4001To04 = new Consist_DN4001to04(this);
                    DN4001To04.CacleConsistRslt(DicCANData);
                    break;
                default:
                    break;
            }

        }

        private void DP_1001_Test()
        {
            try
            {
                IsCard=false;
                Charger_Start_DC();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Thread.Sleep(1000);
                while (true)
                {
                    // 判断是否停止充电，通过can通讯来判断
                    var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
                    bool isStop = true;
                    foreach (bool item in dic_isStop.Values)
                    {
                        if (!item) isStop = false; break;
                    }
                    if (isStop) break;
                    // 超时
                    if (stopwatch.ElapsedMilliseconds > DCWaitTime * 1000)
                    {
                        break;
                    }
                }

                ControlEquipMent.BMS.BMS_OFF(lstIDs);
                stopwatch.Restart();
                while (true)
                {
                    // 判断是否停止充电，通过can通讯来判断
                    var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
                    bool isStop = true;
                    foreach (bool item in dic_isStop.Values)
                    {
                        if (!item) isStop = false; break;
                    }
                    if (isStop) break;

                    if (stopwatch.ElapsedMilliseconds > 15 * 1000)
                    {
                        break;
                    }
                }

                ////判断提交结果
                CacleConsistRslt();

            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
            }
            finally
            {
                ControlEquipMent.BMS.BMS_OFF(lstIDs);
            }
        }

        private void DN_2009_Test()
        {
            try
            {
                IsCard = false;
                Dictionary<int, bool> tmpRslts = new Dictionary<int, bool>();
                bool isK3K4 = true;//是否判断K3K4时间（目前只有公牛下位机有兼容）

                //自动启动桩体充电
                Charger_Start_DC();

                DateTime dts = DateTime.Now;
                string sShowMsg = "";
                while (true)
                {
                    //判断是否停止充电，通过can通讯来判断
                    //Prj.Prj.CanMsgController.IsStopCharge = Prj.Prj.CanMsgController.IsCanStop;
                    Dictionary<int, bool> dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
                    bool isStop = true;
                    foreach (bool item in dic_isStop.Values)
                    {
                        if (!item) isStop = false; break;
                    }
                    foreach (var item in dic_isStop)
                    {
                        if (tmpRslts.ContainsKey(item.Key))
                            tmpRslts[item.Key] = item.Value;
                        else
                            tmpRslts.Add(item.Key, item.Value);
                    }
                    if (isStop)
                    {
                        break;
                    }

                    if (dts.AddSeconds(90) < DateTime.Now)//90
                    {
                        //tmprslt = false;
                        //bRslt &= false; 
                        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                        {
                            if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
                            {
                                tmpRslts[testWorkParam.lstIDs[i]] = false;
                            }
                            else
                            {
                                tmpRslts[testWorkParam.lstIDs[i]] = true;
                            }
                        }
                        //SetCanClose();
                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(15000);
                        //SetCanOpen();
                        break;
                    }
                }
                //for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                //{
                //    if (tmpRslts[testWorkParam.lstIDs[i]])
                //    {
                //        //tir = CacleCanMsgRslt();
                //    }
                //}
                ////判断提交结果
                CacleConsistRslt();

                string stmp = "";

                ////K3K4断开时间
                if (isK3K4)//需要判断才判断
                {
                    Dictionary<int, int> K3K4Times = ControlEquipMent.BMS.GetK3K4StopTime(testWorkParam.lstIDs);

                    dts = DateTime.Now;
                    while (true)
                    {
                        // K3K4断开时间重复获取5s
                        bool IsAllCorrectValue = K3K4Times.Where(kvp => testWorkParam.lstIDs.Contains(kvp.Key)).All(kvp => kvp.Value != -999);
                        if (IsAllCorrectValue || dts.AddSeconds(5) < DateTime.Now)
                        {
                            break;
                        }
                        K3K4Times = ControlEquipMent.BMS.GetK3K4StopTime(testWorkParam.lstIDs);
                    }
                    Dictionary<int, BMS_DC_StateData> bmsData = AllEquipStateData.DicBMS_DC_StateData.Where(kvp => testWorkParam.lstIDs.Contains(kvp.Value.ChargerID)).ToDictionary(p => p.Key, p => p.Value);
                    bool IsAllAssistVoltThan5 = bmsData.All(kvp=> kvp.Value.APSVoltage >= 5);


                    if (IsAllAssistVoltThan5)
                    {
                        ProcessDataResult(testWorkParam.lstIDs, "未响应", "K3K4响应时间(ms)", false);
                    }
                    else
                    {
                        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                        {
                            if (K3K4Times[testWorkParam.lstIDs[i]] != -999)
                            {
                                stmp = K3K4Times[testWorkParam.lstIDs[i]].ToString();
                                ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "-", "充电桩因故障，停止充电", true);
                                if (K3K4Times[testWorkParam.lstIDs[i]] <= 1000)
                                {
                                    ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, stmp, "K3K4响应时间(ms)", true);
                                }
                                else
                                {
                                    ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, stmp, "K3K4响应时间(ms)", false);
                                }
                            }
                            else
                            {
                                ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "未响应", "K3K4响应时间(ms)", false);
                            }
                        }

                    }
                }

                CountDownTimeInfo("请选择桩是否可以解锁\r\n注：勾选上则为解锁，不勾选为锁止", 999, 2);
                ProcessDataConnect();
                //CheckIsLock();
                for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                {
                    if (!DicManualVerifyResult[testWorkParam.lstIDs[i]])
                    {
                        testWorkParam.lstIDs.RemoveAt(i);
                        ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
                    }


                }
                // 如果双枪都为锁止状态则都不合格结束检测
                bool ALLLock = DicManualVerifyResult.All(c => c.Value == false);
                if (ALLLock || testWorkParam.lstIDs.Count == 0)
                {
                    return;
                }




                //先停止充电，恢复下位机状态
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1000);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1000);
                //启动充电
                //ChargingStart();
                RecoverProtocolConsist();
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                //自动启动桩体充电
                Charger_Start_DC();

                dts = DateTime.Now;
                while (true)
                {

                    for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                    {
                        if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
                        {
                            tmpRslts[testWorkParam.lstIDs[i]] = false;
                        }
                        else
                        {
                            tmpRslts[testWorkParam.lstIDs[i]] = true;
                        }
                    }
                    bool ALLIsRun = tmpRslts.All(c => c.Value == true);
                    if (ALLIsRun)
                    {
                        break;
                    }

                    //if (Prj.Prj.CanMsgController.IsCanStop)
                    //{
                    //    break ;
                    //}

                    if (dts.AddSeconds(60) < DateTime.Now)
                    {
                        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                        {
                            if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
                            {
                                tmpRslts[testWorkParam.lstIDs[i]] = false;
                            }
                            else
                            {
                                tmpRslts[testWorkParam.lstIDs[i]] = true;
                            }
                        }

                        //ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(15000);
                        break;
                    }

                }

                bool ALLRun = tmpRslts.All(c => c.Value == false);
                for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                {
                    if (tmpRslts[testWorkParam.lstIDs[i]])
                    {
                        ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "不允许充电", "未重新拔枪、插枪的状态下", true);
                    }
                    else
                    {
                        ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "允许充电", "未重新拔枪、插枪的状态下", false);
                        testWorkParam.lstIDs.RemoveAt(i);
                        ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
                    }
                }
                if (ALLRun)
                {
                    return;
                }



                sShowMsg = "请恢复正常状态，重新拔枪，插枪！！！操作完成后，请点击确定";
                Thread.Sleep(3000);
                MessgaeInfo(true, sShowMsg, true);


                //先停止充电，恢复下位机状态
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1000);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1000);
                //启动充电
                ChargingStart();

                //自动启动桩体充电
                Charger_Start_DC();

                dts = DateTime.Now;
                while (true)
                {

                    for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                    {
                        if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
                        {
                            tmpRslts[testWorkParam.lstIDs[i]] = true;
                        }
                        else
                        {
                            tmpRslts[testWorkParam.lstIDs[i]] = false;
                        }
                    }

                    bool ALLIsRun = tmpRslts.All(c => c.Value == true);
                    if (ALLIsRun)
                    {
                        break;
                    }
                    //if (Prj.Prj.CanMsgController.IsCanStop)
                    //{
                    //    return;
                    //}

                    if (dts.AddSeconds(60) < DateTime.Now)
                    {
                        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                        {
                            if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
                            {
                                tmpRslts[testWorkParam.lstIDs[i]] = true;
                            }
                            else
                            {
                                tmpRslts[testWorkParam.lstIDs[i]] = false;
                            }
                        }
                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(15000);
                        break;
                    }
                }
                for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                {
                    if (tmpRslts[testWorkParam.lstIDs[i]])
                    {
                        ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "允许充电", "重新拔枪、插枪的状态下", true);
                    }
                    else
                    {
                        ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "不允许充电", "重新拔枪、插枪的状态下", false);
                        ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
                        testWorkParam.lstIDs.RemoveAt(i);
                    }
                }

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                dts = DateTime.Now;
                while (true)
                {

                    // 判断是否停止充电，通过can通讯来判断
                    var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
                    bool isStop = true;
                    foreach (bool item in dic_isStop.Values)
                    {
                        if (!item) isStop = false; break;
                    }
                    if (isStop) break;

                    if (dts.AddSeconds(15) < DateTime.Now)
                    {
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                SendException(ex);
                Charger_Stop();
            }
            finally
            {
                SystemEvent.SetBMS(false);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            }
        }

        private void DP_3005_Test()
        {
            try
            {
                IsCard = false;
                //自动启动桩体充电
                Charger_Start_DC();

                DateTime dts = DateTime.Now;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                while (true)
                {
                    bool AllCharge = AllEquipStateData.DicBMS_DC_StateData.Where(kvp => testWorkParam.lstIDs.Contains(kvp.Key)).All(kvp => kvp.Value.ChargingState == "充电中");
                    if (AllCharge)
                    {
                        break;
                    }

                    //if (Prj.Prj.CanMsgController.IsCanStop)
                    //{
                    //    return;
                    //}

                    if (dts.AddSeconds(DCWaitTime) < DateTime.Now)
                    {
                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(15000);
                        return;
                    }


                }

                Thread.Sleep(3000);


                SystemEvent.SetDt(ProtocolNum);

                //延时20s
                //Thread.Sleep(20000);
                dts = DateTime.Now;
                while (true)
                {
                    if (dts.AddSeconds(20) < DateTime.Now)
                    {
                        break;
                    }



                    var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
                    bool isStop = true;
                    foreach (bool item in dic_isStop.Values)
                    {
                        if (!item) isStop = false; break;
                    }
                    if (isStop) break;
                    int Charge = AllEquipStateData.DicBMS_DC_StateData.Count(kvp => testWorkParam.lstIDs.Contains(kvp.Key) && kvp.Value.ChargingState != "充电中");

                    if (Charge > 0)
                    {
                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(10000);
                        return;
                    }
                }
                SystemEvent.SetBMS();

                //Thread.Sleep(30000);
                dts = DateTime.Now;
                while (true)
                {
                    if (dts.AddSeconds(30) < DateTime.Now)
                    {
                        break;
                    }


                }
                //设置禁止充电
                SystemEvent.SetDt(ProtocolNum);

                //延时12分钟
                dts = DateTime.Now;
                //Thread.Sleep(720000);
                while (true)
                {
                    if (dts.AddSeconds(660) < DateTime.Now)
                    {
                        break;
                    }


                    int Charge = AllEquipStateData.DicBMS_DC_StateData.Count(kvp => testWorkParam.lstIDs.Contains(kvp.Key) && kvp.Value.ChargingState != "充电中");

                    if (Charge > 0)
                    {
                        break;
                    }
                }

                dts = DateTime.Now;
                while (true)
                {
                    // 判断是否停止充电，通过can通讯来判断
                    var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
                    bool isStop = true;
                    foreach (bool item in dic_isStop.Values)
                    {
                        if (!item) isStop = false; break;
                    }
                    if (isStop) break;
                    if (dts.AddSeconds(10) < DateTime.Now)
                    {
                        //SetCanClose();
                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(15000);
                        //SetCanOpen();
                        break;
                    }


                }

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                dts = DateTime.Now;
                while (true)
                {

                    // 判断是否停止充电，通过can通讯来判断
                    var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
                    bool isStop = true;
                    foreach (bool item in dic_isStop.Values)
                    {
                        if (!item) isStop = false; break;
                    }
                    if (isStop) break;

                    if (dts.AddSeconds(15) < DateTime.Now)
                    {
                        break;
                    }


                }


                //设置允许充电,恢复正常状态
                SystemEvent.SetBMS();


                //判断提交结果

                CacleConsistRslt();

            }
            catch (Exception ex)
            {
                SendException(ex);
                Charger_Stop();
            }
            finally
            {
                SystemEvent.SetBMS(false);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            }
        }

        private void DP_3006_Test()
        {
            try
            {
                IsCard = false;
                //自动启动桩体充电
                Charger_Start_DC();

                DateTime dts = DateTime.Now;

                while (true)
                {
                    bool AllCharge = JudgeBMSChargerStatus("充电中");
                    if (AllCharge)
                    {
                        break;
                    }

                    //if (Prj.Prj.CanMsgController.IsCanStop)
                    //{
                    //    return;
                    //}

                    if (dts.AddSeconds(DCWaitTime) < DateTime.Now)
                    {
                        bool AllChargeNo = JudgeBMSChargerStatus("充电中");
                        // 超时时间内没能到达充电中的均视为异常
                        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                        {
                            string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState;
                            if (BMSInfo != "充电中")
                            {
                                ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "充电状态异常，未能启动充电", "", false);

                                ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
                                testWorkParam.lstIDs.RemoveAt(i);
                            }
                        }
                        if (AllChargeNo)
                        {
                            return;
                        }
                    }
                    Thread.Sleep(50);
                }

                SystemEvent.SetDt(ProtocolNum);

                dts = DateTime.Now;
                while (true)
                {
                    var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
                    bool isStop = true;
                    foreach (bool item in dic_isStop.Values)
                    {
                        if (!item) isStop = false; break;
                    }
                    if (isStop) break;

                    if (dts.AddSeconds(30) < DateTime.Now)
                    {
                        //SetCanClose();
                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(15000);
                        //SetCanOpen();
                        break;
                    }

                }

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                dts = DateTime.Now;
                while (true)
                {
                    var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
                    bool isStop = true;
                    foreach (bool item in dic_isStop.Values)
                    {
                        if (!item) isStop = false; break;
                    }
                    if (isStop) break;

                    if (dts.AddSeconds(15) < DateTime.Now)
                    {
                        break;
                    }
                }

                //判断提交结果
                CacleConsistRslt();
            }
            catch (Exception ex)
            {
                SendException(ex);
                Charger_Stop();
            }
            finally
            {
                SystemEvent.SetBMS(false);
            }
        }

        private void DP_3007_Test()
        {
            try
            {
                IsCard = false;
                //自动启动桩体充电
                Charger_Start_DC();

                DateTime dts = DateTime.Now;
                while (true)
                {
                    bool AllCharge = AllEquipStateData.DicBMS_DC_StateData.Where(kvp => testWorkParam.lstIDs.Contains(kvp.Key)).All(kvp=> kvp.Value.ChargingState == "充电中");
                    if (AllCharge)
                    {
                        break;
                    }


                    if (dts.AddSeconds(DCWaitTime) < DateTime.Now)
                    {
                        bool AllChargeNo = AllEquipStateData.DicBMS_DC_StateData.Where(kvp => testWorkParam.lstIDs.Contains(kvp.Key)).All(kvp=> kvp.Value.ChargingState != "充电中");

                        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                        {
                            string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState;
                            if (BMSInfo != "充电中")
                            {

                                ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "充电状态异常，未能启动充电", "", false);

                                ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
                                testWorkParam.lstIDs.RemoveAt(i);
                                Thread.Sleep(15000);

                            }
                        }
                        if (AllChargeNo)
                        {

                            return;
                        }
                    }
                }

                string sShowMsg = "";
                if (ProtocolNum == 38)
                {
                    sShowMsg = "请模拟充电桩故障！！！操作完成后，请点击确定";
                    MessgaeInfo(true, sShowMsg, true);
                }
                else
                {
                    //sShowMsg = "请操作充电桩主动中止充电！！！操作完成后，请点击确定";
                    Charger_Stop();
                }

                Thread.Sleep(5000);


                dts = DateTime.Now;
                while (true)
                {
                    //判断是否停止充电，通过can通讯来判断
                    var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
                    bool isStop = true;
                    foreach (bool item in dic_isStop.Values)
                    {
                        if (!item) isStop = false; break;
                    }
                    if (isStop) break;

                    if (dts.AddSeconds(60) < DateTime.Now)
                    {
                        //SetCanClose();
                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(15000);
                        //SetCanOpen();
                        break;
                    }

                }

                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                dts = DateTime.Now;
                while (true)
                {

                    var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
                    bool isStop = true;
                    foreach (bool item in dic_isStop.Values)
                    {
                        if (!item) isStop = false; break;
                    }
                    if (isStop) break;

                    if (dts.AddSeconds(15) < DateTime.Now)
                    {
                        break;
                    }


                }


                //判断提交结果
                CacleConsistRslt();

            }
            catch (Exception ex)
            {
                SendException(ex);
                Charger_Stop();
            }
            finally
            {
                SystemEvent.SetBMS(false);
            }
        }

        private void DP_4002_Test()
        {
            try
            {

                IsCard = false;
                Dictionary<int, bool> tmpRslts = new Dictionary<int, bool>();
                //自动启动桩体充电
                Charger_Start_DC();


                DateTime dts = DateTime.Now;
                while (true)
                {
                    bool AllCharge = AllEquipStateData.DicBMS_DC_StateData.Where(kvp => testWorkParam.lstIDs.Contains(kvp.Key)).All(kvp=> kvp.Value.ChargingState == "充电中");
                    if (AllCharge)
                    {
                        break;
                    }

                    var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
                    bool isStop = true;
                    foreach (bool item in dic_isStop.Values)
                    {
                        if (!item) isStop = false; break;
                    }
                    if (isStop) return;

                    if (dts.AddSeconds(DCWaitTime) < DateTime.Now)
                    {
                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(15000);
                        return;
                    }


                }

                for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                {
                    string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState;
                    if (BMSInfo != "充电中")
                    {

                        ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "充电状态异常，未能启动充电", "", false);

                        ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
                        testWorkParam.lstIDs.RemoveAt(i);
                        Thread.Sleep(15000);

                    }
                }
                bool AllChargeNo = AllEquipStateData.DicBMS_DC_StateData.Where(kvp => testWorkParam.lstIDs.Contains(kvp.Key)).All (kvp=> kvp.Value.ChargingState != "充电中");

                if (AllChargeNo)
                {
                    MessgaeInfo(true, "目前充电状态异常，请检查充电状态，即将退出该项目，开始下一个测试项目！！！", true);
                    return;
                }

                string sShowMsg = "";
                //sShowMsg = "请模拟充电桩故障！！！操作完成后，请点击确定";
                //MessageBox.Show(sShowMsg);
                SystemEvent.SetDt(ProtocolNum);
                Thread.Sleep(5000);


                dts = DateTime.Now;
                while (true)
                {
                    // 判断是否停止充电，通过can通讯来判断
                    Dictionary<int, bool> dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
                    bool isStop = true;
                    foreach (bool item in dic_isStop.Values)
                    {
                        if (!item) isStop = false; break;
                    }
                    foreach (var item in dic_isStop)
                    {
                        tmpRslts.Add(item.Key, item.Value);
                    }
                    if (isStop)
                    {
                        break;
                    }

                    if (dts.AddSeconds(60) < DateTime.Now)
                    {

                        //SetCanClose();
                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(15000);
                        //SetCanOpen();
                        break;
                    }

                }
                bool ALLRun = tmpRslts.All(c => c.Value == false);
                for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                {
                    if (tmpRslts[testWorkParam.lstIDs[i]])
                    {
                        ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "充电桩因故障，停止充电", "", true);
                    }
                    else
                    {
                        ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "充电桩因故障，未停止充电", "", false);
                        testWorkParam.lstIDs.RemoveAt(i);
                        ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
                    }
                }
                if (ALLRun)
                {
                    return;
                }

                //先停止充电，恢复下位机状态
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1000);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1000);
                //启动充电
                //ChargingStart();
                RecoverProtocolConsist();
                ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

                //自动启动桩体充电
                Charger_Start_DC();

                dts = DateTime.Now;
                while (true)
                {
                    for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                    {
                        if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
                        {
                            tmpRslts[testWorkParam.lstIDs[i]] = false;
                        }
                        else
                        {
                            tmpRslts[testWorkParam.lstIDs[i]] = true;
                        }
                    }
                    bool AllChargeStop = tmpRslts.All(c => c.Value == false);
                    if (AllChargeStop)
                    {
                        break;
                    }

                    if (IsCard)
                    {
                        break;
                    }
                    //if (Prj.Prj.CanMsgController.IsCanStop)
                    //{
                    //    break ;
                    //}

                    if (dts.AddSeconds(DCWaitTime) < DateTime.Now)
                    {
                        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                        {
                            if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
                            {
                                tmpRslts[testWorkParam.lstIDs[i]] = false;
                            }
                            else
                            {
                                tmpRslts[testWorkParam.lstIDs[i]] = true;
                            }
                        }

                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(15000);
                        break;
                    }

                }

                ALLRun = tmpRslts.All(c => c.Value == false);
                for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                {
                    if (tmpRslts[testWorkParam.lstIDs[i]])
                    {
                        ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "未重新拔枪、插枪的状态下，不允许充电", "", true);
                    }
                    else
                    {
                        ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "未重新拔枪、插枪的状态下，允许充电", "", false);
                        testWorkParam.lstIDs.RemoveAt(i);
                        ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
                    }
                }
                if (ALLRun)
                {
                    return;
                }

                sShowMsg = "请恢复正常状态，重新拔枪，插枪！！！操作完成后，请点击确定";
                Thread.Sleep(3000);
                MessgaeInfo(true, sShowMsg, true);


                //先停止充电，恢复下位机状态
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1000);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                Thread.Sleep(1000);
                //启动充电
                ChargingStart();

                //自动启动桩体充电
                Charger_Start_DC();

                dts = DateTime.Now;
                while (true)
                {



                    for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                    {
                        if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
                        {
                            tmpRslts[testWorkParam.lstIDs[i]] = true;
                        }
                        else
                        {
                            tmpRslts[testWorkParam.lstIDs[i]] = false;
                        }
                    }

                    bool AllCharge = AllEquipStateData.DicBMS_DC_StateData.Where(kvp => testWorkParam.lstIDs.Contains(kvp.Key)).All(kvp=> kvp.Value.ChargingState == "充电中");
                    if (AllCharge)
                    {
                        Thread.Sleep(5000);
                        break;
                    }
                    if (IsCard)
                    {
                        break;
                    }


                    if (dts.AddSeconds(DCWaitTime) < DateTime.Now)
                    {
                        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                        {
                            if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
                            {
                                tmpRslts[testWorkParam.lstIDs[i]] = true;
                            }
                            else
                            {
                                tmpRslts[testWorkParam.lstIDs[i]] = false;
                            }
                        }
                        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                        Thread.Sleep(15000);
                        break;
                    }


                }
                for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
                {
                    if (tmpRslts[testWorkParam.lstIDs[i]])
                    {
                        ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "重新拔枪、插枪的状态下，允许充电", "", true);
                    }
                    else
                    {
                        ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "重新拔枪、插枪的状态下，不允许充电", "", false);
                        testWorkParam.lstIDs.RemoveAt(i);
                        ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
                    }
                }






                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
                dts = DateTime.Now;
                while (true)
                {

                    // 判断是否停止充电，通过can通讯来判断
                    var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
                    bool isStop = true;
                    foreach (bool item in dic_isStop.Values)
                    {
                        if (!item) isStop = false; break;
                    }
                    if (isStop) break;

                    if (dts.AddSeconds(15) < DateTime.Now)
                    {
                        break;
                    }


                }




                //////判断提交结果
                //if (!Prj.Prj.ConsistController.IsStop)
                //{
                CacleConsistRslt();
                //}
            }
            catch (Exception ex)
            {
                SendException(ex);
                Charger_Stop();
            }
            finally
            {
                SystemEvent.SetBMS(false);
                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
            }
        }

        /// <summary>
        /// 判断是否锁止协议一致性
        /// </summary>
        public void CheckIsLock()
        {
            foreach (var item in DicManualVerifyResult)
            {
           
                int k = LstTrialData.FindIndex(s => s.ChargerId == item.Key);
                int i = LstChargerInfo.FindIndex(s => s.ChargerId == item.Key);
                if (k < 0)
                    return;
                LstTrialData[k].ItemName = iIndex.ToString();
                LstTrialData[k].BarCode = LstChargerInfo[i].BarCode;
                LstTrialData[k].TrialName = TrialItem.ItemName;
                LstTrialData[k].SchemeName = TrialItem.SchemeName;
                LstTrialData[k].SchemeID = TrialItem.SchemeID;
                LstTrialData[k].SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                double volate = AllEquipStateData.DicBMS_AC_StateData[LstTrialData[k].ChargerId].PhaseA_Voltage;
                if (item.Value)
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Pass;
                    LstTrialData[k].ExtentData = TrialItem.ItemName + "|是否有机械锁|-|-|有";
                }
                else
                {
                    LstTrialData[k].TrialResult = EmTrialResult.Fail;
                    LstTrialData[k].ExtentData = TrialItem.ItemName + "|是否有机械锁|-|-|无";
                    testWorkParam.lstIDs.Remove(item.Key);
                }
                LstTrialData[k].PKID = LstChargerInfo[i].PKID;
                //界面展示的数据项格式
                //状态|测试结果     

                LstTrialData[k].Data2 = LstTrialData[k].ExtentData;
                SendTrialDataToUI(LstTrialData[k]);
                //ChargerID,BarCode,TrialType,TrialName,ItemName,SchemeID,SchemeItemName,Data1,Data2,Data3,TrialResult,SaveTime
                SaveTrialData(LstTrialData[k]);

                iIndex++;
            }
        }
        public void SetResult(string TestText1, string BarCode, long PKID)
        {
            List<TrialDataModel> LstItemData = new List<TrialDataModel>();
            string[] testStrs = TestText1.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            int index = 1;
            foreach (string testStr in testStrs)
            {
                if (string.IsNullOrWhiteSpace(testStr))
                    continue;
                TrialDataModel item = new TrialDataModel
                {
                    SchemeID = TrialItem.SchemeID,
                    BarCode = BarCode,
                    PKID = PKID,
                    TrialName = TrialItem.ItemName,
                    TrialType = TrialItem.TrialType,
                    SchemeName = TrialItem.SchemeName,
                    SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    //ItemName = SQLstr[1].ToString(),
                    ItemName = (index++).ToString(),
                    TrialResult = testStr.Contains("不合格") ? EmTrialResult.Fail : EmTrialResult.Pass//测试结果
                };
                string testText = testStr.Replace("不", "").Replace("合格", " ");
                if (testText.Trim().Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Length > 1)
                    item.ExtentData = $"{TrialItem.ItemName}|{testText.Split(':')[0].Trim()}|-|-|{testText.Split(':')[1].Trim()}";
                else
                    item.ExtentData = $"{TrialItem.ItemName}|{testText.Split(':')[0].Trim()}|-|-|-";
                item.Data2 = item.ExtentData;
                LstItemData.Add(item);
            }
            foreach (var item in LstItemData)
            {
                //if(item.TrialResult == EmTrialResult.Fail)
                //    LstTrialData[0].TrialResult = EmTrialResult.Fail;
                item.IsCheck = LstTrialData[0].IsCheck;
                item.ChargerId = LstTrialData[0].ChargerId;
                item.TrialCondition = LstTrialData[0].TrialCondition;
                SendTrialDataToUI(item);
                SaveTrialData(item);
            } 
            LstTrialData[0] = LstItemData[0];
        }

        #endregion

    }
}
