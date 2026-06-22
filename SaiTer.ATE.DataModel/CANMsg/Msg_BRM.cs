using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaiTer.ATE.DataModel
{
    public class Msg_BRM : MsgCommon
    {
        private string MsgHeadLine = "BMS辨识报文";
        private string LastPckgText = "多包报文 该报文为最后一包";

        private string TestVersion = "协议版本";
        private string TestBatType = "电池类型";
        private string TestBatCapacity = "整车动力蓄电池系统额定容量";
        private string TestBatV = "整车动力蓄电池系统额定总电压";
        private string TestVendor = "电池生产厂商名称";
        private string TestBatNum = "电池组序号";
        private string TestBatProduceDate = "电池组生产日期";
        private string TestBatChargeCnt = "电池组充电次数";
        private string TestBatProperty = "电池组产权标识";
        private string TestVin = "车辆识别码VIN";

        private string H01 = "01";
        private string PropertyBorrow = "租赁";
        private string PropertyPrivate = "车自有";

        //private string
        //private string
        //private string
        //private string

        public override CanMsgRich DecodeMsgData(string symbol, List<byte> content)
        {
            CanMsgRich model = new CanMsgRich();
            string text;
            //int i = 0;
            //List<byte> mutiContent = Prj.Prj.MutiPackage.GetMutiContent();  //使用多包数据

            try
            {
                string[] arr = PacketHelper.SplitMsgData(content);
                int i = 0;
                string total = BaseConvert.AsciiBytes2String(content);

                //1.版本号
                string ver = DecodeVersion(content);
                text = PacketHelper.TextAddColonSpace(TestVersion, ver);

                //2.电池类型
                i++;
                i++;
                i++;
                string batType= PacketHelper.GetBatteryType(arr[i++]);
                text += PacketHelper.TextAddColonSpace(TestBatType, batType);

                //3.整车动力蓄电池系统额定容量
                string batCap = DecodeBatCapacity(total);
                text+= PacketHelper.TextAddColonSpace(TestBatCapacity, batCap);

                //4.整车动力蓄电池系统额定总电压
                string batV = DecodeBatVolt(total);
                text += PacketHelper.TextAddColonSpace(TestBatV, batV);

      
                //5.电池生产厂商名称
                string vendor = DecodeBatVendor(total);
                //text += PacketHelper.TextAddColonSpace(TestVendor, vendor);
                string text2 = PacketHelper.TextAddColonSpace(TestVendor, vendor);

                //6.电池组序号
                string batNum = DecodeBatNum(total);
                text2 += PacketHelper.TextAddColonSpace(TestBatNum, batNum);

                //7.电池组生产日期
                string batProduceDate = DecodeBatProduceDate(arr);
                text2 += PacketHelper.TextAddColonSpace(TestBatProduceDate, batProduceDate);

                //8.电池组充电次数
                string batChargeTimes = DecodeBatChargeTimes(arr);
                text2 += PacketHelper.TextAddColonSpace(TestBatChargeCnt, batChargeTimes);

                //9.电池组产权标识
                string batProperty = DecodeBatProperty(arr);
                text2 += PacketHelper.TextAddColonSpace(TestBatProperty, batProperty);

                //10.车辆识别码(VIN)
                string vin = DecodeVin(arr);
                text2 += PacketHelper.TextAddColonSpace(TestVin, vin);
                string testtext = text + text2;
                model.MsgText = PacketHelper.AppendTextToMsgHead(symbol, MsgHeadLine)+ LastPckgText + Punctuation.Space + testtext;
                return model;
            }
            catch (Exception ex)
            {
                model.MsgText = "Tranlate Error!";
                Log.Log.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
                return model;
            }
        }
        private string DecodeVersion(List<byte> content)
        {
            int i = 0;

            string verH = BaseConvert.AsciiBytes2String(new byte[] { content[i++], content[i++] });
            int H = Convert.ToInt32(verH, 16);

            string verL_00FF = BaseConvert.AsciiBytes2String(new byte[] { content[i++], content[i++] });
            string verL_FF00 = BaseConvert.AsciiBytes2String(new byte[] { content[i++], content[i++] });
            string verL = verL_FF00 + verL_00FF;
            int L = Convert.ToInt32(verL, 16);

            string ver = "V" + H.ToString() + "." + L.ToString();
            return ver;
        }

        private string DecodeBatCapacity(string total)
        {
            int i = 2;
            int capacity = BaseConvert.TwoBytes2Int32(total, 4 * i, 5 * i);
            double shrinkV = PacketHelper.ShrinkCntTimes(capacity, 10);
            double result = PacketHelper.KeepCntDecimalPlaces(shrinkV, 1);
            return result.ToString("f1") + "Ah";
        }
        private string DecodeBatVolt(string total)
        {
            int i = 2;
            int volt = BaseConvert.TwoBytes2Int32(total, 6 * i, 7 * i);
            double shrinkV = PacketHelper.ShrinkCntTimes(volt, 10);
            double result = PacketHelper.KeepCntDecimalPlaces(shrinkV, 1);
            return result.ToString("f1") + "V";
        }
        private string DecodeBatVendor(string total)
        {
            int i = 8;
            string char1 = total.Substring((i++) * 2, 2);
            string char2 = total.Substring((i++) * 2, 2);
            string char3 = total.Substring((i++) * 2, 2);
            string char4 = total.Substring((i++) * 2, 2);

            char1 = char1.Replace("00", "20");//ascii码0x20 - 空格字符，替换NULL字符
            char2 = char2.Replace("00", "20");
            char3 = char3.Replace("00", "20");//ascii码0x20 - 空格字符，替换NULL字符
            char4 = char4.Replace("00", "20");

            string vendor = BaseConvert.HexString2AsciiString(char1)
                    + BaseConvert.HexString2AsciiString(char2)
                    + BaseConvert.HexString2AsciiString(char3)
                    + BaseConvert.HexString2AsciiString(char4);
            return vendor;
        }
        private string DecodeBatNum(string total)
        {

            string char1 = total.Substring(12 * 2, 2);
            string char2 = total.Substring(13 * 2, 2);
            string char3 = total.Substring(14 * 2, 2);
            string char4 = total.Substring(15 * 2, 2);
            string sNum = char4 + char3 + char2 + char1;
            int num = Convert.ToInt32(sNum, 16);
            return num.ToString();
        }
        private string DecodeBatProduceDate(string[] arr)
        {
            int year = BaseConvert.HexStr2Int32(arr[16]) + 1985;
            int month = BaseConvert.HexStr2Int32(arr[17]);
            int day = BaseConvert.HexStr2Int32(arr[18]);

            return year.ToString() + "年" + month.ToString() + "月" + day.ToString() + "日";
        }

        private string DecodeBatChargeTimes(string[] arr)
        {
            string sTimes = arr[21] + arr[20] + arr[19];
            int times = BaseConvert.HexStr2Int32(sTimes);
            return times.ToString();
        }
        private string DecodeBatProperty(string[] arr)
        {
            if (arr[22].Equals(this.H01))   //01
                return this.PropertyPrivate;//车自有
            else
                return this.PropertyBorrow; //租赁
        }
        private string DecodeVin(string[] arr)
        {
            string[] arrVin = BaseConvert.CutArrs(arr, 24, 17);
            string vin = BaseConvert.HexString2AsciiString(arrVin);
            return vin;
        }
    }
}
