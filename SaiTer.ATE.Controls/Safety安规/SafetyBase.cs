using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;


namespace SaiTer.ATE.Controls
{
    public abstract class SafetyBase : ControlsBase
    {
        /// <summary>
        /// 初始化安规测试方案
        /// </summary>
        /// <param name="lstIDs">充电枪ID集合</param>
        public virtual bool SafetyInit(List<int> lstIDs, string SchemeID, string SchemeName, bool isSave = false) { return false; }

        /// <summary>
        /// 停止安规测试
        /// </summary>
        /// <param name="lstIDs">充电枪ID集合</param>
        /// 
        public virtual void SafetyOFF(List<int> lstIDs) { }

        /// <summary>
        /// 设置安规参数
        /// </summary>
        /// <param name="lstIDs">充电枪ID集合</param>
        public virtual void SafetySetParam(List<int> lstIDs, string AsciiSendStr, string AsciiOutEndFlag = "\r\n", string AsciiInEndFlag = "\r\n", int SleepTime = 100) { }


        /// <summary>
        /// 读取安规信息
        /// </summary>
        /// <param name="lstIDs">充电枪ID集合</param>
        public virtual bool SafetyReadParam(List<int> lstIDs, string AsciiSendStr, string AsciiOutEndFlag = "\r\n", string AsciiInEndFlag = "\r\n") { return true; }

        /// <summary>
        /// 读一个桩的安规测试数据
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="AsciiSendStr"></param>
        /// <param name="AsciiOutEndFlag"></param>
        /// <param name="AsciiInEndFlag"></param>
        /// <returns></returns>
        public virtual string SafetyReadResult(List<int> lstIDs, string AsciiSendStr, string AsciiOutEndFlag = "\r\n", string AsciiInEndFlag = "\r\n") { return ""; }

        public virtual void PauseReadSafetyStateData(List<int> lstIDs, bool IsPause) { }
    }
}
