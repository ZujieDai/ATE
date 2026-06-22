using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    public class MsgManager
    {
        public MsgManager()
        {
        }
        public CanMsgRich DecodeMsgData(string flowId, List<byte> content)
        {
            string symbol = string.Empty;
            CanMsgRich model = new CanMsgRich();
            try
            {
                MsgCommon machine = CreateDecodeMsgMachine(flowId);
                symbol = FindMsgSymbol(flowId, content);

                if (flowId != CanMsgId.MUTI_PACKAGE_TEXT)
                {
                    model = machine.DecodeMsgData(symbol, content);
                    if (flowId == CanMsgId.MUTI_PACKAGE_HEAD)  //是否多包的首包
                    {
                        model.ConsistMsg.IsFirstPackage = 1;
                    }
                }
                else //多包 正文
                {
                    int index = GetMuitPckgIndex(content);

                    if (index == Prj.Prj.MutiPackage.GetCountPlan())    //最后一包
                    {
                        Prj.Prj.MutiPackage.AppendContentPackage(content);  //先附加包，再解析
                        machine = CreateDecodeMsgMachine(symbol);
                        model = machine.DecodeMsgData(symbol, Prj.Prj.MutiPackage.GetMutiContent());    //解析所有包
                        model.ConsistMsg.MutiLength = Prj.Prj.MutiPackage.GetAppendCnt();               //获取实际包数 for Consist Test
                    }
                    else
                    {
                        model = machine.DecodeMsgData(symbol, content);     //先解析当前包：for解析出第几包
                        Prj.Prj.MutiPackage.AppendContentPackage(content);  //再附加包
                        //Prj.Prj.MutiPackage.Finish = false;                 //标记未完成解包
                    }
                }
                model.ConsistMsg.PackageId = SetPackageId(flowId);
                model.ConsistMsg.TextId = SetPackageTextId(flowId);
                model.Symbol = symbol;
                model.ConsistMsg.MsgName = symbol;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return model;
        }


        private MsgCommon CreateDecodeMsgMachine(string idFlow)
        {
            switch (idFlow)
            {
                case CanMsgId.BHM:
                    return new Msg_BHM();
                case CanMsgId.CHM:
                    return new Msg_CHM();
                case CanMsgId.CRM:
                    return new Msg_CRM();
                case CanMsgId.MUTI_PACKAGE_HEAD:
                    return new Msg_MutiPackageHead();
                case CanMsgId.MUTI_PACKAGE_READY:
                    return new Msg_MutiPackageReady();
                case CanMsgId.MUTI_PACKAGE_TEXT:
                    return new Msg_MutiPackageText();
                case CanMsgId.BRM:
                    return new Msg_BRM();
                case CanMsgId.BCP:
                    return new Msg_BCP();
                case CanMsgId.CTS:
                    return new Msg_CTS();
                case CanMsgId.CML:
                    return new Msg_CML();
                case CanMsgId.BRO:
                    return new Msg_BRO();
                case CanMsgId.CRO:
                    return new Msg_CRO();
                case CanMsgId.BCL:
                    return new Msg_BCL();
                case CanMsgId.BCS:
                    return new Msg_BCS();
                case CanMsgId.CCS:
                    return new Msg_CCS();
                case CanMsgId.BSM:
                    return new Msg_BSM();
                case CanMsgId.BMV:
                    return new Msg_BMV();
                case CanMsgId.BMT:
                    return new Msg_BMT();
                case CanMsgId.BSP:
                    return new Msg_BSP();
                case CanMsgId.BST:
                    return new Msg_BST();
                case CanMsgId.CST:
                    return new Msg_CST();
                case CanMsgId.BSD:
                    return new Msg_BSD();
                case CanMsgId.CSD:
                    return new Msg_CSD();
                case CanMsgId.BEM:
                    return new Msg_BEM();
                case CanMsgId.CEM:
                    return new Msg_CEM();
                case CanMsgId.JPDC108://日标
                    return new Msg_JPDC108();
                case CanMsgId.JPDC109:
                    return new Msg_JPDC109();
                case CanMsgId.JPDC100:
                    return new Msg_JPDC100();
                case CanMsgId.JPDC101:
                    return new Msg_JPDC101();
                case CanMsgId.JPDC102:
                    return new Msg_JPDC102();
                case CanMsgId.JPDC118:
                    return new Msg_JPDC118();
                case CanMsgId.JPDC110:
                    return new Msg_JPDC110();
                default:
                    return new Msg_Undefined();
            }
        }

        private string FindMsgSymbol(string flowId, List<byte> content)
        {
            string symbol = flowId;
            if (flowId == CanMsgId.MUTI_PACKAGE_HEAD)
            {
                Prj.Prj.MutiPackage.UpdateMutiPackage_Head(content);    //更新多包信息
                symbol = Prj.Prj.MutiPackage.GetSymbol();  //多包symbol
            }
            else if (flowId == CanMsgId.MUTI_PACKAGE_READY)
            {
                Prj.Prj.MutiPackage.UpdateMutiPackage_Ready(content);
                symbol = Prj.Prj.MutiPackage.GetSymbol();
            }
            else if (flowId == CanMsgId.MUTI_PACKAGE_TEXT)
            {
                symbol = Prj.Prj.MutiPackage.GetSymbol();
            }
            else { }
            return symbol;
        }
        private int GetMuitPckgIndex(List<byte> content)
        {
            try
            {
                string[] arr = PacketHelper.SplitMsgData(content);
                int index = Convert.ToInt32(arr[0], 16);
                return index;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return 0;
        }

        private int SetPackageId(string flowId)
        {
            if (flowId == CanMsgId.MUTI_PACKAGE_HEAD || flowId == CanMsgId.MUTI_PACKAGE_READY || flowId == CanMsgId.MUTI_PACKAGE_TEXT)
            {
                return Prj.Prj.MutiPackage.GetCurrentPackageId();
            }
            else
                return 0;
        }
        private int SetPackageTextId(string flowId)
        {
            if (flowId == CanMsgId.MUTI_PACKAGE_TEXT)
            {
                return Prj.Prj.MutiPackage.GetCurrentTextId();
            }
            else
                return 0;
        }
    }
}
