using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备基类  -  负载相关虚函数
    /// </summary>
    public partial class EquipMentBase
    {

        #region ---------------------电阻负载---------------------

        /// <summary>
        /// 读电阻负载实时状态数据
        /// </summary>
        public virtual void ReadResisLoad_State_AC() { }

        /// <summary>
        /// 读电阻负载实时状态数据
        /// </summary>
        public virtual void ReadResisLoad_State_DC() { }

        /// <summary>
        /// 启动负载
        /// </summary>
        public virtual void ResisLoad_ON() { }
        /// <summary>
        /// 关闭负载
        /// </summary>
        public virtual void ResisLoad_OFF() { }
        /// <summary>
        /// 设置负载需求电压电流
        /// </summary>
        /// <param name="volt">电压</param>
        /// <param name="curr">电流</param>
        public virtual void SetResisLoadVoltCurr(double volt, double curr) { }
        /// <summary>
        /// 设置负载需求电压电流
        /// </summary>
        /// <param name="volt">电压</param>
        /// <param name="curr">电流</param>
        /// <param name="Rate">电流倍率</param>
        public virtual void SetResisLoadVoltCurr(double volt, double curr, int Rate = 100) { }
        ///// <summary>
        ///// 设置负载需求电流
        ///// </summary>
        ///// <param name="curr"></param>
        //public virtual void SetResisLoadCurr(double curr) { }
        /// <summary>
        /// 设置负载需求功率
        /// </summary>
        /// <param name="volt">电压</param>
        /// <param name="power">功率</param>
        public virtual void SetResisLoadPower(double volt, double power) { }

        /// <summary>
        /// 多通道并机入1通道
        /// </summary>
        public virtual void ResisLoad_Parallel() { }

        /// <summary>
        /// 取消并机
        /// </summary>
        public virtual void ResisLoad_NoParallel() { }

        #endregion


        #region ---------------------电子负载---------------------
        public virtual void ReadElectronicLoad_StateData() { }
        /// <summary>
        /// 启动电子负载
        /// </summary>
        public virtual void ElectronicLoad_ON() { }
        /// <summary>
        /// 关闭电子负载
        /// </summary>
        public virtual void ElectronicLoad_OFF() { }

        /// <summary>
        /// 设置电子负载参数
        /// </summary>
        public virtual void SetElectronicLoadParams(byte tCom, UInt32 tOperate) { }

        /// <summary>
        /// 设置电子负载参数
        /// </summary>
        public virtual void SetElectronicLoadParams(byte tCom, byte tOperate) { }

        /// <summary>
        /// 读取电子负载参数
        /// </summary>
        public virtual UInt32 ReadElectronicLoadParams(byte tCom) { return 0; }

        #endregion



        #region ---------------------普通回馈负载---------------------
        /// <summary>
        /// 是否开启回馈负载电压差
        /// true:开启大电压  false:关闭
        /// </summary>
        public bool isOpenFeedBackLoadVoltDiff { get; set; } = false;

        public virtual void ReadFeedbackLoad_StateData() { }

        /// <summary>
        /// BMS信息写入回馈负载
        /// </summary>
        public virtual void WriteFeedbackLoad_BMSInfo() { }

        /// <summary>
        /// 模拟BMS启动充电
        /// </summary>
        public virtual void FeedbackLoad_BMSON() { }
        /// <summary>
        /// 模拟BMS结束充电
        /// </summary>
        public virtual void FeedbackLoad_BMSOFF() { }

        /// <summary>
        /// 启动回馈负载
        /// </summary>
        public virtual void FeedbackLoad_ON() { }
        /// <summary>
        /// 关闭电回馈负载
        /// </summary>
        public virtual void FeedbackLoad_OFF() { }
        /// <summary>
        /// 多通道并机入1通道
        /// </summary>
        public virtual void FeedbackLoad_Parallel() { }

        /// <summary>
        /// 取消并机
        /// </summary>
        public virtual void FeedbackLoad_NoParallel() { }

        /// <summary>
        /// 设置指定通道参数
        /// </summary>
        /// <param name="voltage">设定电压值,单位1mV</param>
        /// <param name="current">设定电流值,单位1mA</param>
        public virtual void SetFeedbackLoadParams(double voltage, double current) { }

        #endregion


        #region ---------------------手拉手环式回馈负载---------------------



        /// <summary>
        /// 启动回馈负载
        /// <param name="channel">通道号</param>
        /// </summary>
        public virtual void LoopFeedbackLoad_ON(int channel) { }
        /// <summary>
        /// 关闭电回馈负载
        /// <param name="channel">通道号</param>
        /// </summary>
        public virtual void LoopFeedbackLoad_OFF(int channel) { }
        /// <summary>
        /// 通道并机
        /// </summary>
        /// <param name="channel">通道号</param>
        public virtual void LoopFeedbackLoad_Parallel(int channel) { }

        /// <summary>
        /// 取消并机
        /// </summary>
        /// <param name="channel">通道号</param>
        public virtual void LoopFeedbackLoad_NoParallel(int channel) { }

        /// <summary>
        /// 设置指定通道参数
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <param name="voltage">设定电压值,单位1mV</param>
        /// <param name="current">设定电流值,单位1mA</param>
        public virtual void SetLoopFeedbackLoadParams(int channel, double voltage, double current) { }

        #endregion


        #region ---------------------程控板控制辅源负载---------------------
        public virtual void ReadAuxiliaryLoadCtrl_StateData() { }

        /// <summary>
        /// 取消所有状态
        /// </summary>
        public virtual void CancelAllState() { }
        /// <summary>
        /// 设置12V辅源过压
        /// </summary>
        public virtual void Set12VoltOver() { }
        /// <summary>
        /// 设置24V辅源过压
        /// </summary>
        public virtual void Set24VoltOver() { }

        /// <summary>
        /// 设置辅源短路
        /// </summary>
        public virtual void SetShortCircuite() { }

        /// <summary>
        /// 设置12V辅源电流参数(1-16A范围，步进1A)
        /// </summary>
        public virtual void Set12VCurrent(int current) { }

        /// <summary>
        /// 设置24V辅源电流参数（2-14A范围，步进2A）
        /// </summary>
        public virtual void Set24VCurrent(int current) { }

        #endregion
    }
}
