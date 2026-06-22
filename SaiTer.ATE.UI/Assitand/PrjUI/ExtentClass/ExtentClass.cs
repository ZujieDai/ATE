using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace SaiTer.ATE.UI.Assitand.PrjUI.ExtentClass
{
    /// <summary>
    /// Represents the method that will handle row-related events of a DataGridViewCheckBoxRowHeaderCell. 
    /// </summary>
    /// <param name="sender">
    /// The source of the event. 
    /// </param>
    /// <param name="e">
    /// A DataGridViewCheckBoxRowHeaderEventArgs that contains the event data. 
    /// </param>
    public delegate void DataGridViewCheckBoxTopLeftHeaderEventHander(object sender,
                    DataGridViewCheckBoxTopLeftHeaderEventArgs e);

    /// <summary>
    /// Provides data for row-related System.Windows.Forms.DataGridView events. 
    /// </summary>
    public class DataGridViewCheckBoxTopLeftHeaderEventArgs : EventArgs
    {
        private bool _Checked = false;
        /// <summary>
        /// Gets or sets a value indicating whether the DataGridViewCheckBoxRowHeaderCell
        /// is in checked state. 
        /// </summary>
        public bool Checked
        {
            get { return _Checked; }
            set { _Checked = value; }
        }
    }

    /// <summary>
    /// Represents the checkBox cell in the top left corner of the DataGridView 
    /// that sits above the row headers and to the left of the column headers. 
    /// </summary>
    public class DataGridViewCheckBoxTopLeftHeader : DataGridViewTopLeftHeaderCell
    {
        #region ---Field region---
        private Point checkBoxLocation;
        private Size checkBoxSize;
        private bool _checked = false;
        private Point _cellLocation = new Point();
        private CheckBoxState _cbState = CheckBoxState.UncheckedNormal;
        private CheckState checkState = CheckState.Checked;
        #endregion

        #region ---Property region---
        /// <summary>
        /// Gets or sets a value indicating whether the DataGridViewCheckBoxRowHeaderCell
        /// is in checked state. 
        /// </summary>
        public bool Checked
        {
            get { return _checked; }
            set { _checked = value; }
        }

        /// <summary>
        /// Gets the state of the CheckBoxTopLeftHeaderCell.
        /// </summary>
        public CheckState CheckState
        {
            get
            {
                return checkState;
            }
        }
        #endregion

        #region ---Event region---
        /// <summary>
        /// Occurs when the value of the Checked property changes between posts to the server. 
        /// </summary>
        public event DataGridViewCheckBoxTopLeftHeaderEventHander OnCheckBoxClicked;
        #endregion

        #region ---Event Method---
        /// <summary>
        /// Ovveride Painting the current DataGridViewCheckBoxRowHeader. 
        /// </summary>
        /// <param name="graphics">The Graphics used to paint the DataGridViewCell.</param>
        /// <param name="clipBounds">
        /// A Rectangle that represents the area of the DataGridView that needs to be repainted.
        /// </param>
        /// <param name="cellBounds">
        /// A Rectangle that contains the bounds of the DataGridViewCell that is being painted.
        /// </param>
        /// <param name="rowIndex">The row index of the cell that is being painted.</param>
        /// <param name="cellState">
        /// A bitwise combination of DataGridViewElementStates values that specifies the state of the cell.
        /// </param>
        /// <param name="value">The data of the DataGridViewCell that is being painted.</param>
        /// <param name="formattedValue">
        /// The formatted data of the DataGridViewCell that is being painted.
        /// </param>
        /// <param name="errorText">An error message that is associated with the cell.</param>
        /// <param name="cellStyle">
        /// A DataGridViewCellStyle that contains formatting and style information about the cell.
        /// </param>
        /// <param name="advancedBorderStyle">
        /// A DataGridViewAdvancedBorderStyle that contains border styles for the cell that is being painted.
        /// </param>
        /// <param name="paintParts">
        /// A bitwise combination of the DataGridViewPaintParts values 
        /// that specifies which parts of the cell need to be painted.
        /// </param>
        protected override void Paint(
            Graphics graphics,
            Rectangle clipBounds,
            Rectangle cellBounds,
            int rowIndex,
            DataGridViewElementStates cellState,
            object value,
            object formattedValue,
            string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics,
                clipBounds,
                cellBounds,
                rowIndex,
                cellState,
                value,
                formattedValue,
                errorText,
                cellStyle,
                advancedBorderStyle,
                paintParts);

            Point p = new Point();
            Size s = CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxState.UncheckedNormal);
            p.X = cellBounds.Location.X + 5;//the x-coordinate of a check box header
            p.Y = cellBounds.Location.Y + 4;//the y-coordinate of a check box header
            _cellLocation = cellBounds.Location;
            checkBoxLocation = p;
            checkBoxSize = s;
            if (_checked)
                _cbState = CheckBoxState.CheckedNormal;
            else
                _cbState = CheckBoxState.UncheckedNormal;
            CheckBoxRenderer.DrawCheckBox(graphics, checkBoxLocation, _cbState);
        }

        /// <summary>
        /// Called when the user clicks a mouse button 
        /// while the pointer is on a row header cell. (override)
        /// </summary>
        /// <param name="e">
        /// A DataGridViewCellMouseEventArgs that contains the event data. 
        /// </param>
        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            Point p = new Point(e.X + _cellLocation.X, e.Y + _cellLocation.Y);
            if (p.X >= checkBoxLocation.X && p.X <= checkBoxLocation.X + checkBoxSize.Width
            && p.Y >= checkBoxLocation.Y && p.Y <= checkBoxLocation.Y + checkBoxSize.Height)
            {
                _checked = !_checked;
                //获取行头checkbox的选择状态
                DataGridViewCheckBoxTopLeftHeaderEventArgs ex =
                    new DataGridViewCheckBoxTopLeftHeaderEventArgs();
                ex.Checked = _checked;
                //此处不代表选择的行头checkbox，只是作为参数传递。
                //该行头checkbox是绘制出来的，无法获得它的实例
                object sender = new object();
                if (ex.Checked)
                    checkState = CheckState.Checked;
                else
                    checkState = CheckState.Unchecked;
                if (OnCheckBoxClicked != null)
                {
                    OnCheckBoxClicked(sender, ex);
                    DataGridView.InvalidateCell(this);
                }
            }
            base.OnMouseClick(e);
        }
        #endregion
    }
}
