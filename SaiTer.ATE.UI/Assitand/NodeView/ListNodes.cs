using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.UI.Assitand.NodeView
{
    /// <summary>
    /// 检定节点列表
    /// </summary>
    public class ListNodes
    {
        #region ---Field region---
        private List<ItemNode> _LstItems = null;
        #endregion

        #region ---Property region---
        /// <summary>
        /// 检定项总数
        /// </summary>
        public int Count
        {
            get { return _LstItems.Count; }
        }

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index">检定项索引号</param>
        /// <returns>检定项</returns>
        public ItemNode this[int index]
        {
            get
            {
                if (index > -1 && index < _LstItems.Count)
                    return _LstItems[index];
                else
                    return _LstItems[0];
            }
        }
        #endregion

        #region ---Constructor---
        /// <summary>
        /// 节点列表
        /// </summary>
        public ListNodes()
        {
            _LstItems = new List<ItemNode>();
        }
        #endregion

        #region ---Function region---
        /// <summary>
        /// 加载一个检定点
        /// </summary>
        /// <param name="itemId">检定项ID</param>
        public void Add(int itemId)
        {
            ItemNode _Item = new ItemNode();
            _Item.ItemID = itemId;
            _LstItems.Add(_Item);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="itemCount"></param>
        public void Init(int itemCount)
        {
            for (int i = 0; i < itemCount; i++)
            {
                ItemNode _Item = new ItemNode();
                _Item.ItemID = i;
                _LstItems.Add(_Item);
            }
        }

        /// <summary>
        /// 查找是否包含检定项ID
        /// </summary>
        /// <param name="itemID">检定项ID</param>
        /// <returns></returns>
        public bool ContainsId(int itemID)
        {
            for (int i = 0; i < _LstItems.Count; i++)
            {
                if (_LstItems[i].ItemID == itemID)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 清空列表
        /// </summary>
        public void Clear()
        {
            _LstItems.Clear();
        }

        public void Sort()
        {
            
        }
        #endregion
    }
}
