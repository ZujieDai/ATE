using SaiTer.ATE.DataModel;
using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    public class FeedbackLoadControl : FeedbackLoadBase
    {
        private string[] ClassNames = new string[] { "emtFeedbackLoad", "emtFeedbackLoad_YKR", "emtLoopFeedbackLoad", "emtFeedbackLoad_SZHY", "emtFeedbackLoad_DC_ST" };

        /// <summary>
        /// 模拟BMS启动充电
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void FeedbackLoad_BMSON(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    //ThreadPool.QueueUserWorkItem(state =>
                    //{
                    item.Value.FeedbackLoad_BMSON();
                    //});
                }
                //item.Value.FeedbackLoad_ON();
            }

        }
        /// <summary>
        /// 模拟BMS结束充电
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void FeedbackLoad_BMSOFF(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.FeedbackLoad_BMSOFF();
                }
                //item.Value.FeedbackLoad_OFF();

            }

        }


        /// <summary>
        /// 启动负载输出
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void FeedbackLoad_ON(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    //ThreadPool.QueueUserWorkItem(state =>
                    //{
                        item.Value.FeedbackLoad_ON();
                    //});
                }
                //item.Value.FeedbackLoad_ON();
            }

        }
        /// <summary>
        /// 关闭负载
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void FeedbackLoad_OFF(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.FeedbackLoad_OFF();
                }
                //item.Value.FeedbackLoad_OFF();

            }

        }

        /// <summary>
        /// 并机
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void FeedbackLoad_Parallel(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.FeedbackLoad_Parallel();
                }
                //item.Value.FeedbackLoad_Parallel();

            }

        }

        /// <summary>
        /// 取消并机
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void FeedbackLoad_NoParallel(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                //item.Value.FeedbackLoad_NoParallel();

                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.FeedbackLoad_NoParallel();
                }
            }

        }
        /// <summary>
        /// 设置负载需求电压
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="voltage">需求电压</param>
        public override void SetFeedbackLoadParams(List<int> lstIDs, double voltage, double current)
        {

            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                //item.Value.SetFeedbackLoadParams(voltage, current);

                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {

                    //ThreadPool.QueueUserWorkItem(state =>
                    //{
                        item.Value.SetFeedbackLoadParams(voltage, current);
                    //});
                }
            }

        }

        /// <summary>
        /// BMS启动（只有深圳HY的项目有这个设置）
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void FeedbackLoad_BMS_ON(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                //item.Value.FeedbackLoad_NoParallel();

                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.BMS_ON();
                }
            }

        }
    }
}
