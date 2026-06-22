
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.EquipMent;
using SaiTer.ATE.InterFace;
using SaiTer.ATE.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    /// <summary>
    /// 设备控制类基类
    /// </summary>
    public abstract class ControlsBase
    {
        /// <summary>
        /// 公共锁
        /// </summary>
        protected static object SynLock = new object();

        /// <summary>
        /// 待测充电枪信息集合
        /// </summary>
        public List<ChargerInfoModel> lstChargerInfo = null;

        /// <summary>
        /// 设备基类
        /// </summary>
        public Dictionary<int, EquipMentBase> DitEquipMentBase
        {
            get;
            set;
        }
        /// <summary>
        /// 查找对应设备
        /// </summary>
        /// <param name="ClassName"></param>
        /// <returns></returns>
        protected Dictionary<int, EquipMentBase> DitControlEquipMent(string ClassName)
        {
            Dictionary<int, EquipMentBase> temp = new Dictionary<int, EquipMentBase>();
            try
            {
                int[] keysArr = DitEquipMentBase.Where(s => s.Value.EquipMentClassName.Trim().ToLowerInvariant() == ClassName.Trim().ToLowerInvariant()).Select(s => s.Key).ToArray();
                for (int i = 0; i < keysArr.Length; i++)
                {
                    temp.Add(keysArr[i], DitEquipMentBase[keysArr[i]]);
                }
            }
            catch (Exception ex)
            {
                SendExMsg(ex);
            }
            return temp;
        }
        /// <summary>
        /// 查找对应设备
        /// </summary>
        /// <param name="ClassNames">多个设备集合</param>
        /// <returns></returns>
        protected Dictionary<int, EquipMentBase> DitControlEquipMent(string[] ClassNames)
        {
            Dictionary<int, EquipMentBase> temp = new Dictionary<int, EquipMentBase>();
            try
            {
                foreach (var item in ClassNames)
                {
                    int[] keysArr = DitEquipMentBase.Where(s => s.Value.EquipMentClassName.Trim().ToLowerInvariant() == item.Trim().ToLowerInvariant()).Select(s => s.Key).ToArray();
                    for (int i = 0; i < keysArr.Length; i++)
                    {
                        temp.Add(keysArr[i], DitEquipMentBase[keysArr[i]]);
                    }
                }

            }
            catch (Exception ex)
            {
                SendExMsg(ex);
            }
            return temp;
        }

        //protected int[] GetChargerID(List<int> lstIDs)
        //{
        //    //List<int> lstChargerID = new List<int>();

        //    //for (int i = 0; i < lstCharger.Count; i++)
        //    //{
        //    //    if (lstCharger[i].IsCheck)
        //    //    {
        //    //        lstChargerID.Add(lstCharger[i].ChargerId);
        //    //    }               
        //    //}           
        //    //return lstChargerID.ToArray(); ;
        //}
        /// <summary>
        /// 发送设备返回结果集
        /// </summary>
        /// <param name="Result">结果</param>
        protected void SendResultData(StResultData Result)
        {
            SystemEvent.SendEquipMentResult(Result);
        }
        /// <summary>
        /// 发送日志到文件
        /// </summary>
        /// <param name="Msg">日志信息</param>
        protected void SendMsgToFile(string Msg)
        {

        }
        /// <summary>
        /// 发送异常到文件
        /// </summary>
        /// <param name="ex">异常信息</param>
        protected void SendExMsg(Exception ex)
        {
            Log.Log.LogException(ex);
        }
        /// <summary>
        /// 开启线程
        /// </summary>
        /// <param name="CallBack">线程执行方法</param>
        /// <param name="s">参数对象</param>
        protected void StartThread(WaitCallback CallBack, object[] s = null)
        {
            ThreadPool.QueueUserWorkItem(CallBack, s);
        }
    }
}
