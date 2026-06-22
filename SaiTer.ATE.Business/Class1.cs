using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.InterFace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Business
{
    public partial class BusinessBase
    {
        //private void DN_2009_Test1()
        //{
        //    try
        //    {



        //        Dictionary<int, bool> tmpRslts = new Dictionary<int, bool>();
        //        bool isK3K4 = true;//是否判断K3K4时间（目前只有公牛下位机有兼容）





        //        //自动启动桩体充电
        //        Charger_Start_DC();



        //        DateTime dts = DateTime.Now;

        //        string sShowMsg = "";


        //        dts = DateTime.Now;
        //        while (true)
        //        {
        //            //判断是否停止充电，通过can通讯来判断
        //            //Prj.Prj.CanMsgController.IsStopCharge = Prj.Prj.CanMsgController.IsCanStop;
        //            Dictionary<int, bool> dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
        //            bool isStop = true;
        //            foreach (bool item in dic_isStop.Values)
        //            {
        //                if (!item) isStop = false; break;
        //            }
        //            foreach (var item in dic_isStop)
        //            {
        //                tmpRslts.Add(item.Key, item.Value);
        //            }
        //            if (isStop)
        //            {
        //                break;
        //            }

        //            if (dts.AddSeconds(90) < DateTime.Now)//90
        //            {
        //                //tmprslt = false;
        //                //bRslt &= false; 
        //                for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //                {
        //                    if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
        //                    {
        //                        tmpRslts[testWorkParam.lstIDs[i]] = false;
        //                    }
        //                    else
        //                    {
        //                        tmpRslts[testWorkParam.lstIDs[i]] = true;
        //                    }
        //                }
        //                //SetCanClose();
        //                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //                Thread.Sleep(15000);
        //                //SetCanOpen();
        //                break;
        //            }
        //        }
        //        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //        {
        //            if (tmpRslts[testWorkParam.lstIDs[i]])
        //            {
        //                //tir = CacleCanMsgRslt();
        //            }
        //        }

        //        string stmp = "";

        //        ////K3K4断开时间
        //        if (isK3K4)//需要判断才判断
        //        {
           

        //            Dictionary<int, int> K3K4Times = ControlEquipMent.BMS.GetK3K4StopTime(testWorkParam.lstIDs);

        //            dts = DateTime.Now;
        //            while (true)
        //            {

        //                bool IsAllCorrectValue = K3K4Times.All(kvp => testWorkParam.lstIDs.Contains(kvp.Key) && kvp.Value != -999);
        //                if (IsAllCorrectValue
        //                    || dts.AddSeconds(5) < DateTime.Now)
        //                {
        //                    break;
        //                }
        //            }
        //            bool IsAllAssistVoltThan5 = AllEquipStateData.DicBMS_DC_StateData.All(kvp => testWorkParam.lstIDs.Contains(kvp.Key) && kvp.Value.APSVoltage >= 5);


        //            if (IsAllAssistVoltThan5)
        //            {

        //                ProcessDataResult(testWorkParam.lstIDs, "K3K4响应时间：未响应", "", false);

        //            }
        //            else
        //            {
        //                for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //                {
        //                    if (K3K4Times[testWorkParam.lstIDs[i]]!=999)
        //                    {
        //                        stmp = K3K4Times[testWorkParam.lstIDs[i]].ToString() + "ms";
        //                        ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "充电桩因故障，停止充电", "", true);
        //                        if (K3K4Times[testWorkParam.lstIDs[i]] <=1000)
        //                        {
        //                            ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "K3K4响应时间："+ stmp, "", true);
        //                        }
        //                        else
        //                        {
        //                            ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "K3K4响应时间：" + stmp, "", false);
        //                        }
        //                        }
        //                    else
        //                    {


        //                        ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "K3K4响应时间：未响应", "", false);
        //                        testWorkParam.lstIDs.RemoveAt(i);
        //                        ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
        //                    }
        //                }


        //            }

        //        }



        //        CountDownTimeInfo("请选择桩是否可以解锁（勾选上则为解锁，不勾选为锁止）",999, 2);
        //        CheckIsLock();
        //        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //        {
        //            if(!DicManualVerifyResult[testWorkParam.lstIDs[i]])
        //            {
        //                testWorkParam.lstIDs.RemoveAt(i);
        //                ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
        //            }


        //        }

        //        bool ALLLock = DicManualVerifyResult.All(c => c.Value == false);
        //        if(ALLLock)
        //        {
        //            return;
        //        }




        //        //先停止充电，恢复下位机状态
        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //        Thread.Sleep(1000);
        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //        Thread.Sleep(1000);
        //        //启动充电
        //        //ChargingStart();
        //        RecoverProtocolConsist();
        //        ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

        //        //自动启动桩体充电
        //        Charger_Start_DC();

        //        dts = DateTime.Now;
        //        while (true)
        //        {
        //            for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //            {
        //                if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
        //                {
        //                    tmpRslts[testWorkParam.lstIDs[i]] = false;
        //                }
        //                else
        //                {
        //                    tmpRslts[testWorkParam.lstIDs[i]] = true;
        //                }
        //            }


        //            //if (Prj.Prj.MainController.IsCard)
        //            //{
        //            //    if (Prj.Prj._baseInfo.ChargeState == "充电中")
        //            //    {
        //            //        tmprslt = false;
        //            //        break;
        //            //    }
        //            //    else
        //            //    {
        //            //        tmprslt = true;
        //            //        break;
        //            //    }
        //            //}
        //            //if (Prj.Prj.CanMsgController.IsCanStop)
        //            //{
        //            //    break ;
        //            //}

        //            if (dts.AddSeconds(60) < DateTime.Now)
        //            {
        //                for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //                {
        //                    if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
        //                    {
        //                        tmpRslts[testWorkParam.lstIDs[i]] = false;
        //                    }
        //                    else
        //                    {
        //                        tmpRslts[testWorkParam.lstIDs[i]] = true;
        //                    }
        //                }

        //                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //                Thread.Sleep(15000);
        //                break;
        //            }

        //        }

        //        bool ALLRun = tmpRslts.All(c => c.Value == false);
        //        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //        {
        //            if (tmpRslts[testWorkParam.lstIDs[i]])
        //            {
        //                ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "未重新拔枪、插枪的状态下，不允许充电", "", true);
        //            }
        //            else
        //            {
        //                ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "未重新拔枪、插枪的状态下，允许充电", "", false);
        //                testWorkParam.lstIDs.RemoveAt(i);
        //                ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
        //            }
        //        }
        //        if (ALLRun)
        //        {
        //            return;
        //        }



        //        sShowMsg = "请恢复正常状态，重新拔枪，插枪！！！操作完成后，请点击确定";
        //        Thread.Sleep(3000);
        //        MessgaeInfo(true, sShowMsg, true);


        //        //先停止充电，恢复下位机状态
        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //        Thread.Sleep(1000);
        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //        Thread.Sleep(1000);
        //        //启动充电
        //        ChargingStart();

        //        //自动启动桩体充电
        //        Charger_Start_DC();

        //        dts = DateTime.Now;
        //        while (true)
        //        {
   
        //            for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //            {
        //                if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
        //                {
        //                    tmpRslts[testWorkParam.lstIDs[i]] = true;
        //                }
        //                else
        //                {
        //                    tmpRslts[testWorkParam.lstIDs[i]] = false;
        //                }
        //            }

        //            bool ALLIsRun = tmpRslts.All(c => c.Value == true);
        //            if(ALLIsRun)
        //            {
        //                break;
        //            }

        //            //if (Prj.Prj.CanMsgController.IsCanStop)
        //            //{
        //            //    return;
        //            //}

        //            if (dts.AddSeconds(60) < DateTime.Now)
        //            {
        //                for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //                {
        //                    if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
        //                    {
        //                        tmpRslts[testWorkParam.lstIDs[i]] = true;
        //                    }
        //                    else
        //                    {
        //                        tmpRslts[testWorkParam.lstIDs[i]] = false;
        //                    }
        //                }
        //                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //                Thread.Sleep(15000);
        //                break;
        //            }


        //        }
        //        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //        {
        //            if (tmpRslts[testWorkParam.lstIDs[i]])
        //            {
        //                ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "重新拔枪、插枪的状态下，允许充电", "", true);
        //            }
        //            else
        //            {
        //                ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "重新拔枪、插枪的状态下，不允许充电", "", false);
        //                testWorkParam.lstIDs.RemoveAt(i);
        //                ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
        //            }
        //        }





        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //        dts = DateTime.Now;
        //        while (true)
        //        {

        //            // 判断是否停止充电，通过can通讯来判断
        //            var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
        //            bool isStop = true;
        //            foreach (bool item in dic_isStop.Values)
        //            {
        //                if (!item) isStop = false; break;
        //            }
        //            if (isStop) break;

        //            if (dts.AddSeconds(15) < DateTime.Now)
        //            {
        //                break;
        //            }


        //        }




        //    }
        //    catch (Exception ex)
        //    {
        //        Charger_Stop();
        //    }
        //    finally
        //    {
        //        SystemEvent.SetBMS();
        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //    }
        //}

        //private void DP_3005_Test1()
        //{
        //    try
        //    {
        //        //自动启动桩体充电
        //        Charger_Start_DC();

        //        DateTime dts = DateTime.Now;
        //        Stopwatch stopwatch = new Stopwatch();
        //        stopwatch.Start();
        //        while (true)
        //        {
        //            bool AllCharge = AllEquipStateData.DicBMS_DC_StateData.All(kvp => testWorkParam.lstIDs.Contains(kvp.Key) && kvp.Value.ChargingState == "充电中");
        //            if (AllCharge)
        //            {
        //                break;
        //            }

        //            //if (Prj.Prj.CanMsgController.IsCanStop)
        //            //{
        //            //    return;
        //            //}

        //            if (dts.AddSeconds(DCWaitTime) < DateTime.Now)
        //            {
        //                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //                Thread.Sleep(15000);
        //                return;
        //            }


        //        }

        //        Thread.Sleep(3000);


        //        SystemEvent.SetDt(ProtocolNum);

        //        //延时20s
        //        //Thread.Sleep(20000);
        //        dts = DateTime.Now;
        //        while (true)
        //        {
        //            if (dts.AddSeconds(20) < DateTime.Now)
        //            {
        //                break;
        //            }



        //            var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
        //            bool isStop = true;
        //            foreach (bool item in dic_isStop.Values)
        //            {
        //                if (!item) isStop = false; break;
        //            }
        //            if (isStop) break;
        //            int Charge = AllEquipStateData.DicBMS_DC_StateData.Count(kvp => testWorkParam.lstIDs.Contains(kvp.Key) && kvp.Value.ChargingState != "充电中");

        //            if (Charge > 0)
        //            {
        //                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //                Thread.Sleep(10000);
        //                return;
        //            }
        //        }
        //        SystemEvent.SetBMS();

        //        //Thread.Sleep(30000);
        //        dts = DateTime.Now;
        //        while (true)
        //        {
        //            if (dts.AddSeconds(30) < DateTime.Now)
        //            {
        //                break;
        //            }


        //        }
        //        //设置禁止充电
        //        SystemEvent.SetDt(ProtocolNum);

        //        //延时12分钟
        //        dts = DateTime.Now;
        //        //Thread.Sleep(720000);
        //        while (true)
        //        {
        //            if (dts.AddSeconds(660) < DateTime.Now)
        //            {
        //                break;
        //            }


        //            int Charge = AllEquipStateData.DicBMS_DC_StateData.Count(kvp => testWorkParam.lstIDs.Contains(kvp.Key) && kvp.Value.ChargingState != "充电中");

        //            if (Charge > 0)
        //            {
        //                break;
        //            }
        //        }

        //        dts = DateTime.Now;
        //        while (true)
        //        {
        //            // 判断是否停止充电，通过can通讯来判断
        //            var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
        //            bool isStop = true;
        //            foreach (bool item in dic_isStop.Values)
        //            {
        //                if (!item) isStop = false; break;
        //            }
        //            if (isStop) break;
        //            if (dts.AddSeconds(10) < DateTime.Now)
        //            {
        //                //SetCanClose();
        //                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //                Thread.Sleep(15000);
        //                //SetCanOpen();
        //                break;
        //            }


        //        }

        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //        dts = DateTime.Now;
        //        while (true)
        //        {

        //            // 判断是否停止充电，通过can通讯来判断
        //            var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
        //            bool isStop = true;
        //            foreach (bool item in dic_isStop.Values)
        //            {
        //                if (!item) isStop = false; break;
        //            }
        //            if (isStop) break;

        //            if (dts.AddSeconds(15) < DateTime.Now)
        //            {
        //                break;
        //            }


        //        }


        //        //设置允许充电,恢复正常状态
        //        SystemEvent.SetBMS();


        //        //判断提交结果

        //        //CacleConsistRslt();

        //    }
        //    catch (Exception ex)
        //    {
        //        Charger_Stop();
        //    }
        //    finally
        //    {
        //        SystemEvent.SetBMS();
        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //    }


        //}

        //private void DP_3006_Test1()
        //{
        //    try
        //    {



        //        //自动启动桩体充电
        //        Charger_Start_DC();

        //        DateTime dts = DateTime.Now;

        //        while (true)
        //        {
        //            bool AllCharge = AllEquipStateData.DicBMS_DC_StateData.All(kvp => testWorkParam.lstIDs.Contains(kvp.Key) && kvp.Value.ChargingState == "充电中");
        //            if (AllCharge)
        //            {
        //                break;
        //            }


        //            //if (Prj.Prj.CanMsgController.IsCanStop)
        //            //{
        //            //    return;
        //            //}

        //            if (dts.AddSeconds(DCWaitTime) < DateTime.Now)
        //            {

        //                bool AllChargeNo = AllEquipStateData.DicBMS_DC_StateData.All(kvp => testWorkParam.lstIDs.Contains(kvp.Key) && kvp.Value.ChargingState != "充电中");

        //                for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //                {
        //                    string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState;
        //                    if (BMSInfo != "充电中")
        //                    {

        //                        ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "充电状态异常，未能启动充电", "", false);

        //                        ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
        //                        testWorkParam.lstIDs.RemoveAt(i);
        //                        Thread.Sleep(15000);

        //                    }
        //                }
        //                if (AllChargeNo)
        //                {

        //                    return;
        //                }




        //            }

        //        }




        //        SystemEvent.SetDt(ProtocolNum);

        //        dts = DateTime.Now;
        //        while (true)
        //        {
        //            var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
        //            bool isStop = true;
        //            foreach (bool item in dic_isStop.Values)
        //            {
        //                if (!item) isStop = false; break;
        //            }
        //            if (isStop) break;

        //            if (dts.AddSeconds(30) < DateTime.Now)
        //            {
        //                //SetCanClose();
        //                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //                Thread.Sleep(15000);
        //                //SetCanOpen();
        //                break;
        //            }

        //        }

        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //        dts = DateTime.Now;
        //        while (true)
        //        {
        //            var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
        //            bool isStop = true;
        //            foreach (bool item in dic_isStop.Values)
        //            {
        //                if (!item) isStop = false; break;
        //            }
        //            if (isStop) break;

        //            if (dts.AddSeconds(15) < DateTime.Now)
        //            {
        //                break;
        //            }


        //        }

        //        //判断提交结果

        //        //CacleConsistRslt();

        //    }
        //    catch (Exception ex)
        //    {
        //        Charger_Stop();
        //    }
        //    finally
        //    {

        //        SystemEvent.SetBMS();

        //    }
        //}

        //private void DP_3007_Test1()
        //{
        //    try
        //    {
        //        //自动启动桩体充电
        //        Charger_Start_DC();

        //        DateTime dts = DateTime.Now;
        //        while (true)
        //        {
        //            bool AllCharge = AllEquipStateData.DicBMS_DC_StateData.All(kvp => testWorkParam.lstIDs.Contains(kvp.Key) && kvp.Value.ChargingState == "充电中");
        //            if (AllCharge)
        //            {
        //                break;
        //            }


        //            if (dts.AddSeconds(DCWaitTime) < DateTime.Now)
        //            {
        //                bool AllChargeNo = AllEquipStateData.DicBMS_DC_StateData.All(kvp => testWorkParam.lstIDs.Contains(kvp.Key) && kvp.Value.ChargingState != "充电中");

        //                for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //                {
        //                    string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState;
        //                    if (BMSInfo != "充电中")
        //                    {

        //                        ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "充电状态异常，未能启动充电", "", false);

        //                        ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
        //                        testWorkParam.lstIDs.RemoveAt(i);
        //                        Thread.Sleep(15000);

        //                    }
        //                }
        //                if (AllChargeNo)
        //                {

        //                    return;
        //                }
        //            }


        //        }

        //        string sShowMsg = "";
        //        if (ProtocolNum == 38)
        //        {
        //            sShowMsg = "请模拟充电桩故障！！！操作完成后，请点击确定";
        //            MessgaeInfo(true, sShowMsg, true);
        //        }
        //        else
        //        {
        //            //sShowMsg = "请操作充电桩主动中止充电！！！操作完成后，请点击确定";
        //            Charger_Stop();
        //        }

        //        Thread.Sleep(5000);


        //        dts = DateTime.Now;
        //        while (true)
        //        {
        //            //判断是否停止充电，通过can通讯来判断
        //            var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
        //            bool isStop = true;
        //            foreach (bool item in dic_isStop.Values)
        //            {
        //                if (!item) isStop = false; break;
        //            }
        //            if (isStop) break;

        //            if (dts.AddSeconds(60) < DateTime.Now)
        //            {
        //                //SetCanClose();
        //                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //                Thread.Sleep(15000);
        //                //SetCanOpen();
        //                break;
        //            }

        //        }

        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //        dts = DateTime.Now;
        //        while (true)
        //        {

        //            var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
        //            bool isStop = true;
        //            foreach (bool item in dic_isStop.Values)
        //            {
        //                if (!item) isStop = false; break;
        //            }
        //            if (isStop) break;

        //            if (dts.AddSeconds(15) < DateTime.Now)
        //            {
        //                break;
        //            }


        //        }


        //        //判断提交结果

        //        //CacleConsistRslt();

        //    }
        //    catch (Exception ex)
        //    {
        //        Charger_Stop();
        //    }
        //    finally
        //    {

        //        SystemEvent.SetBMS();
        //    }
        //}

        //private void DP_4002_Test1()
        //{
        //    try
        //    {


        //        Dictionary<int, bool> tmpRslts = new Dictionary<int, bool>();
        //        //自动启动桩体充电
        //        Charger_Start_DC();


        //        DateTime dts = DateTime.Now;
        //        while (true)
        //        {
        //            bool AllCharge = AllEquipStateData.DicBMS_DC_StateData.All(kvp => testWorkParam.lstIDs.Contains(kvp.Key) && kvp.Value.ChargingState == "充电中");
        //            if (AllCharge)
        //            {
        //                break;
        //            }

        //            var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
        //            bool isStop = true;







        //            foreach (bool item in dic_isStop.Values)
        //            {
        //                if (!item) isStop = false; break;
        //            }
        //            if (isStop) return;

        //            if (dts.AddSeconds(DCWaitTime) < DateTime.Now)
        //            {
        //                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //                Thread.Sleep(15000);
        //                return;
        //            }


        //        }

        //        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //        {
        //            string BMSInfo = AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState;
        //            if (BMSInfo != "充电中")
        //            {

        //                ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "充电状态异常，未能启动充电", "", false);

        //                ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
        //                testWorkParam.lstIDs.RemoveAt(i);
        //                Thread.Sleep(15000);

        //            }
        //        }
        //        bool AllChargeNo = AllEquipStateData.DicBMS_DC_StateData.All(kvp => testWorkParam.lstIDs.Contains(kvp.Key) && kvp.Value.ChargingState != "充电中");

        //        if (AllChargeNo)
        //        {
        //            MessgaeInfo(true, "目前充电状态异常，请检查充电状态，即将退出该项目，开始下一个测试项目！！！", true);
        //            return;
        //        }





        //        string sShowMsg = "";
        //        //sShowMsg = "请模拟充电桩故障！！！操作完成后，请点击确定";
        //        //MessageBox.Show(sShowMsg);
        //        SystemEvent.SetDt(ProtocolNum);
        //        Thread.Sleep(5000);


        //        dts = DateTime.Now;
        //        while (true)
        //        {
        //            // 判断是否停止充电，通过can通讯来判断
        //            Dictionary<int, bool> dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
        //            bool isStop = true;
        //            foreach (bool item in dic_isStop.Values)
        //            {
        //                if (!item) isStop = false; break;
        //            }
        //            foreach (var item in dic_isStop)
        //            {
        //                tmpRslts.Add(item.Key, item.Value);
        //            }
        //            if (isStop)
        //            {
        //                break;
        //            }

        //            if (dts.AddSeconds(60) < DateTime.Now)
        //            {

        //                //SetCanClose();
        //                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //                Thread.Sleep(15000);
        //                //SetCanOpen();
        //                break;
        //            }

        //        }
        //        bool ALLRun = tmpRslts.All(c => c.Value == false);
        //        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //        {
        //            if (tmpRslts[testWorkParam.lstIDs[i]])
        //            {
        //                ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "充电桩因故障，停止充电", "", true);
        //            }
        //            else
        //            {
        //                ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "充电桩因故障，未停止充电", "", false);
        //                testWorkParam.lstIDs.RemoveAt(i);
        //                ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
        //            }
        //        }
        //        if (ALLRun)
        //        {
        //            return;
        //        }



        //        //先停止充电，恢复下位机状态
        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //        Thread.Sleep(1000);
        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //        Thread.Sleep(1000);
        //        //启动充电
        //        //ChargingStart();
        //        RecoverProtocolConsist();
        //        ControlEquipMent.BMS.BMS_ON(testWorkParam.lstIDs);

        //        //自动启动桩体充电
        //        Charger_Start_DC();

        //        dts = DateTime.Now;
        //        while (true)
        //        {
        //            for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //            {
        //                if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
        //                {
        //                    tmpRslts[testWorkParam.lstIDs[i]] = false;
        //                }
        //                else
        //                {
        //                    tmpRslts[testWorkParam.lstIDs[i]] = true;
        //                }
        //            }

        //            bool AllChargeStop = tmpRslts.All(c => c.Value == false);
        //            if (AllChargeStop)
        //            {
        //                break;
        //            }


        //            //if (Prj.Prj.CanMsgController.IsCanStop)
        //            //{
        //            //    break ;
        //            //}

        //            if (dts.AddSeconds(DCWaitTime) < DateTime.Now)
        //            {
        //                for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //                {
        //                    if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
        //                    {
        //                        tmpRslts[testWorkParam.lstIDs[i]] = false;
        //                    }
        //                    else
        //                    {
        //                        tmpRslts[testWorkParam.lstIDs[i]] = true;
        //                    }
        //                }

        //                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //                Thread.Sleep(15000);
        //                break;
        //            }

        //        }


        //        ALLRun = tmpRslts.All(c => c.Value == false);
        //        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //        {
        //            if (tmpRslts[testWorkParam.lstIDs[i]])
        //            {
        //                ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "未重新拔枪、插枪的状态下，不允许充电", "", true);
        //            }
        //            else
        //            {
        //                ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "未重新拔枪、插枪的状态下，允许充电", "", false);
        //                testWorkParam.lstIDs.RemoveAt(i);
        //                ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
        //            }
        //        }
        //        if (ALLRun)
        //        {
        //            return;
        //        }








        //        sShowMsg = "请恢复正常状态，重新拔枪，插枪！！！操作完成后，请点击确定";
        //        Thread.Sleep(3000);
        //        MessgaeInfo(true, sShowMsg, true);


        //        //先停止充电，恢复下位机状态
        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //        Thread.Sleep(1000);
        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //        Thread.Sleep(1000);
        //        //启动充电
        //        ChargingStart();

        //        //自动启动桩体充电
        //        Charger_Start_DC();

        //        dts = DateTime.Now;
        //        while (true)
        //        {



        //            for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //            {
        //                if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
        //                {
        //                    tmpRslts[testWorkParam.lstIDs[i]] = true;
        //                }
        //                else
        //                {
        //                    tmpRslts[testWorkParam.lstIDs[i]] = false;
        //                }
        //            }

        //            bool AllCharge = AllEquipStateData.DicBMS_DC_StateData.All(kvp => testWorkParam.lstIDs.Contains(kvp.Key) && kvp.Value.ChargingState == "充电中");
        //            if (AllCharge)
        //            {
        //                Thread.Sleep(5000);
        //                break;
        //            }



        //            if (dts.AddSeconds(DCWaitTime) < DateTime.Now)
        //            {
        //                for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //                {
        //                    if (AllEquipStateData.DicBMS_DC_StateData[testWorkParam.lstIDs[i]].ChargingState == "充电中")
        //                    {
        //                        tmpRslts[testWorkParam.lstIDs[i]] = true;
        //                    }
        //                    else
        //                    {
        //                        tmpRslts[testWorkParam.lstIDs[i]] = false;
        //                    }
        //                }
        //                ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //                Thread.Sleep(15000);
        //                break;
        //            }


        //        }
        //        for (int i = testWorkParam.lstIDs.Count - 1; i >= 0; i--)
        //        {
        //            if (tmpRslts[testWorkParam.lstIDs[i]])
        //            {
        //                ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "重新拔枪、插枪的状态下，允许充电", "", true);
        //            }
        //            else
        //            {
        //                ProcessDataResult(new List<int>() { testWorkParam.lstIDs[i] }, "重新拔枪、插枪的状态下，不允许充电", "", false);
        //                testWorkParam.lstIDs.RemoveAt(i);
        //                ControlEquipMent.BMS.BMS_OFF(new List<int>() { testWorkParam.lstIDs[i] });
        //            }
        //        }






        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //        dts = DateTime.Now;
        //        while (true)
        //        {

        //            // 判断是否停止充电，通过can通讯来判断
        //            var dic_isStop = ControlEquipMent.BMS.GetIsCANStop(testWorkParam.lstIDs);
        //            bool isStop = true;
        //            foreach (bool item in dic_isStop.Values)
        //            {
        //                if (!item) isStop = false; break;
        //            }
        //            if (isStop) break;

        //            if (dts.AddSeconds(15) < DateTime.Now)
        //            {
        //                break;
        //            }


        //        }




        //        ////判断提交结果
        //        //if (!Prj.Prj.ConsistController.IsStop)
        //        //{
        //        //    CacleConsistRslt();
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        Charger_Stop();
        //    }
        //    finally
        //    {
        //        SystemEvent.SetBMS();
        //        ControlEquipMent.BMS.BMS_OFF(testWorkParam.lstIDs);
        //    }
        //}
    }
}
