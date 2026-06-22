using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    public class FeedbackLoadACControl : FeedbackLoadBase
    {
        private string[] ClassNames = new string[] { "emtFeedbackLoad_AC"};
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
        /// 设置负载需求电压
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="voltage">需求电压</param>
        public override void SetFeedbackLoadParams(List<int> lstIDs, double voltage, double current)
        {

            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {

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




    }
}
