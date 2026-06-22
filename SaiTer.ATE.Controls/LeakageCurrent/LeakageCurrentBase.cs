using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{

    public abstract class LeakageCurrentBase : ControlsBase
    {
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="address">寄存器地址</param>
        /// <param name="param">参数</param>
        public virtual void LeakageCurrent_SetParams(List<int> lstIDs, int address, int param) { }


        public virtual void QCLeakageCurrent_SetParams(List<int> lstIDs, int address, int param) { }


        public virtual void QCLeakageCurrent_SendData(byte[] Sbuf) { }

        public virtual void QCLeakageCurrent_SetMulParams(List<int> lstIDs, int address, byte[] param) { }

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
        public virtual void Leakage_SetParameters(List<int> lstIDs, int _TestType,
            int _WaveType, int _CurrentFreq, int _InteruptType, int _LoadLine,
            int _OutCurrent, int _DCAddMode, int _DCAddCurrent, int _CurrentEnableTime,
            int _StartCurrent, int _EndCurrent, int _TestTime, int _CurrentNP)
        { }

        public virtual void Leakage_EnableVolatge(List<int> lstIDs, int _enable) { }

        public virtual void Leakage_EnableCurrent(List<int> lstIDs, int _enable) { }

        public virtual void Leakage_StartTest(List<int> lstIDs, int _TestType, int _SnapTime) { }

        /// <summary>
        /// 回读数据
        /// </summary>
        /// <param name="lstIDs">充电枪id信息集合</param>
        /// <param name="address">寄存器地址</param>
        /// <param name="param">参数</param>
        public virtual Dictionary<int, string> LeakageCurrent_ReadData(List<int> lstIDs, int address, int param) { return null; }
    }
}
