using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备基类 - 功率分析仪相关函数
    /// </summary>
    public abstract partial class EquipMentBase
    {
        /// <summary>
        /// 读实时状态数据
        /// </summary>
        public virtual void ReadPA6500_StateData() { }


        /// <summary>
        /// 启动输出4通道积分
        /// </summary>
        public virtual void IntegralStart() { }

        /// <summary>
        /// 停止输出4通道积分
        /// </summary>
        public virtual void IntegralStop() { }

        /// <summary>
        /// 清零输出4通道积分
        /// </summary>
        public virtual void IntegralClear() { }

        /// <summary>
        /// 读输出4通道积分值
        /// </summary>
        public virtual double ReadIntegralValue() { return 0; }

        /// <summary>
        /// 读取直流分量电压（V）
        /// </summary>
        /// <param name="iCH">通道</param>
        /// <returns></returns>
        public virtual double ReadDcComponentVoltage(int iCH) { return 0; }
        /// <summary>
        /// 读取直流分量电流（A）
        /// </summary>
        /// <param name="iCH">通道</param>
        /// <returns></returns>
        public virtual double ReadDcComponentCurrent(int iCH) { return 0; }

        /// <summary>
        /// 读电流谐波含量（%）
        /// </summary>
        public virtual double ReadCurrentHarmonicValue() { return 0; }
        /// <summary>
        /// 读取频率
        /// </summary>
        /// <param name="iCH"></param>
        /// <returns></returns>
        public virtual double ReadFreq(int iCH) { return 0; }

        /// <summary>
        /// 读取50次电流谐波含量（%）
        /// </summary>
        /// <param name="iCH"></param>
        /// <returns></returns>
        public virtual List<double> ReadCurrentHarmonicValue_50(int iCH) { return new List<double>(); }

        /// <summary>
        /// 读取50次电压谐波含量（%）
        /// </summary>
        /// <param name="iCH"></param>
        /// <returns></returns>
        public virtual List<double> ReadVoltageHarmonicValue_50(int iCH) { return new List<double>(); }

        public virtual void Integral123Start(int iState) { }
        /// <summary>
        /// 设置通道变比
        /// </summary>
        /// <param name="iCH">通道</param>
        /// <param name="isU">是否电压</param>
        /// <param name="iRatio">变比</param>
        public virtual void SetChannelRatio(int iCH,bool isU,int iRatio) { }

        /// <summary>
        /// 设置缩放功能开关
        /// </summary>
        /// <param name="iState"></param>
        public virtual void SetScalingState(int iState) {  }
        /// <summary>
        /// 打开谐波开关
        /// </summary>
        /// <param name="iCH">通道</param>
        /// <param name="isON">是否打开</param>
        public virtual void SetHarmonicState(int iCH, bool isON) { }

        /// <summary>
        /// 读取电压谐波总失真度（%）
        /// </summary>
        /// <param name="iCH"></param>
        /// <returns></returns>
        public virtual double ReadVoltageHarmonic_Total(int iCH) { return 0; }
        /// <summary>
        /// 读取电流谐波总失真度（%）
        /// </summary>
        /// <param name="iCH"></param>
        /// <returns></returns>
        public virtual double ReadCurrentHarmonic_Total(int iCH) { return 0; }
    }
}
