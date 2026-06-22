using SaiTer.ATE.DataModel.CAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    public class PacketHelper
    {
        /// <summary>
        /// 通过报文ID返回类别
        /// </summary>
        /// <param name="msgid"></param>
        /// <returns></returns>
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
            StringBuilder sb = new StringBuilder();
            src = src.Substring(0, dlc * 2);
            for (int i = 0; i < src.Length; i += 2)
            {
                sb.Append(src.Substring(i, 2));
                sb.Append(" ");
            }
            return sb.ToString();
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
            double shrinkV = ShrinkCntTimes(val, shrink);
            double result = KeepCntDecimalPlaces(shrinkV, keep);
            string format = "f" + keep.ToString();
            return result.ToString(format);
        }
        public static double Shrink10Keep1ByStr(string low, string high)
        {
            int volt = BaseConvert.HexStr2Int32(high + low);
            double shrinkV = ShrinkCntTimes(volt, 10);
            double result = KeepCntDecimalPlaces(shrinkV, 1);
            return result;
        }
        public static double Shrink100Keep2ByStr(string low, string high)
        {
            int volt = BaseConvert.HexStr2Int32(high + low);
            double shrinkV = ShrinkCntTimes(volt, 100);
            double result = KeepCntDecimalPlaces(shrinkV, 2);
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
            double result = keepV - offset;
            return result;
        }
        public static string GetState(string bits, string val1, string val2, string val3)
        {
            if (bits == "00")
                return val1;
            else if (bits == "01")
                return val2;
            else
                return val3;

        }
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

    public class Punctuation
    {
        public const string Space = " ";
        public const string Colon = ":";
    }

    public class Machine
    {
        public const string EQ = "充电机报文";
        //public const string Car = "车辆报文";
        public const string Bms = "BMS报文";
    }



    public class PGN
    {
        public const string BRM = "02";
        public const string BCS = "11";
        public const string BMV = "15";
        public const string BMT = "16";
        public const string BCP = "06";
        public const string BSP = "17";
    }

    public class TextFormat
    {
        public const string Date = "yyyyMMdd HH:mm:ss fff";
    }

    public class MdiText_Common
    {
        public const string Illegal = "输入的数值不合法！请输入正确的数值！";
    }

    public class MdiText_Calc
    {
        public const string Calculating = "计算中...";
        public const string CalcFinish = "计算完成";
    }

    public class BatteryType
    {
        public const string H01 = "01";
        public const string H02 = "02";
        public const string H03 = "03";
        public const string H04 = "04";
        public const string H05 = "05";
        public const string H06 = "06";
        public const string H07 = "07";
        public const string H08 = "08";
        public const string HFF = "FF";
    }

    public class SPN
    {
        public enum StateName
        {
            SPN3090,
            SPN3091,
            SPN3092,
            SPN3093,
            SPN3094,
            SPN3095,
            SPN3096,

            SPN3511_12,
            SPN3511_34,
            SPN3511_56,
            SPN3511_78,

            SPN3512_12,
            SPN3512_34,
            SPN3512_56,
            SPN3512_78,
            SPN3512_9,
            SPN3512_11,
            SPN3512_13,
            SPN3512_15,

            SPN3513_12,
            SPN3513_34,

            SPN3521_12,
            SPN3521_34,
            SPN3521_56,
            SPN3521_78,

            SPN3522_12,
            SPN3522_34,
            SPN3522_56,
            SPN3522_78,
            SPN3522_9,
            SPN3522_11,

            SPN3523_12,
            SPN3523_34,

            SPN3901,
            SPN3902,
            SPN3903,
            SPN3904,
            SPN3905,
            SPN3906,
            SPN3907,

            SPN3921,
            SPN3922,
            SPN3923,
            SPN3924,
            SPN3925,
            SPN3926,
            SPN3927
        }
        public class StateType
        {
            public const string Normal = "正常";
            public const string TooHigh = "过高";
            public const string TooLow = "过低";
            public const string OverCurrent = "过流";
            public const string OverTemp = "过温";
            public const string TimeOut = "超时";
            public const string Untrusted = "不可信状态";
            public const string Forbid = "禁止";
            public const string Permit = "允许";

            public const string Abnormal = "不正常";
            public const string Undefined = "Undefined SPN";

            public const string NotAchievedSoc = "未达到所需SOC目标值";
            public const string AchievedSoc = "达到所需SOC目标值";

            public const string NotAchievedTotalVolt = "未达到总电压设定值";
            public const string AchievedTotalVolt = "达到总电压设定值";

            public const string NotAchievedSingleVolt = "未达到单体电压的设定值";
            public const string AchievedSingleVolt = "达到单体电压的设定值";

            public const string AchievedConditionPaused = "达到充电机设定条件中止";

            public const string ChargePaused = "充电机中止(收到CST帧)";

            public const string Trouble = "故障";
            public const string OverReq = "超过需求值";
            public const string Unusual = "异常";

            public const string Paused = "中止";
            public const string ManPaused = "人工中止";
            public const string TroublePaused = "故障中止";
            public const string BSTPaused = "故障中止(收到BST帧)";
            public const string BmsPaused = "BMS主动中止";
            public const string CannotTransfer = "不能传送";
            public const string EmergencyStop = "急停";

            public const string Matched = "匹配";
            public const string Mismatched = "不匹配";
        }
        public enum SPN2560
        {
            Recognized,
            Unrecognized
        }
        public enum SPN2829
        {
            BeReady,
            NotReady,
            Invalid
        }
        public enum SPN3929
        {
            Permit,
            Pause
        }
    }

    public class MatchClass
    {
        public enum StateName
        {
            HighLow,
            OverI,
            TooHigh,
            Abnormal,
            Permit,
            Achieved,
            Pause,
            Trouble,
            ExceedingDemand,
            Unusual,
            AchievedConditionPaused,
            ManPaused,
            TroublePaused,
            BMSPaused,
            HighTemp,
            EnergyTransfer,
            EmergencyStop,
            Mismatched
        }
        public class StateType
        {
            public const string Normal = "正常";
            public const string TooHigh = "过高";
            public const string TooLow = "过低";
            public const string OverCurrent = "过流";
            public const string OverTemp = "过温";
            public const string TimeOut = "超时";
            public const string Untrusted = "不可信状态";
            public const string Forbid = "禁止";
            public const string Permit = "允许";

            public const string Abnormal = "不正常";
            public const string Undefined = "Undefined SPN";

            public const string NotAchievedSoc = "未达到所需SOC目标值";
            public const string AchievedSoc = "达到所需SOC目标值";

            public const string NotAchievedTotalVolt = "未达到总电压设定值";
            public const string AchievedTotalVolt = "达到总电压设定值";

            public const string NotAchievedSingleVolt = "未达到单体电压的设定值";
            public const string AchievedSingleVolt = "达到单体电压的设定值";

            public const string NotAchieved = "未达到";
            public const string Achieved = "达到";

            public const string AchievedConditionPaused = "达到充电机设定条件中止";

            public const string ChargePaused = "充电机中止(收到CST帧)";

            public const string Trouble = "故障";
            public const string OverReq = "超过需求值";
            public const string Unusual = "异常";

            public const string Paused = "中止";
            public const string ManPaused = "人工中止";
            public const string TroublePaused = "故障中止";
            public const string BSTPaused = "故障中止(收到BST帧)";
            public const string BmsPaused = "BMS主动中止";
            public const string CannotTransfer = "不能传送";
            public const string EmergencyStop = "急停";

            public const string Matched = "匹配";
            public const string Mismatched = "不匹配";
        }
    }
    public class SwitchSource
    {
        public enum Source
        {
            Light,
            Total,
            Moment
        }
    }
    public class HeaderText
    {
        public const string OBJECT_NO = "帧序号";
        public const string DIRECTION = "收发标志";
        public const string CREATE_TIME = "接收时间";
        public const string TIME_INCREMENT = "时间增量";//add for 时间增量
        public const string MSG_ID = "帧ID";
        public const string DLC = "DLC";
        public const string MSG_DATA = "数据";
        public const string MSG_TEXT = "BMS报文翻译";

        public const string INTEROP_NO = "序号";
        public const string INTEROP_NAME = "测试项目";
        public const string INTEROP_PURPOSE = "测试目的";
        public const string INTEROP_STEP = "测试步骤";
        public const string INTEROP_JUDGE = "合格评判";

        public const string MSG_NAME = "报文名称";
        public const string MSG_COUNT = "报文总数";
        public const string MIN_INTERVAL = "最小间隔(ms)";
        public const string MAX_INTERVAL = "最大间隔(ms)";
        public const string AVG_INTERVAL = "平均间隔(ms)";
        public const string BeginDate = "开始时间";
        public const string EndDate = "结束时间";

        public const string CURVE_CHARGE_V = "充电电压(1V/div)";
        public const string CURVE_CHARGE_I = "充电电流(0.3A/div)";
        public const string CURVE_CC1 = "CC1电压(0.3V/div)";
        public const string CURVE_CC2 = "CC2电压(0.3V/div)";
        public const string CURVE_ASSIST = "辅源电压(0.3V/div)";
    }


    public class Interop
    {
        public class ItemID
        {
            public const string D00001 = "D00001";
            public const string D01001 = "D01001";
            public const string D02001 = "D02001";
            public const string D03001 = "D03001";
            public const string D04001 = "D04001";
            public const string D05001 = "D05001";
            public const string D06001 = "D06001";
            public const string D04501 = "D04501";
            public const string D04502 = "D04502";
            public const string D04503 = "D04503";
            public const string D04504 = "D04504";
            public const string D02501 = "D02501";
            public const string D04505 = "D04505";
            public const string D04506 = "D04506";
            public const string D03101 = "D03101";
            public const string D04101 = "D04101";
            public const string D04102 = "D04102";
            public const string D04103 = "D04103";
            public const string D05101 = "D05101";
            public const string D06002 = "D06002";
        }
        public class Result
        {
            public const string Qualified = "合格";
            public const string Unqualified = "不合格";
        }
    }

    public class Consist
    {
        public class ItemId
        {
            //public const string BP1001 = "BP1001";
            //public const string BP1002 = "BP1002";
            //public const string BP1003 = "BP1003";

            //public const string BN1001 = "BN1001";
            //public const string BN1002 = "BN1002";
            //public const string BN1003 = "BN1003";
            //public const string BN1004 = "BN1004";
            //public const string BN1005 = "BN1005";
            //public const string BN1006 = "BN1006";
            //public const string BN1007 = "BN1007";
            //public const string BN1008 = "BN1008";
            //public const string BN1009 = "BN1009";
            //public const string BN1010 = "BN1010";
            //public const string BP2001 = "BP2001";
            //public const string BP2002 = "BP2002";
            //public const string BP2003 = "BP2003";
            //public const string BN2001 = "BN2001";
            //public const string BN2002 = "BN2002";
            //public const string BN2003 = "BN2003";
            //public const string BN2004 = "BN2004";
            //public const string BN2005 = "BN2005";
            //public const string BN2006 = "BN2006";
            //public const string BN2007 = "BN2007";
            //public const string BP3001 = "BP3001";
            //public const string BP3002 = "BP3002";
            //public const string BP3003 = "BP3003";
            //public const string BP3004 = "BP3004";
            //public const string BP3005 = "BP3005";
            //public const string BN3001 = "BN3001";
            //public const string BN3002 = "BN3002";
            //public const string BN3003 = "BN3003";
            //public const string BN3004 = "BN3004";
            //public const string BN3005 = "BN3005";
            //public const string BN3006 = "BN3006";
            //public const string BN3007 = "BN3007";
            //public const string BN3008 = "BN3008";
            //public const string BP4001 = "BP4001";
            //public const string BP4002 = "BP4002";
            //public const string BP4003 = "BP4003";
            //public const string BN4001 = "BN4001";
            //public const string BN4002 = "BN4002";
            //public const string BN4003 = "BN4003";

            public const string DP1001 = "DP1001";
            public const string DP1002 = "DP1002";
            public const string DP1003 = "DP1003";
            public const string DN1001 = "DN1001";
            public const string DN1002 = "DN1002";
            public const string DN1003 = "DN1003";
            public const string DN1004 = "DN1004";
            public const string DP2001 = "DP2001";
            public const string DP2002 = "DP2002";
            public const string DP2003 = "DP2003";
            public const string DN2001 = "DN2001";
            public const string DN2002 = "DN2002";
            public const string DN2003 = "DN2003";
            public const string DN2004 = "DN2004";
            public const string DN2005 = "DN2005";
            public const string DN2006 = "DN2006";
            public const string DN2007 = "DN2007";
            public const string DN2008 = "DN2008";
            public const string DN2009 = "DN2009";
            public const string DN2010 = "DN2010";
            public const string DP3001 = "DP3001";
            public const string DP3002 = "DP3002";
            public const string DP3003 = "DP3003";
            public const string DP3004 = "DP3004";
            public const string DP3005 = "DP3005";
            public const string DP3006 = "DP3006";
            public const string DP3007 = "DP3007";
            public const string DN3001 = "DN3001";
            public const string DN3002 = "DN3002";
            public const string DN3003 = "DN3003";
            public const string DN3004 = "DN3004";
            public const string DN3005 = "DN3005";
            public const string DN3006 = "DN3006";
            public const string DN3007 = "DN3007";
            public const string DN3008 = "DN3008";
            public const string DN3009 = "DN3009";
            public const string DN3010 = "DN3010";
            public const string DP4001 = "DP4001";
            public const string DP4002 = "DP4002";
            public const string DN4001 = "DN4001";
            public const string DN4002 = "DN4002";
            public const string DN4003 = "DN4003";
            public const string DN4004 = "DN4004";
        }
        public class TestItemName
        {
            public const string Format = "报文格式";
            public const string Length = "长度";
            public const string Content = "内容";
            public const string Period = "周期";
            public const string Interval = "间隔";
            public const string MaxInterval = "最大间隔";
            public const string MinInterval = "最小间隔";
        }
        public class Result
        {
            public const string Qualified = "合格";
            public const string Unqualified = "不合格";
        }
        public class TimeError
        {
            public const int Std1s_Positive200ms = 200;
            public const int Std5s_Positive500ms = 500;
            public const int Std10s_Positive3000ms = 3000;
            public const int Std10ms_PositiveNegative3ms = 3;
            public const double Std50ms_PositiveNegative10Per = 0.1;

            public const long Std250ms_PositiveNegative = 25;
        }

    }
    public class HexState
    {
        public const string READY = "AA";
        public const string UNREADY = "00";
        public const string INVALID = "55";
    }
    public class TimeToSend
    {
        public enum Page
        {
            BaseInfo,
            Alarm,
            Handshake,
            ChargeParaGet,
            ChargingGet,
            ChargeStop,
            DCGet,
            ACGet,
            ACInterop,
            VersionGet,
            SetTestPara,
            BaseInfoJPDC,
            ParaSetJpDC,
            InteropJPDC,
            BMSHandshakeGet,
            BMSChargeParaGet,
            BMSChargingGet,
            BMSChargeStopGet,
            ParaGetH118JpDC,
            MeasureDCInfo
        }
    }
    public class ACTest
    {
        public enum Gun
        {
            Single,
            Dual
        }
    }
    public class WavePara
    {
        public const int CurveCnt = 23;
        public const int LineCnt = 9;
    }
    public class WinLabel
    {
        public const string ST9980AP = "充电桩测试系统ST-9980A+";
        public const string ST9980APNZ = "直流充电桩综合校验仪";
        public const string ST9980A = "充电桩测试系统ST-9980A";
        public const string ST9980BP = "充电桩测试系统ST-9980B+";
        public const string ST9980BPNZ = "交流充电桩综合校验仪";
        public const string ST990 = "充电桩测试系统ST-990";
        public const string ST990AL = "充电桩测试系统ST-990AL";
        public const string ST9980UAAC = "充电桩测试系统ST-9980UA-AC";
        public const string ST9980EAAC = "充电桩测试系统ST-9980EA-AC";
        public const string ST9980CADC = "充电桩测试系统ST-9980CA-DC";
        public const string ST990BL = "充电桩测试系统ST-990BL";
        public const string ST910DC = "充电桩测试系统ST-910DC";
        public const string ST910AC = "充电桩测试系统ST-910AC";

        public const string AST9000DC = "充电桩车载测试系统AST-9000-DC";
        public const string AST9000AC = "充电桩车载测试系统AST-9000-AC";
    }

    //传输协议功能的状态：同意并准备接收，完成接收，拒绝接收
    public enum TransitState
    {
        Ready,
        Finish,
        Reject
    }
}
