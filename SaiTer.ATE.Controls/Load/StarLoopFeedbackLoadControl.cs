using SaiTer.ATE.DataModel;
using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    public class StarLoopFeedbackLoadControl : LoopFeedbackLoadBase
    {
        private string[] ClassNames = new string[] { "emtStarLoopFeedbackLoad" };
        /// <summary>
        /// 启动负载输出
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void LoopFeedbackLoad_ON(List<int> lstIDs, int channel)
        {

            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                   
                        item.Value.LoopFeedbackLoad_ON(channel);
                    
                }
            }

        }
        /// <summary>
        /// 关闭负载
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void LoopFeedbackLoad_OFF(List<int> lstIDs, int channel)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.LoopFeedbackLoad_OFF(channel);
                }
            }

        }

        /// <summary>
        /// 并机
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void LoopFeedbackLoad_Parallel(List<int> lstIDs, int channel)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.LoopFeedbackLoad_Parallel(channel);
                }
            }

        }

        /// <summary>
        /// 取消并机
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void LoopFeedbackLoad_NoParallel(List<int> lstIDs, int channel)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.LoopFeedbackLoad_NoParallel(channel);
                }
            }

        }
        /// <summary>
        /// 设置负载需求电压
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="voltage">需求电压</param>
        public override void SetLoopFeedbackLoadParams(List<int> lstIDs, int channel, double voltage, double current)
        {

            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {

                    item.Value.SetLoopFeedbackLoadParams(channel, voltage, current);

                }
            }

        }



    }
}
