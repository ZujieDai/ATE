using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.Struct
{
    #region =========== 树节点标签结构体 ===========
    /// <summary>
    /// 树节点标签结构
    /// </summary>
    public struct StTreeNodeTag
    {
        /// <summary>
        /// 检定项ID
        /// </summary>
        public int ItemId;
        /// <summary>
        /// 检定项排序序号
        /// </summary>
        public int ItemIndex;
        /// <summary>
        /// 检定项详情
        /// </summary>
        public string ItemDetail;
        /// <summary>
        /// 检定子项目ID
        /// </summary>
        public int SubItemId;
    }
    #endregion
}
