using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.EquipStateData;
using SaiTer.ATE.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 交流BMS协议
    /// </summary>
    public class proBMS_AC
    {
        public byte[] GBExchangeSetStrAllSend(Double t1, Double t2, bool[] tBitS)//模拟桩或者BMS 为桩时t1为R3  t2为辅助电源电压 或者 为BMS时 t1为R4电阻值 t2为电池电压 
        {
            UInt16 temp1 = 0, temp2 = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x80 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            temp1 = Convert.ToUInt16(t1);
            ReturnbyteSource.Add((byte)(temp1 >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp1);//0000  0000  //// 

            temp2 = Convert.ToUInt16(t2 * 10);
            ReturnbyteSource.Add((byte)(temp2 >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp2);//0000  0000  // 

            UInt64 Temp = 0;
            foreach (bool tBit in tBitS)
            {
                if (tBit)
                {

                    Temp = ((0x0001 | Temp) << 1);
                }
                else
                {

                    Temp = ((0xFFFFFFFE & Temp) << 1);
                }

            }
            Temp = (Temp >> 1);//上面多左移动了1位
            ReturnbyteSource.Add((byte)(Temp >> 24));//0000  0000
            ReturnbyteSource.Add((byte)(Temp >> 16));//0000  0000
            ReturnbyteSource.Add((byte)(Temp >> 8));//0000  0000
            ReturnbyteSource.Add((byte)Temp);//0000  0000


            ReturnbyteSource.Add(0x0d);
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }
        public byte[] BMSSetONOFF(bool istrue)// 模拟桩或者BMS   istrue==true启动充电  
        {

            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x30 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  

            if (istrue)
            {
                ReturnbyteSource.Add(0x01); //启动充电
            }
            else
            {
                ReturnbyteSource.Add(0x00);//停止充电
            }


            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSSetPara(Double t1)///BMS   t1 最高允许总电压   
        {       //充电及充电握手阶段报文设置
            UInt16 temp1 = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x31 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            temp1 = Convert.ToUInt16(t1 * 10);
            ReturnbyteSource.Add((byte)(temp1 >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp1);//0000  0000  //    //1)最高允许电压：精度0.1V，就是400.0V，即0x0fa0;

            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0xFA);//0000  0000  //250ms   2）BHM报文周期：精度1ms，就是4000ms，即0x0fa0;

            ReturnbyteSource.Add(0x01);//0000  0000
            ReturnbyteSource.Add(0x01);//0000  0000  
            ReturnbyteSource.Add(0x00);// 3）协议版本号：版本V1.1，即0x010100;


            ReturnbyteSource.Add(0x03);//0000  0000  5）电池类型：	01：铅酸电池
                                       // 02：镍氢电池
                                       //03：磷酸铁锂电池
                                       //04：锰酸锂电池
                                       //05：钴酸锂电池
                                       //06：三元材料电池
                                       //07：聚合物锂离子电池
                                       //08：钛酸锂电池
                                       //FF：其他电池


            ReturnbyteSource.Add(0x03);//0000  0000
            ReturnbyteSource.Add(0xE8);//0000  0000  100AH// 6）整车电池额定容量：（车体BMS上传值 1byte定点数），精度0.1，放大10倍取整数，如400AH电池就是4000，即0x0fa0； 


            ReturnbyteSource.Add(0x10);//0000  0000
            ReturnbyteSource.Add(0x04);//0000  0000  //410V    7）额定电池总电压：（车体BMS上传值 1byte定点数），精度0.1，放大10倍取整数，如500V，就是5000，即0x1388；


            ReturnbyteSource.Add(0x42);//0000  0000
            ReturnbyteSource.Add(0x59);//0000  0000  // 
            ReturnbyteSource.Add(0x44);//0000  0000
            ReturnbyteSource.Add(0x20);//0000  0000  //BYD   8）电池生产厂商：4字节，标准ASCII码；


            ReturnbyteSource.Add(0x00);//0000  0000  
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x01);//0000  0000  // 9）电池组序号：4字节；

            ReturnbyteSource.Add(0x21);//0000  0000
            ReturnbyteSource.Add(0x05);//0000  0000
            ReturnbyteSource.Add(0x1F);//0000  0000 2018.5.31    10）电池生产日期：年：1年/位，1985年偏移量，就是1990年即X21=0x05；
                                       // 月：1月 / 位，0月偏移量，就是5月即X22 = 0x05；
                                       //日：1日 / 位，0日偏移量，就是5日即X23 = 0x05；
            ReturnbyteSource.Add(0x00);//0000  0000  
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x02);//0000  0000 //  11）电池充电次数：1次 / 位；

            ReturnbyteSource.Add(0x01);//0000  0000         //12）产权标识：0x01代表车自有，0x00代表租赁；


            ReturnbyteSource.Add(0x00);//0000  0000  // 预留

            ReturnbyteSource.Add(0x53);
            ReturnbyteSource.Add(0x41);
            ReturnbyteSource.Add(0x49);
            ReturnbyteSource.Add(0x54);
            ReturnbyteSource.Add(0x45);
            ReturnbyteSource.Add(0x52);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x30);
            ReturnbyteSource.Add(0x31);//SAITER00000000001   车辆识别码（VIN）

            ReturnbyteSource.Add(0x00);//0000  0000  // 预留
            ReturnbyteSource.Add(0x00);//0000  0000  // 预留
            ReturnbyteSource.Add(0x00);//0000  0000  // 预留
            ReturnbyteSource.Add(0x00);//0000  0000  // 预留
            ReturnbyteSource.Add(0x00);//0000  0000  // 预留
            ReturnbyteSource.Add(0x00);//0000  0000  // 预留
            ReturnbyteSource.Add(0x00);//0000  0000  // 预留
            ReturnbyteSource.Add(0x00);//0000  0000  // 预留


            ReturnbyteSource.Add(0x00);
            ReturnbyteSource.Add(0xFA); //250ms BRM报文周期

            ReturnbyteSource.Add(0x0d);
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSSetPara(Double t1, Double t2, double t3)///BMS   t1 动力蓄电池当前电池电压   t2最高允许充电电压
        {       //参数配置阶段报文设置
            UInt16 temp1 = 0, temp2 = 0, temp3 = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x32 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add(0x01);//0000  0000
            ReturnbyteSource.Add(0xf4);//0000  0000  //5V    //1) 单体蓄电池最高允许充电电压：精度0.01V，0V偏移量，就是3.14V，即0x013a；



            temp3 = Convert.ToUInt16(t3 * 10);
            ReturnbyteSource.Add((byte)(temp3 >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp3);//0000  0000  //    //4)最高允许充电电流：精度0.1A，就是400.0A，即0x0fa0;
            //ReturnbyteSource.Add(0x09);//0000  0000
            //ReturnbyteSource.Add(0xC4);//0000  0000  //200A   //2)最高允许充电电流：精度0.1A，就是400.0A，即0x0fa0; 07D0->09C4 2000修改成2500

            ReturnbyteSource.Add(0x07);//0000  0000
            ReturnbyteSource.Add(0xD0);//0000  0000  // 200kWh    3)蓄电池标称总能量：精度0.1kWh，就是400.kWh，即即0x0fa0；

            temp2 = Convert.ToUInt16(t2 * 10);
            ReturnbyteSource.Add((byte)(temp2 >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp2);//0000  0000  //    //4)最高允许充电电压：精度0.1V，就是400.0V，即0x0fa0;

            ReturnbyteSource.Add(0x94);//0000  0000  //98℃  //5)最高允许温度：1℃/位，-50℃偏移，就是10℃，即0x3C；



            ReturnbyteSource.Add(0x03);//0000  0000
            ReturnbyteSource.Add(0x20);//0000  0000  //80%    //6)动力蓄电池荷电状态：0.1％/位，就是50%，即0x01f4； 

            temp1 = Convert.ToUInt16(t1 * 10);
            ReturnbyteSource.Add((byte)(temp1 >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp1);//0000  0000  //    //7)	动力蓄电池当前电池电压：精度0.1V，就是400.0V，即0x0fa0;


            ReturnbyteSource.Add(0x01);//0000  0000
            ReturnbyteSource.Add(0xF4);//0000  0000  //500ms    //  //8)BCP报文周期：精度1ms，就是60ms，即0x003c;


            ReturnbyteSource.Add(0xAA);//0000  0000  //  ////9)	车辆准备就绪状态：准备就绪0xAA，未准备就绪0x00，其他值无效;


            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0xFA);//0000  0000  //250ms  //10)BRO报文周期：精度1ms，就是60ms，即0x003c;


            ReturnbyteSource.Add(0x0d);
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSSetPara(Double t1, Double t2, byte t3, Double t4)////BMS  t1 BMS需求电压设置(V)     t2 BMS需求电流设置(A)   t3 0X01恒压  0X02恒流      t4充电电压测量值
        {
            t2 = System.Math.Abs(t2);
            //充电阶段报文设置
            UInt16 temp1 = 0, temp2 = 0, temp4 = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x33 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            temp1 = Convert.ToUInt16(t1 * 10);
            ReturnbyteSource.Add((byte)(temp1 >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp1);//0000  0000  //// 1）电压需求：精度0.1V，就是400.0V，即0x0fa0;


            temp2 = Convert.ToUInt16(t2 * 10);
            ReturnbyteSource.Add((byte)(temp2 >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp2);//0000  0000  //  2）电流需求：精度0.1A，就是400.0V，即0x0fa0;

            ReturnbyteSource.Add(t3);
            ///////////////////////////////////////////////////////     
            //00 32 
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x32);//0000  0000  //4）50ms	BCL报文周期：精度1ms，就是60ms，即0x003c;

            //ReturnbyteSource.Add(0x0F);//0000  0000
            //ReturnbyteSource.Add(0xA0);//0000  0000 400V // 5）	充电电压测量值：精度0.1V，就是400.0V，即0x0fa0;

            //ReturnbyteSource.Add(0x07);//0000  0000
            //ReturnbyteSource.Add(0x6C);//0000  0000 190V // 5）	充电电压测量值：精度0.1V，就是190.0V，即0x076C;

            //ReturnbyteSource.Add(0x0B);//0000  0000
            //ReturnbyteSource.Add(0x54);//0000  0000 290V // 5）	充电电压测量值：精度0.1V，就是290.0V，即0x0B54;


            temp4 = Convert.ToUInt16(t4 * 10);
            ReturnbyteSource.Add((byte)(temp4 >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp4);//0000  0000  //// 1）充电电压测量值：精度0.1V，就是290.0V，即0x0B54;


            ReturnbyteSource.Add(0x01);//0000  0000
            ReturnbyteSource.Add(0x2C);//0000  0000 30A // 6）	充电电流测量值：精度0.1A，就是400.0V，即0x0fa0;

            ReturnbyteSource.Add(0xA1);//0000  0000
            ReturnbyteSource.Add(0x90);//0000  0000  10// //  7）最高单体电池电压及其组号：1 - 12位：最高单体动力蓄电池电压，数据分辨率：0.01 V / 位，0 V偏移量；数据范围：0~24 V；
            ////////////////////////////////////////////////13 - 16位：最高单体动力蓄电池电压所在组号，数据分辨率：1 / 位，0偏移量；数据范围：0~15；
            /////////////////////////////////////////////////

            // 1E 
            ReturnbyteSource.Add(0x1E);//0000  0000  30%//8）	当前荷电状态：数据分辨率：1 %/ 位，0 % 偏移量；数据范围：0~100 %；

            // 07 D0 
            ReturnbyteSource.Add(0x07);//0000  0000
            ReturnbyteSource.Add(0xD0);//0000  0000  //200min  //9）	估算剩余充电时间：精度1min，就是400.0min，即0x0fa0;


            //00 FA 
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0xFA);//0000  0000   10）250ms BCS报文周期：精度1ms，就是60ms，即0x003c;


            ReturnbyteSource.Add(0x02);//0000  0000  //  10）	单体最高电池电压编号：数据分辨率：1 / 位，1偏移量；数据范围：1~256；
            ReturnbyteSource.Add(0x5F);//0000  0000  45℃ //11）	最高电池温度：数据分辨率：1℃位，-50 ℃偏移量；数据范围：-50 ºC ~+200 ℃；
            ReturnbyteSource.Add(0x03);//0000  0000  //12）	最高温度编号：1 / 位，1偏移量；数据范围：1~128；
            ReturnbyteSource.Add(0x5A);//0000  0000 40℃    //13）	最低电池温度：数据分辨率：1℃/ 位，-50 ℃偏移量；数据范围：-50 ºC ~+200 ℃；
            ReturnbyteSource.Add(0x04);//0000  0000 //14）	最低温度编号：1 / 位，1偏移量；数据范围：1~128；


            //00 10 
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x10);//0000  0000  15）	相关状态：
            //  X24.1 - X24.2 单体动力蓄电池电压过高 / 过低(< 00 >：= 正常; < 01 >：= 过高; < 10 >：= 过低)
            //X24.3 - X24.4 整车动力蓄电池荷电状态SOC过高 / 过低(< 00 >：= 正常; < 01 >：= 过高; < 10 >：= 过低)
            //X24.5 - X24.6 动力蓄电池充电过电流(< 00 >：= 正常; < 01 >：= 过流; < 10 >：= 不可信状态)
            //X24.7 - X24.8动力蓄电池温度过高(< 00 >：= 正常; < 01 >：= 过高; < 10 >：= 不可信状态)
            //X25.1 - X25.2 动力蓄电池绝缘状态(< 00 >：= 正常; < 01 >：= 不正常; < 10 >：= 不可信状态)
            //X25.3 - X25.4 动力蓄电池组输出连接器连接状态(< 00 >：= 正常; < 01 >：= 不正常; < 10 >：= 不可信状态)
            //X25.5 - X25.6 充电允许(< 00 >：= 禁止; < 01 >：= 允许)


            //  00 FA 
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0xFA);//0000  0000  250ms  	16）	BSM报文周期：精度1ms，就是60ms，即0x003c;

            //00
            ReturnbyteSource.Add(0x00);//0000  0000    17）	BMS终止充电原因

            //00 00
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  	18）	BMS终止充电故障原因：

            // 00
            ReturnbyteSource.Add(0x00);//0000  0000        19）	BMS终止充电错误原因：


            // 00 0A
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x0A);//0000  0000  	20）BST报文周期：精度1ms，就是60ms，即0x003c;

            ReturnbyteSource.Add(0x0d);//0D 95 A7
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSSetPara(Double t1, Double t2, bool isTrue, Double t4)//BMS  t1 BMS需求电压设置(V)     t2 BMS需求电流设置(A)  isTrue=true恒压  t4充电电压测量值
        {
            if (isTrue)// 3）充电模式：0x01：恒压充电；0x02：恒流充电；
            { return BMSSetPara(t1, t2, 0x01, t4); }//恒压充电
            else
            { return BMSSetPara(t1, t2, 0x02, t4); }//恒流充电

        }

        public byte[] BMSSetResistance(Double t1)//模拟桩或者BMS
        {
            //设置互操电阻阻值（桩模拟器R3/车模拟器R4）
            UInt16 temp = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x81 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            temp = Convert.ToUInt16(t1);
            ReturnbyteSource.Add((byte)(temp >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp);//0000  0000  //// 
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSSetBatteryVoltage(Double t1)//BMS 
        {
            //设置互操电池电压
            UInt16 temp = 0;
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x82 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list
            temp = Convert.ToUInt16(t1 * 10);
            ReturnbyteSource.Add((byte)(temp >> 8));//0000  0000
            ReturnbyteSource.Add((byte)temp);//0000  0000  //// 
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            ReturnbyteSource.Add(0x00);//0000  0000  

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }


        public byte[] BMSSetControl(byte tComm, bool isTrue)//模拟桩或者BMS  通断控制 true 1代表闭合， false 0代表断开
        {
            //直流CC1通断控制 下发0x88设置（桩模拟器R3/车模拟器R4）
            //直流CC2通断控制 下发0x89设置（桩模拟器R3/车模拟器R4）
            //直流开关S通断控制 下发0x8A设置（桩模拟器R3/车模拟器R4）

            //直流电池反接控制 下发0x8B设置（桩模拟器R3/车模拟器R4）
            //直流输出过压控制 下发0x8C设置（桩模拟器R3/车模拟器R4）
            //直流停止发送报文设置 下发0x8D设置（桩模拟器R3/车模拟器R4）
            //充电桩报文设置读取 下发0x50设置 ）	X4 = 01，代表设置启动BMS报文数据读取，X4 = 00，代表关闭BMS报文数据读取；
            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add(tComm);//0000  0000  

            ReturnbyteSource.Add(0x00);//0000  0000  
            ReturnbyteSource.Add(0x00);//0000  0000   
            ReturnbyteSource.Add(0x00);//0000  0000
            if (isTrue)
            { ReturnbyteSource.Add(0x01); }// true 0x01代表闭合//X4

            else
            { ReturnbyteSource.Add(0x00); }// false 0x00代表断开//X4

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSProtocolConsistency(byte byte0, byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7)// BMS 协议一致性测试设置
        {

            List<byte> ReturnbyteSource = new List<byte>();
            byte[] PrefixCode = new byte[] { 0x7e, 0x00, 0xff, 0x03, 0x70 };//前缀
            ReturnbyteSource.AddRange(PrefixCode);//前缀压入list

            ReturnbyteSource.Add(byte0);//0000  0000   
            ReturnbyteSource.Add(byte1);//0000  0000
            ReturnbyteSource.Add(byte2);//0000  0000  
            ReturnbyteSource.Add(byte3);//
            ReturnbyteSource.Add(byte4);//0000  0000   
            ReturnbyteSource.Add(byte5);//0000  0000
            ReturnbyteSource.Add(byte6);//0000  0000  
            ReturnbyteSource.Add(byte7);//

            ReturnbyteSource.Add(0x0d);//
            byte[] writeBuff = ReturnbyteSource.ToArray();//把前面压入的数据转成成数组，校验
            byte[] CheckSumByte = CheckOut.GetModbusCrc16_High_Right(writeBuff);//CRC校验函数。
            ReturnbyteSource.AddRange(CheckSumByte);//把校验压入list
            byte[] writeBuffer = ReturnbyteSource.ToArray();//返回要发送的字节
            return writeBuffer;
        }

        public byte[] BMSReadData()
        {
            //直流
            // byte[] BMSBuffer = { 0x7e, 0x00, 0xff, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d, 0x16, 0x96 };

            //交流
            byte[] BMSBuffer = { 0x7e, 0x00, 0xff, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d, 0xe6, 0x99 };
            return BMSBuffer;
        }
        /// <summary>
        /// 解析BMS实时数据
        /// </summary>
        /// <param name="buffer">报文</param>
        /// <param name="chargerID">枪编号</param>
        /// <returns></returns>
        public BMS_AC_StateData GetBMSStateData(byte[] buffer, int chargerID)
        {

            BMS_AC_StateData bms = new BMS_AC_StateData();
            bms.ChargerID = chargerID;
            if (buffer.Length >= 50)
            {
                int temp = Convert.ToInt32(buffer[37].ToString("x") + buffer[38].ToString("x"), 16);
                bms.ChargerTemp = Convert.ToSingle(temp) / 100 ;
                bms.CPVoltage = 12;
                bms.CPFrequency = 50;
                bms.SystemState = "待机中";
            }

            return bms;
        }
    }
}
