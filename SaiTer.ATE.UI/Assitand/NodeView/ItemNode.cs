using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.UI.Assitand.NodeView
{
    /// <summary>
    /// 方案节点
    /// </summary>
    public class ItemNode
    {
        #region ---Field region---
        private List<RowNode> _LstRows = null;
        private int _ItemID = -1;
        #endregion

        #region ---Constructor---
        /// <summary>
        /// 构造函数
        /// </summary>
        public ItemNode()
        {
            _LstRows = new List<RowNode>();
        }
        #endregion

        #region ---Property region---
        /// <summary>
        /// 行节点个数
        /// </summary>
        public int Count
        {
            get { return _LstRows.Count; }
        }

        /// <summary>
        /// 检定项序号
        /// </summary>
        public int ItemID
        {
            get { return _ItemID; }
            set { _ItemID = value; }
        }

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index">行索引号</param>
        /// <returns>行节点</returns>
        public RowNode this[int index]
        {
            get { return _LstRows[index]; }
        }
        #endregion

        #region ---Function region---
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="rowCount">行节点个数</param>
        /// <param name="colCount">列节点个数</param>
        public void Init(int rowCount, int colCount)
        {
            for (int i = 0; i < rowCount; i++)
            {
                RowNode _Row = new RowNode();
                _Row.Checked = true;
                _Row.Init(i + 1, colCount);
                _LstRows.Add(_Row);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="rowCount">行节点个数</param>
        /// <param name="colCount">列节点个数</param>
        public void Init(List<int> lsrChargerID, int colCount)
        {
            try
            {
                for (int i = 0; i < lsrChargerID.Count; i++)
                {
                    RowNode _Row = new RowNode();
                    _Row.Checked = true;
                    _Row.Init(lsrChargerID[i], colCount);
                    _Row.AddTime = DateTime.Now;
                    _LstRows.Add(_Row);
                }
                _LstRows.Sort();
            }
            catch (Exception ex)
            { }
        }

        /// <summary>
        /// 增加一行
        /// </summary>

        public void AddRow(int chargerID)
        {
            RowNode _Row = new RowNode();
            _Row.Checked = true;
            _Row.Init(chargerID, 18);
            _Row.AddTime = DateTime.Now;
            _LstRows.Add(_Row);

        }
        /// <summary>
        /// 清空列表
        /// </summary>
        public void Clear()
        {
            //_ItemID = -1;
            _LstRows.Clear();
        }
        /// <summary>
        /// 移除一行数据
        /// </summary>
        /// <param name="index">行号索引</param>
        public void RemoveAt(int index)
        {
            _LstRows.RemoveAt(index);
        }


        #endregion
    }
}
