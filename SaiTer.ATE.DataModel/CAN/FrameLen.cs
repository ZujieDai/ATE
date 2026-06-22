using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.CAN
{
    public static class FrameLen
    {
        public const int DLC_LEN = 2;//加了dlc之后为2

        public const int UNDEFINED = 0;
        //public const int CAN = 36 - 12 + DLC_LEN;

        public const int BASE_INFO = 46 - 12 + 16 + 8;
        public const int BASE_INFO_OLD = 46 - 12;
        public const int SYS_START = 20 - 12;

        public const int CHARGE_WRONG_GET = 46 - 12;
        public const int CHARGE_WRONG_SET = 20 - 12;


        public const int DC_GET = 16;
        public const int AC_GET = 144;
        public const int HANDSHAKE_GET = 22;//34
        public const int CHARGE_PARA_GET = 34;
        public const int CHARGING_GET = 24;//34
        public const int CHARGE_STOP_GET = 16;//34

        public const int HANDSHAKE_SET = 110;
        public const int CHARGE_PARA_SET = 36;
        public const int CHARGING_SET = 66;
        public const int CHARGE_STOP_SET = 18;

        public const int DC_SET = 16;
        public const int AC_SET = 16;

        public const int CONSIST = 16;
        public const int UPGRADE_ACK = 8;

        public const int ALARM_GET = 8;
        public const int AC_INTEROP_GET = 16;
        public const int VER_GET = 16;
        public const int ACVIN_SET = 40;
        public const int ACR_GET = 16;

        //public const int CAN = 44;  //add for 实时时间 
        //public const int CAN = 26;  //add for 无时间
        public const int CAN = 30;  //add for 时间间隔
        public const int CAN_SET = 8;//设置Can
        public const int TIME_SYNC = 18;//add for 实时时间 
        public const int SYS_SAVEPRA = 8;//参数保存
        public const int JPDCPARA_SET = 16;//日标参数设置返回报文
        public const int JPDCPARA_GET = 32;
        public const int JPDCPARABMS_GET = 48;
        public const int JPDCPARAH118_GET = 16;
        public const int JPDCINTEROPGET = 16;//互操作开关读取
        public const int BASEJPDC_INFO = 24;
        public const int CANNew = 10000;  //新报文解析，长度不固定，虚拟
        public const int CANNewSTWave = 10000;  //新录波板报文解析，长度不固定，虚拟

        public const int MeasureAC_Amplitude_Get = 48;
        public const int MeasureAC_PH_HZ_Get = 88;
        public const int MeasureAC_Power_Get = 96;
        public const int MeasureAC_ElectricEnergy_Get = 96;
        public const int MeasureAC_Gear_Get = 56;//交流档位
        public const int MeasureAC_Model_Set = 16;//交流模式
        public const int MeasureAC_PulseType_Set = 16;//脉冲类型
        public const int MeasureAC_MeterPulseErrParam = 120;//脉冲误差参数

        public const int MeasureDC_Amplitude_Get = 16;//直流幅值
        public const int MeasureDC_Power_Get = 8;//直流功率
        public const int MeasureDC_ElectricEnergy_Get = 8;//直流电能
                                                          //public const string MeasureDC_EnergyStart_Set = "65";//启动电能计量
        public const int MeasureDC_MeterPulseErrParam = 80;//脉冲误差参数
                                                           //public const string MeasureDC_PulseErrStart_Set = "67";//启动误差
        public const int MeasureDC_Info_Get = 16;//直流幅值

        public const int Measure_WSD = 16;//温湿度
                                          //public const int Measure_GpsTime = 18;//GPS时间
        public const int Measure_GpsTime = 80;//GPS时间
        public const int Measure_GpsDw = 18;//GPS定位

    }
}
