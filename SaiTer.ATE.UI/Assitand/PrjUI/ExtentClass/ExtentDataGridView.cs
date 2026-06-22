using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SaiTer.ATE.UI.Assitand.PrjUI.ExtentClass;

namespace SaiTer.ATE.UI.PrjUI
{
    public partial class ExtentDataGridView : DataGridView
    { /// <summary>
      /// Constructor
      /// </summary>
        public ExtentDataGridView()
            : base()
        {
        }

        /// <summary>
        /// Gets or sets the header cell located in the upper left corner of the DataGridView control. 
        /// </summary>
        [Browsable(false)]
        public new DataGridViewCheckBoxTopLeftHeader TopLeftHeaderCell
        {
            get
            {
                return base.TopLeftHeaderCell as DataGridViewCheckBoxTopLeftHeader;
            }
            set
            {
                base.TopLeftHeaderCell = value;
            }
        }
        [Browsable(false)]
        public DataGridViewHeaderCell _RowHeaderCell;
        public new DataGridViewCheckBoxRowHeaderCell RowHeaderCell
        {
            get
            {
                return _RowHeaderCell as DataGridViewCheckBoxRowHeaderCell;
            }
            set
            {
                _RowHeaderCell = value;
            }
        }
        /// <summary>
        /// Gets or sets the value associated with this cell.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(null)]
        [Description("Gets or sets the value associated with this cell. ")]
        public string TopLeftHeaderCellText
        {
            get
            {
                if (base.TopLeftHeaderCell.Value == null)
                    return string.Empty;
                return base.TopLeftHeaderCell.Value.ToString();
            }
            set
            {
                base.TopLeftHeaderCell.Value = value;
            }
        }

        /// <summary>
        /// Occurs after a new row is added to the ExtendedDataGridView. 
        /// </summary>
        /// <param name="e">
        /// A DataGridViewRowsAddedEventArgs that contains the event data. 
        /// </param>
        protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
        {
            //this.Rows[e.RowIndex].DefaultHeaderCellType = typeof(DataGridViewCheckBoxRowHeaderCell);
            base.OnRowsAdded(e);
        }
    }
}
