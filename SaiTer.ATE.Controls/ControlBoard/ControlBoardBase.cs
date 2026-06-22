using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    public abstract class ControlBoardBase : ControlsBase
    {
        /// <summary>
        /// 控制继电器状态
        /// </summary>
        /// <param name="lstConditionState">继电器状态集合（按照S1.S2.S3到.S16的顺序）</param>
        public virtual void ControlResistanceSetRelay( List<bool> lstConditionState) { }

        /// <summary>
        /// 控制继电器状态
        /// </summary>
        /// <param name="lstConditionState">继电器状态集合（按照S1.S2.S3到.S16的顺序）</param>
        public virtual void ControlResistanceSetRelay(List<int> lstIDs, List<bool> lstConditionState) { }

        /// <summary>
        /// 设置三色灯颜色
        /// </summary>
        /// <param name="color">颜色</param>
        public virtual void SetLightColor(EmLightColor color) { }
        /// <summary>
        /// 读继电器状态
        /// </summary>
        /// <returns></returns>
        public virtual List<bool> ControlBoardReadState() { return null; }
        /// <summary>
        /// 读继电器状态
        /// </summary>
        /// <returns></returns>
        public virtual List<bool> ControlBoardReadState(List<int> lstIDs) { return null; }
        /// <summary>
        /// 设置寄存器开关
        /// </summary>
        /// <param name="Register">寄存器地址</param>
        /// <param name="OnOff">通道控制</param>
        public virtual void SetRelaySwitch(uint Register, bool OnOff) { }
        /// <summary>
        /// 设置寄存器开关
        /// </summary>
        /// <param name="Register">寄存器地址</param>
        /// <param name="OnOff">通道控制</param>
        public virtual void SetRelaySwitch(List<int> lstIDs, uint Register, bool OnOff) { }
        /// <summary>
        /// 读取寄存器开关
        /// </summary>
        /// <param name="StratIndex">起始位置</param>
        /// <param name="RelayCount">寄存器数量</param>
        /// <returns></returns>
        public virtual List<bool> ReadRelaySwitch(int StratIndex, int RelayCount) { return null; }
    }
}
