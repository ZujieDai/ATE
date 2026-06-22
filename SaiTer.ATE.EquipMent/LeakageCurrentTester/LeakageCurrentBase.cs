using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
   序号	地址			备注
    1	14	99999	交流参数	AC额定电流In
    2	16	10--999		AC-In倍数
    3	19	999		AC动作电流上限
    4	20	999		AC动作电流下限
    5	21	9999		AC动作时间上限
    6	22	9999		AC动作时间下限
    7	10	1--30		AC电流上升率
    8	28	1--20000		AC不动作电流输出时间
    9	15	99999	脉动直流参数	DC额定电流In
    10	17	10--999		DC-In倍数
    11	23	999		DC动作电流上限
    12	24	999		DC动作电流下限
    13	25	9999		DC动作时间上限
    14	26	9999		DC动作时间下限
    15	27	1--30		DC电流上升率
    16	28	1--20000		DC不动作电流输出时间
    17	11	0/1	功能选择界面	交流/脉动直流切换
    18	12	0、动作电流 1、闭合电流动作时间 2、突然电流动作时间 3、S不动作时间		交流模式
    19	13	0、动作电流 1、突然电流动作时间 2、S不动作时间		脉动直流模式
    20	18	0、0度 1、90度 2、135度		角度
    21	37		测试显示界面	电压
    22	38			电流
    23	40			RST三相切换
    24	33			显示
    25	44			复位
    26	34			S3
    27	41			预先调整动作电流 返回02正在执行中
    28	32			报警显示
    29	2	1-60000	校准界面	电压系数
    30	3	1-60000		0-35mA交流系数
    31	8	1-60000		大于35mA交流系数
    32	4	1-60000		0度系数
    33	5	1-60000		90度系数
    34	6	1-60000		135度系数
    35	7	0-20		角度补偿
    36	1	1-99		时间补偿
    37	35	0/1		实验模式/校准模式
    38	9	写1保存		保存
    39	36	0、交流 1、直流正0度 2、直流90度 3、直流135度 4、直流负0度 5、直流负90度 6、直流负135度		负载模式
    40	42	0-FFFF		底16负载
    41	43	0-F		高4负载
				
				
				
    波特率：19200，8,N,1;最大可连续读8个数据；				
    MODBUS-RTU 设备地址：1				
    0X03	多个度			
    0X06	一个写			
0X10	多个写			

 * 
 */
namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备基类- 中佳漏电测试仪函数部分
    /// </summary>
    public abstract partial class EquipMentBase
    {
        #region ---------漏电流测试仪/剩余电流测试仪-----------
        /// <summary>
        /// 设置漏电测试参数
        /// </summary>
        /// <param name="viChannel">无效参数，默认0</param>
        /// <param name="viAddr">地址：默认3</param>
        /// <param name="_TestType">测试类型：0，无效；1，漏电脱口电流；2，漏电突现时间；3，漏电闭合时间</param>
        /// <param name="_WaveType">电流波形：0=AC;1=10mA;2=30mA;3=50mA;4=100mA;5=300mA;6=500mA;7=1A;8=3A;9=5A;10=最大电流档位</param>
        /// <param name="_CurrentFreq">电流频率(Hz)：0=50;1=60;2=150;3=400;4=700;5=1K;6=2K;7=3K;</param>
        /// <param name="_InteruptType">触发模式：0=外部触发；1=内部触发</param>
        /// <param name="_LoadLine">剩余电流加载相线： 0=N； 1=L1；2=L2；3=L3</param>
        /// <param name="_OutCurrent">直流叠加电流值：0~15000</param>
        /// <param name="_DCAddMode">直流叠加模式：0=关闭；1=正向叠加；2=负向叠加；</param>
        /// <param name="_CurrentEnableTime">电流使能时间：10~60000 (ms)</param>
        /// <param name="_StartCurrent"></param>
        /// <param name="_EndCurrent"></param>
        /// <param name="_TestTime"></param>
        /// <param name="_CurrentNP"></param>
        /// <param name="rlngErrNum"></param>
        /// <param name="rstrErrDescr"></param>
        public virtual void Leakage_SetParameters(int _TestType,
            int _WaveType, int _CurrentFreq, int _InteruptType, int _LoadLine,
            int _OutCurrent, int _DCAddMode, int _DCAddCurrent, int _CurrentEnableTime,
            int _StartCurrent, int _EndCurrent, int _TestTime, int _CurrentNP) { }

        public virtual void Leakage_EnableVolatge(int _enable) { }

        public virtual void Leakage_EnableCurrent(int _enable) { }

        public virtual void Leakage_StartTest(int _TestType, int _SnapTime) { }
        #region 弃置
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="param">参数</param>
        public virtual void LeakageCurrent_SetParams(int address, int param) { }

        public virtual void QCLeakageCurrent_SetParams(int address, int param) { }

        public virtual void QCLeakageCurrent_SetMulParams(int address, byte[] param) { }


        public virtual void  QCSendData(byte[] buf) { }
		
        /// <summary>
        /// 回读数据
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="param">参数</param>
        public virtual string LeakageCurrent_ReadData(int address, int param) { return ""; }
        /// <summary>
        /// 漏电仪-设置设置模式
        /// </summary>
        /// <param name="model">0 = 手动模式     1 = 自动模式</param>
        public virtual void LeakageCurrent_SetModel(int model) { }

        /// <summary>
        /// 漏电仪-设置模式为直接输出模式
        /// <param name="AcRateCurrent">AC额定电流(直接电流)单位 mA</param>
        /// </summary>
        public virtual void LeakageCurrent_SetACRateCurrent(double AcRateCurrent) { }
        /// <summary>
        /// 漏电仪-设置AC_In倍数
        /// <param name="ACInRate">AC_In倍数</param>
        /// </summary>
        public virtual void LeakageCurrent_SetACInRate(double ACInRate) { }
        /// <summary>
        /// 漏电仪-设置模式为爬升模式
        /// </summary>
        /// <param name="startCurrent">起始电流(mA)</param>
        /// <param name="endCurrent">结束电流(mA)</param>
        /// <param name="climbTime">爬升时间(mS)</param>
        public virtual void LeakageCurrent_SetAcClimb(double startCurrent, double endCurrent, double climbTime) { }
        /// <summary>
        /// 漏电仪-设置AC电流使能时间
        /// </summary>
        /// <param name="enableTime">使能时间(mS)</param>
        public virtual void LeakageCurrent_SetAcEnableTime(double enableTime) { }
        /// <summary>
        /// 漏电仪-交流/脉动直流切换
        /// </summary>
        /// <param name="type">0-交流   1-直流</param>
        public virtual void LeakageCurrent_SetAcOrDC(int type) { }
        /// <summary>
        /// 漏电仪-设置电压使能
        /// </summary>
        /// <param name="type">0-断开   1-使能</param>
        public virtual void LeakageCurrent_SetVoltageEnable(int type) { }
        /// <summary>
        /// 漏电仪-设置电流使能
        /// </summary>
        /// <param name="type">0-断开   1-使能</param>
        public virtual void LeakageCurrent_SetCurrentEnable(int type) { }


        public virtual void LeakageCurrent_SetCurrentGear(int gear) { }
        //AC额定电流In
        public virtual void LeakageCurrent_SetAcRatedCurrentIn(double ratedCurrent) { }
        //AC-In倍数
        public virtual void LeakageCurrent_SetAcInRate(double rate) { }
        //AC动作电流上限
        public virtual void LeakageCurrent_SetAcActionCurrentMax(double actionMax) { }
        //AC动作电流下限
        public virtual void LeakageCurrent_SetAcActionCurrentMin(double actionMin) { }
        //AC动作时间上限
        public virtual void LeakageCurrent_SetAcActionTimeMax(double actionTimeMax) { }
        //AC动作时间下限
        public virtual void LeakageCurrent_SetAcActionTimeMin(double actionTimeMin) { }
        //AC电流上升率
        public virtual void LeakageCurrent_SetAcCurrentClimb(double climbRate) { }
        //AC不动作电流输出时间
        //DC额定电流In
        //DC-In倍数
        //DC动作电流上限
        //DC动作电流下限
        //DC动作时间上限
        //DC动作时间下限
        //DC电流上升率
        //DC不动作电流输出时间
        //交流/脉动直流切换
        //交流模式
        //脉动直流模式
        //角度
        #endregion

        /// <summary>
        /// 漏电仪- 读实时状态数据
        /// </summary>
        public virtual void LeakageCurrent_ReadState() { }


        #endregion
    }
}
