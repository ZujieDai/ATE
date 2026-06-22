using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SaiTer.ATE.DataModel.CAN
{
    public class Function
    {
        public static string GetMsgFlow(string msgid)
        {
            string id = msgid.ToUpper().Trim();
            string flow;
            switch (id)
            {
                case "1826F456":
                    flow = CanMsgId.CHM;
                    break;
                case "182756F4":
                    flow = CanMsgId.BHM;
                    break;
                case "1801F456":
                    flow = CanMsgId.CRM;
                    break;
                case "1CEC56F4":
                    flow = CanMsgId.MUTI_PACKAGE_HEAD;
                    break;
                case "1CECF456":
                    flow = CanMsgId.MUTI_PACKAGE_READY;
                    break;
                case "1CEB56F4":
                    flow = CanMsgId.MUTI_PACKAGE_TEXT;
                    break;
                case "1808F456":
                    flow = CanMsgId.CML;
                    break;
                case "1807F456":
                    flow = CanMsgId.CTS;
                    break;
                case "100956F4":
                    flow = CanMsgId.BRO;
                    break;
                case "100AF456":
                    flow = CanMsgId.CRO;
                    break;
                case "181056F4":
                    flow = CanMsgId.BCL;
                    break;
                case "181356F4":
                    flow = CanMsgId.BSM;
                    break;
                case "1812F456":
                    flow = CanMsgId.CCS;
                    break;
                case "101956F4":
                    flow = CanMsgId.BST;
                    break;
                case "101AF456":
                    flow = CanMsgId.CST;
                    break;
                case "181C56F4":
                    flow = CanMsgId.BSD;
                    break;
                case "181DF456":
                    flow = CanMsgId.CSD;
                    break;
                case "081E56F4":
                    flow = CanMsgId.BEM;
                    break;
                case "081FF456":
                    flow = CanMsgId.CEM;
                    break;
                case "00000108":
                    flow = CanMsgId.JPDC108;
                    break;
                case "00000109":
                    flow = CanMsgId.JPDC109;
                    break;
                case "00000100":
                    flow = CanMsgId.JPDC100;
                    break;
                case "00000101":
                    flow = CanMsgId.JPDC101;
                    break;
                case "00000102":
                    flow = CanMsgId.JPDC102;
                    break;
                case "00000110":
                    flow = CanMsgId.JPDC110;
                    break;
                case "00000118":
                    flow = CanMsgId.JPDC118;
                    break;
                default:
                    flow = CanMsgId.UNDEFINED;
                    break;
            }
            return flow;
        }

        public static string AppendTextToMsgHead(string symbol, string item)
        {
            string headText = string.Empty;
            switch (symbol)
            {
                case CanMsgId.BHM:
                case CanMsgId.BCL:
                case CanMsgId.BCP:
                case CanMsgId.BCS:
                case CanMsgId.BEM:
                case CanMsgId.BMT:
                case CanMsgId.BMV:
                case CanMsgId.BRM:
                case CanMsgId.BRO:
                case CanMsgId.BSD:
                case CanMsgId.BSM:
                case CanMsgId.BSP:
                case CanMsgId.BST:
                    headText = Machine.Bms;
                    break;
                case CanMsgId.CCS:
                case CanMsgId.CEM:
                case CanMsgId.CHM:
                case CanMsgId.CML:
                case CanMsgId.CRM:
                case CanMsgId.CRO:
                case CanMsgId.CSD:
                case CanMsgId.CST:
                case CanMsgId.CTS:
                    headText = Machine.EQ;
                    break;
            }
            return headText + symbol + Punctuation.Colon + item + Punctuation.Space;
        }
        public static bool CompareBytesEqual(byte[] bytes1, byte[] bytes2)
        {
            if (Enumerable.SequenceEqual(bytes1, bytes2))
                return true;
            else
                return false;
        }
        public static string AppendSpaceIn2Char(string src, int dlc)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                src = src.Substring(0, dlc * 2);
                for (int i = 0; i < src.Length; i += 2)
                {
                    sb.Append(src.Substring(i, 2));
                    sb.Append(" ");
                }
                return sb.ToString();
            }
            catch (Exception e)
            { 
                Log.Log.LogException(e); 
                return null; 
            }
        }
        public static string GetSymbolByPgn(string pgn)
        {

            switch (pgn)
            {
                case PGN.BCP:
                    return CanMsgId.BCP;
                case PGN.BCS:
                    return CanMsgId.BCS;
                case PGN.BMV:
                    return CanMsgId.BMV;
                case PGN.BMT:
                    return CanMsgId.BMT;
                case PGN.BRM:
                    return CanMsgId.BRM;
                case PGN.BSP:
                    return CanMsgId.BSP;
                default:
                    return CanMsgId.UNDEFINED;
            }
        }
        public static string TextAddColonSpace(string testItem, string testResult)
        {
            return testItem + Punctuation.Colon + testResult + Punctuation.Space;
        }

        public static string DecodeCommon2Byte(string high, string low)
        {
            int val = BaseConvert.HexStr2Int32(high + low);
            return val.ToString();
        }
        public static string DecodeCommon1Byte(string str)
        {
            int val = BaseConvert.HexStr2Int32(str);
            return val.ToString();
        }

        public static double ShrinkCntTimes(int num, int cnt)
        {
            double val = Convert.ToDouble(num);
            return val / cnt;
        }
        public static double KeepCntDecimalPlaces(double num, int cnt)
        {
            return Math.Round(num, cnt);
        }
        public static string DecodeCommonShrinkmKeepn(string[] strs, int shrink, int keep)
        {
            string text = "";
            foreach (string str in strs)
            {
                text += str;
            }
            int val = BaseConvert.HexStr2Int32(text);
            double shrinkV = Function.ShrinkCntTimes(val, shrink);
            double result = Function.KeepCntDecimalPlaces(shrinkV, keep);
            string format = "f" + keep.ToString();
            return result.ToString(format);
        }
        public static double Shrink10Keep1ByStr(string low, string high)
        {
            int volt = BaseConvert.HexStr2Int32(high + low);
            double shrinkV = Function.ShrinkCntTimes(volt, 10);
            double result = Function.KeepCntDecimalPlaces(shrinkV, 1);
            return result;
        }
        public static double Shrink100Keep2ByStr(string low, string high)
        {
            int volt = BaseConvert.HexStr2Int32(high + low);
            double shrinkV = Function.ShrinkCntTimes(volt, 100);
            double result = Function.KeepCntDecimalPlaces(shrinkV, 2);
            return result;
        }
        //public static string DecodeBatteryType(string content)
        //{
        //    string sType = content.Substring(8, 2);
        //    return GetBatteryType(sType);
        //}
        public static string GetBatteryType(string sType)
        {
            //string sType = total.Substring(8, 2);
            string bat = string.Empty;
            switch (sType)
            {
                case BatteryType.H01:
                    bat = "铅酸电池";
                    break;
                case BatteryType.H02:
                    bat = "镍氢电池";
                    break;
                case BatteryType.H03:
                    bat = "磷酸铁锂电池";
                    break;
                case BatteryType.H04:
                    bat = "锰酸锂电池";
                    break;
                case BatteryType.H05:
                    bat = "钴酸锂电池";
                    break;
                case BatteryType.H06:
                    bat = "三元材料电池";
                    break;
                case BatteryType.H07:
                    bat = "聚合物锂离子电池";
                    break;
                case BatteryType.H08:
                    bat = "钛酸锂电池";
                    break;
                case BatteryType.HFF:
                    bat = "其他电池";
                    break;
                default:
                    bat = "Undefined";
                    break;
            }
            return bat;
        }
        public static string byteToString(byte[] buff)
        {
            string bs = "";
            bs = BaseConvert.AsciiBytes2String(buff); ;
            return bs;
        }
        public static string[] SplitMsgData(List<byte> content)
        {
        

            List<string> strList = new List<string>();
            for (int i = 0; i < content.Count; i++)
            {
                strList.Add(content[i].ToString("x2"));
            }
            return strList.ToArray();


        }
        public static string[] SplitMsgData(byte[] content)//add for 实时时间
        {
            string msgData = BaseConvert.AsciiBytes2String(content);
            List<string> strList = new List<string>();
            for (int i = 0; i < msgData.Length; i += 2)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(msgData[i]);
                sb.Append(msgData[i + 1]);
                strList.Add(sb.ToString());
            }
            return strList.ToArray();
        }
        public static double ShrinkKeepOffset(int num, int times, int keepCnt, int offset)
        {
            double shrink = ShrinkCntTimes(num, times);
            double keepV = KeepCntDecimalPlaces(shrink, keepCnt);
            double result = offset - keepV;
            return Math.Abs(result);
        }
        /// <summary>
        /// 获取BIT状态值，00-值1，01-值2，10-值3
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <param name="val3"></param>
        /// <returns></returns>
        public static string GetState(string bits, string val1, string val2, string val3)
        {
            if (bits == "00")
                return val1;
            else if (bits == "01")
                return val2;
            else
                return val3;

        }
        /// <summary>
        /// 获取BIT状态值，00-值1，01-值2，10-值3
        /// </summary>
        /// <param name="bits"></param>
        public static string MatchState(string bits, SPN.StateName spn)
        {
            switch (spn)
            {
                case SPN.StateName.SPN3090:
                case SPN.StateName.SPN3091:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.TooHigh, SPN.StateType.TooLow);
                case SPN.StateName.SPN3092:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.OverCurrent, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3093:
                case SPN.StateName.SPN3512_9:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.TooHigh, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3094:
                case SPN.StateName.SPN3095:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.Abnormal, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3096:
                    return GetState(bits, SPN.StateType.Forbid, SPN.StateType.Permit, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3511_12:
                    return GetState(bits, SPN.StateType.NotAchievedSoc, SPN.StateType.AchievedSoc, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3511_34:
                    return GetState(bits, SPN.StateType.NotAchievedTotalVolt, SPN.StateType.AchievedTotalVolt, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3511_56:
                    return GetState(bits, SPN.StateType.NotAchievedSingleVolt, SPN.StateType.AchievedSingleVolt, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3511_78:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.ChargePaused, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3512_12:
                case SPN.StateName.SPN3512_34:
                case SPN.StateName.SPN3512_56:
                case SPN.StateName.SPN3512_78:
                case SPN.StateName.SPN3512_11:
                case SPN.StateName.SPN3512_13:
                case SPN.StateName.SPN3512_15:
                case SPN.StateName.SPN3522_11:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.Trouble, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3513_12:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.OverReq, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3513_34:
                case SPN.StateName.SPN3523_34:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.Unusual, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3521_12:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.AchievedConditionPaused, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3521_34:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.ManPaused, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3521_56:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.TroublePaused, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3521_78:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.BSTPaused, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3522_12:
                case SPN.StateName.SPN3522_56:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.OverTemp, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3522_34:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.Trouble, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3522_78:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.CannotTransfer, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3522_9:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.EmergencyStop, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3523_12:
                    return GetState(bits, SPN.StateType.Matched, SPN.StateType.Mismatched, SPN.StateType.Untrusted);
                case SPN.StateName.SPN3901:
                case SPN.StateName.SPN3902:
                case SPN.StateName.SPN3903:
                case SPN.StateName.SPN3904:
                case SPN.StateName.SPN3905:
                case SPN.StateName.SPN3906:
                case SPN.StateName.SPN3907:
                case SPN.StateName.SPN3921:
                case SPN.StateName.SPN3922:
                case SPN.StateName.SPN3923:
                case SPN.StateName.SPN3924:
                case SPN.StateName.SPN3925:
                case SPN.StateName.SPN3926:
                case SPN.StateName.SPN3927:
                    return GetState(bits, SPN.StateType.Normal, SPN.StateType.TimeOut, SPN.StateType.Untrusted);
                default:
                    return SPN.StateType.Undefined;

            }
        }
        public static string MatchStateText(string bits, MatchClass.StateName state)
        {
            switch (state)
            {
                case MatchClass.StateName.HighLow:
                    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.TooHigh, MatchClass.StateType.TooLow);
                case MatchClass.StateName.OverI:
                    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.OverCurrent, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.TooHigh:
                    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.TooHigh, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.Abnormal:
                    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.Abnormal, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.Permit:
                    return GetState(bits, MatchClass.StateType.Forbid, MatchClass.StateType.Permit, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.Achieved:
                    return GetState(bits, MatchClass.StateType.NotAchieved, MatchClass.StateType.Achieved, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.Pause:
                    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.ChargePaused, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.Trouble:
                    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.Trouble, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.ExceedingDemand:
                    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.OverReq, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.Unusual:
                    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.Unusual, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.AchievedConditionPaused:
                    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.AchievedConditionPaused, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.ManPaused:
                    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.ManPaused, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.TroublePaused:
                    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.TroublePaused, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.BMSPaused:
                    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.BmsPaused, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.HighTemp:
                    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.OverTemp, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.EnergyTransfer:
                    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.CannotTransfer, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.EmergencyStop:
                    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.EmergencyStop, MatchClass.StateType.Untrusted);
                case MatchClass.StateName.Mismatched:
                    return GetState(bits, MatchClass.StateType.Matched, MatchClass.StateType.Mismatched, MatchClass.StateType.Untrusted);
                //case SPN.StateName.SPN3521_78:
                //    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.BSTPaused, MatchClass.StateType.Untrusted);
                //case SPN.StateName.SPN3927:
                //    return GetState(bits, MatchClass.StateType.Normal, MatchClass.StateType.TimeOut, MatchClass.StateType.Untrusted);
                default:
                    return SPN.StateType.Undefined;

            }
        }
        public static DateTime StampToDatetime(string datetext)
        {
            string dateFormat = TextFormat.Date;
            DateTime dt = DateTime.ParseExact(datetext, dateFormat, System.Globalization.CultureInfo.CurrentCulture);
            return dt;
        }
        public static List<long> CalcInterval(List<ConsistMsg> lists)
        {
            List<long> intervalList = new List<long>();

            DateTime pre;
            DateTime next = DateTime.Now;
            if (lists != null && lists.Count != 0)
            {
                if (lists.Count == 1)
                {
                    intervalList.Add(0);
                    return intervalList;
                }
                else
                {
                    for (int i = 0; i < lists.Count; i++)
                    {
                        if (i == 0)
                        {
                            next = StampToDatetime(lists[i].CreateTimestamp);
                            continue;
                        }
                        else
                        {
                            pre = next;
                            next = StampToDatetime(lists[i].CreateTimestamp);
                            TimeSpan span = next.Subtract(pre);
                            long interval = (long)span.TotalMilliseconds;
                            //if (interval <= 1000)//去除大于1s的周期
                            if (interval <= 1000 && interval >= 5)//去除大于1s的周期,小于5ms的周期
                            {
                                intervalList.Add(interval);
                            }
                        }
                    }
                }
                return intervalList;
            }
            return null;
        }
        public static long CalcIntervalByTwoPara(string stampActive, string stampPassive)
        {
            TimeSpan span = StampToDatetime(stampActive).Subtract(StampToDatetime(stampPassive));
            return (long)span.TotalMilliseconds;
        }
        public enum ConsistResult
        {
            Init,
            OK,
            NG,
            ERROR,
        }
        public static string AddSecondsToTimestamp(string time, int seconds)
        {
            DateTime dateTime = StampToDatetime(time);
            DateTime add = dateTime.AddSeconds(seconds);
            return add.ToString(TextFormat.Date);
        }
        public static string ReturnRegisterWarning(int ret)
        {
            string warning = "";
            if (ret == 0)
            {
                warning = "软件已注册！";
            }
            else if (ret == 1)
            {
                warning = "软件尚未注册，请注册软件！";
            }
            else if (ret == 2)
            {
                warning = "注册码与本机不一致,请联系管理员！";
            }
            else if (ret == 3)
            {
                warning = "软件试用已到期！";
            }
            else if (ret == 4)
            {
                warning = "请先注册软件！";
            }
            else
            {
                warning = "软件运行出错，请重新启动！";
            }
            return warning;
        }
        public static string RandomCodeReplaceBySpace(string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append(' ');
                }
            }
            return sb.ToString();
        }
        public static string MapY2Name(double val)
        {
            switch (val)
            {
                case 1: return "-100";
                case 2: return "-50";
                case 3: return "0";
                case 4: return "50";
                case 5: return "100";
                case 6: return "150";
                case 7: return "200";
                case 8: return "250";
                case 9: return "300";
                case 10: return "350";
                case 11: return "400";
                case 12: return "450";
                case 13: return "500";
                case 14: return "550";
                case 15: return "600";
                case 16: return "650";
                case 17: return "700";
                case 18: return "750";
                case 19: return "800";
                case 20: return "850";
                case 21: return "900";
                case 22: return "950";
                case 23: return "1000";
                case 24: return "1050";
                default: return "";
            }
        }
        public static string MapMsgIndex(double val)
        {
            switch (val)
            {
                case 1: return "BHM";
                case 2: return "CHM";
                case 3: return "BRM";
                case 4: return "CRM";
                case 5: return "BCP";
                case 6: return "BRO";
                case 7: return "CTS";
                case 8: return "CML";
                case 9: return "CRO";
                case 10: return "BCL";
                case 11: return "BCS";
                case 12: return "BSM";
                case 13: return "BST";
                case 14: return "CCS";
                case 15: return "CST";
                case 16: return "BSD";
                case 17: return "CSD";
                case 18: return "BEM";
                case 19: return "CEM";
                case 20: return "BSP";
                case 21: return "BMT";
                case 22: return "BMV";
                case 23: return "UNDEFINED";
                default: return "";
            }
        }

        public static string MapMsgIndexJpDC(double val)
        {
            switch (val)
            {
                case 800: return "H100";
                case 900: return "H101";
                case 1000: return "H102";
                case 1100: return "H108";
                case 1200: return "H109";
                //case 0: return "UNDEFINED";
                default: return "";
            }
        }
        public static int MapMsgName(string name)
        {
            switch (name)
            {
                case "BHM": return 1;
                case "CHM": return 2;
                case "BRM": return 3;
                case "CRM": return 4;
                case "BCP": return 5;
                case "BRO": return 6;
                case "CTS": return 7;
                case "CML": return 8;
                case "CRO": return 9;
                case "BCL": return 10;
                case "BCS": return 11;
                case "BSM": return 12;
                case "BST": return 13;
                case "CCS": return 14;
                case "CST": return 15;
                case "BSD": return 16;
                case "CSD": return 17;
                case "BEM": return 18;
                case "CEM": return 19;
                case "BSP": return 20;
                case "BMT": return 21;
                case "BMV": return 22;
                case "UNDEFINED": return 23;

                case "H100": return 800;
                case "H101": return 900;
                case "H102": return 1000;
                case "H108": return 1100;
                case "H109": return 1200;
                default: return 0;
            }
        }


        public static string MapDatetime(double val)
        {
            long ticks = (long)val;
            DateTime date = new DateTime(2000, 1, 1, 0, 0, 0, 0).AddMilliseconds(ticks);
            string hour = date.Hour.ToString().PadLeft(2, '0');
            string minute = date.Minute.ToString().PadLeft(2, '0');
            string second = date.Second.ToString().PadLeft(2, '0');
            string ms = date.Millisecond.ToString().PadLeft(3, '0');
            return hour + Punctuation.Colon + minute + Punctuation.Colon + second + "." + ms;

        }
    }
}
