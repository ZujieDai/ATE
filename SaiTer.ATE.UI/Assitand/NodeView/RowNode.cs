using SaiTer.ATE.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SaiTer.ATE.UI.Assitand.NodeView
{
    /// <summary>
    /// 行节点
    /// </summary>
    public class RowNode : IComparable<RowNode>
    {
        #region ---Field   region---
        private List<ColumnNode> _LstColumns = null;
        private bool _Checked = true; //行选中否
        #endregion

        #region ---Property region---
        /// <summary>
        /// 列节点个数
        /// </summary>
        public int Count
        {
            get { return _LstColumns.Count; }
        }
        /// <summary>
        /// 第一个列节点值（枪编号）
        /// </summary>
        public int ChargerID
        {
            get { return GetChargerID(); }
        }
        private int GetChargerID()
        {
            string str = _LstColumns[0].Column.ToString();//行头文本  如：1号枪
            int id = Convert.ToInt32(Regex.Replace(str, @"[^0-9]+", ""));//行头字符串提取出枪号
            return id;
        }
        private DateTime _AddTime = DateTime.Now;
        /// <summary>
        /// 这一行添加的时间（用于排序）
        /// </summary>
        public DateTime AddTime
        {
            get { return _AddTime; }
            set { _AddTime = value; }
        }
        /// <summary>
        /// 指定行是否选中
        /// </summary>
        public bool Checked
        {
            get { return _Checked; }
            set { _Checked = value; }
        }

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index">列索引号</param>
        /// <returns>列节点</returns>
        public ColumnNode this[int index]
        {
            get { return _LstColumns[index]; }
        }
        #endregion

        #region ---Constructor---
        /// <summary>
        /// 构造函数
        /// </summary>
        public RowNode()
        {
            _LstColumns = new List<ColumnNode>();
        }
        #endregion

        #region ---Function region---
        /// <summary>
        /// 初始化列表
        /// </summary>
        /// <param name="chargerID">这一行数据对应的枪编号</param>
        /// <param name="colCount">列表大小</param>
        public void Init(int chargerID, int colCount)
        {
            for (int i = 0; i < colCount; i++)
            {
                ColumnNode _Column = new ColumnNode();
                _Column.Column = String.Empty;
                _Column.Visible = true;
                _LstColumns.Add(_Column);
            }
            _LstColumns[0].Column = chargerID.ToString() + LanguageManager.GetByKey("号枪");
        }

        /// <summary>
        /// 清空列表
        /// </summary>
        public void Clear()
        {
            _Checked = true;
            _LstColumns.Clear();
        }
        /// <summary>
        /// 按照枪ID和时间排序
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(RowNode other)
        {
            if (this.ChargerID != other.ChargerID)
            {
                return this.ChargerID.CompareTo(other.ChargerID);
            }
            if (this.AddTime != other.AddTime)
            {
                return this.AddTime.CompareTo(other.AddTime);
            }
            else return 0;

        }
        #endregion
    }
}
