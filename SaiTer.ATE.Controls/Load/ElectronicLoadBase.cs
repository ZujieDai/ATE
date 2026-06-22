using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    /// <summary>
    /// 电子负载控制类
    /// </summary>
    public class ElectronicLoadBase : ControlsBase
    {

        /// <summary>
        /// 启动电子负载
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void ElectronicLoad_ON(List<int> lstIDs) { }
        /// <summary>
        /// 关闭电子负载
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void ElectronicLoad_OFF(List<int> lstIDs) { }

        /// <summary>
        /// 设置电子负载参数
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void SetElectronicLoadParams(List<int> lstIDs, byte tCom, UInt32 tOperate) { }

        /// <summary>
        /// 设置电子负载参数
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void SetElectronicLoadParams(List<int> lstIDs, byte tCom, byte tOperate) { }

        /// <summary>
        /// 读取电子负载参数
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual Dictionary<int, UInt32> ReadElectronicLoadParams(List<int> lstIDs, byte tCom) { return null; }
    }
}
