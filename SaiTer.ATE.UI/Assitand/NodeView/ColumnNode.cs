using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.UI.Assitand.NodeView
{
    /// <summary>
    /// 列节点
    /// </summary>
    public class ColumnNode
    {
        private object _Column = String.Empty;
        /// <summary>
        /// 列数据
        /// </summary>
        public object Column
        {
            get { return this._Column; }
            set { this._Column = value; }
        }

        private bool _Visible = true;
        /// <summary>
        /// 列可视化
        /// </summary>
        public bool Visible
        {
            get { return this._Visible; }
            set { this._Visible = value; }
        }
    }
}
