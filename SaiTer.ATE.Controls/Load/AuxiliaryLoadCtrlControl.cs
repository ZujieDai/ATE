using SaiTer.ATE.EquipMent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    /// <summary>
    /// 辅源负载（程控板）
    /// </summary>
    public class AuxiliaryLoadCtrlControl : AuxiliaryLoadCtrlBase
    {
        private string[] ClassNames = new string[] { "emtAuxiliaryLoadCtrl" };

        /// <summary>
        /// 取消所有状态
        /// </summary>
        /// <param name="lstIDs"></param>
        public override void CancelAllState(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.CancelAllState();
                }
            }
        }

        /// <summary>
        /// 设置12V辅源过压
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void Set12VoltOver(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Set12VoltOver();
                }
            }
        }
        /// <summary>
        /// 设置24V辅源过压
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void Set24VoltOver(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Set24VoltOver();
                }
            }
        }

        /// <summary>
        /// 设置辅源短路
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public override void SetShortCircuite(List<int> lstIDs)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.SetShortCircuite();
                }
            }
        }

        /// <summary>
        /// 设置12V辅源电流参数(1-16A范围，步进1A)
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="current">电流值(1-16A范围，步进1A)</param>
        public override void Set12VCurrent(List<int> lstIDs, int current)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Set12VCurrent(current);
                }
            }
        }

        /// <summary>
        /// 设置24V辅源电流参数（2-14A范围，步进2A）
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="current">电流值（2-14A范围，步进2A）</param>
        public override void Set24VCurrent(List<int> lstIDs, int current)
        {
            foreach (KeyValuePair<int, EquipMentBase> item in DitControlEquipMent(ClassNames))
            {
                int[] id = lstIDs.Intersect(item.Value.EquipManageChargerId.ToArray()).ToArray();
                if (id.Length > 0)
                {
                    item.Value.Set24VCurrent(current);
                }
            }
        }
    }
}
