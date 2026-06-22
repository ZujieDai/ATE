using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    public class PowerAnalyzerBase : ControlsBase
    {
        /// <summary>
        /// 启动输出4通道积分
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void IntegralStart(List<int> lstIDs) { }

        /// <summary>
        /// 停止输出4通道积分
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void IntegralStop(List<int> lstIDs) { }

        /// <summary>
        /// 清零输出4通道积分
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void IntegralClear(List<int> lstIDs) { }

        /// <summary>
        /// 读输出4通道积分值
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual  double ReadIntegralValue(List<int> lstIDs) { return 0; }

        /// <summary>
        /// 读电流谐波含量（%）
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual double ReadCurrentHarmonicValue(List<int> lstIDs) { return 0; }

        public virtual void Integral123Start(List<int> lstIDs,int iState) { }
        /// <summary>
        /// 设置通道变比
        /// </summary>
        /// <param name="iCH">通道</param>
        /// <param name="isU">是否电压</param>
        /// <param name="iRatio">变比</param>
        public virtual void SetChannelRatio(List<int> lstIDs,int iCH, bool isU, int iRatio) { }


        /// <summary>
        /// 设置缩放功能开关
        /// </summary>
        /// <param name="state">关闭=0，打开=1</param>
        public virtual void SetScalingState(List<int> lstIDs, int state) { }

        /// <summary>
        /// 读取直流分量电压（V）
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="iCH">通道</param>
        /// <returns></returns>
        public virtual double ReadDcComponentVoltage(List<int> lstIDs, int iCH) { return 0; }
        /// <summary>
        /// 读取直流分量电流（A）
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="iCH">通道</param>
        /// <returns></returns>
        public virtual double ReadDcComponentCurrent(List<int> lstIDs, int iCH) { return 0; }
        /// <summary>
        /// 读取频率
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="iCH">通道</param>
        /// <returns></returns>
        public virtual double ReadFreq(List<int> lstIDs, int iCH) { return 0; }

        /// <summary>
        /// 读取50次电流谐波含量（%）
        /// </summary>
        /// <param name="iCH"></param>
        /// <returns></returns>
        public virtual List<double> ReadCurrentHarmonicValue_50(List<int> lstIDs, int iCH) { return new List<double>(); }

        /// <summary>
        /// 读取50次电压谐波含量（%）
        /// </summary>
        /// <param name="iCH"></param>
        /// <returns></returns>
        public virtual List<double> ReadVoltageHarmonicValue_50(List<int> lstIDs, int iCH) { return new List<double>(); }
        /// <summary>
        /// 打开谐波开关
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="iCH">通道</param>
        /// <param name="isON">是否打开</param>
        public virtual void SetHarmonicState(List<int> lstIDs, int iCH, bool isON) { }

        /// <summary>
        /// 读取电压谐波总失真度（%）
        /// </summary>
        /// <param name="iCH"></param>
        /// <returns></returns>
        public virtual double ReadVoltageHarmonic_Total(List<int> lstIDs, int iCH) { return 0; }
        /// <summary>
        /// 读取电流谐波总失真度（%）
        /// </summary>
        /// <param name="iCH"></param>
        /// <returns></returns>
        public virtual double ReadCurrentHarmonic_Total(List<int> lstIDs, int iCH) { return 0; }
    }
}
