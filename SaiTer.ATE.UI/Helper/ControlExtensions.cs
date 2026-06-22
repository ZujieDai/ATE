using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaiTer.ATE.UI.Helper
{
    public static class ControlExtensions
    {
        /// <summary>
        /// 在控件树中根据 Name 查找控件（递归）。
        /// </summary>
        public static Control FindControlRecursive(this Control parent, string name)
        {
            if (parent == null || string.IsNullOrEmpty(name)) return null;
            if (parent.Name == name) return parent;
            // Controls.Find 可递归，优先使用它
            var found = parent.Controls.Find(name, true);
            if (found != null && found.Length > 0) return found[0];
            // 兜底递归（通常不需要）
            foreach (Control c in parent.Controls)
            {
                var r = c.FindControlRecursive(name);
                if (r != null) return r;
            }
            return null;
        }

        /// <summary>
        /// 尝试通过控件名设置控件的属性（支持跨线程）。返回是否成功。
        /// propertyName：比如 "Text"、"Visible"、"Enabled" 等。
        /// value：可以是目标类型或字符串形式（会尝试转换）。
        /// </summary>
        public static bool TrySetControlProperty(this Control parent, string controlName, string propertyName, object value)
        {
            if (parent == null || string.IsNullOrEmpty(controlName) || string.IsNullOrEmpty(propertyName))
                return false;

            Control ctl = parent.FindControlRecursive(controlName);
            if (ctl == null) return false;

            PropertyInfo pi = ctl.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (pi == null || !pi.CanWrite) return false;

            object converted = null;
            Type propType = pi.PropertyType;

            try
            {
                if (value == null)
                {
                    converted = null;
                }
                else if (propType.IsInstanceOfType(value))
                {
                    converted = value;
                }
                else if (propType.IsEnum)
                {
                    // 支持枚举字符串或整数
                    if (value is string)
                        converted = Enum.Parse(propType, value as string);
                    else
                        converted = Enum.ToObject(propType, value);
                }
                else
                {
                    TypeConverter tc = TypeDescriptor.GetConverter(propType);
                    if (tc != null && tc.CanConvertFrom(value.GetType()))
                    {
                        converted = tc.ConvertFrom(value);
                    }
                    else if (value is string)
                    {
                        converted = tc.ConvertFromInvariantString((string)value);
                    }
                    else
                    {
                        converted = Convert.ChangeType(value, propType);
                    }
                }
            }
            catch
            {
                // 转换失败
                return false;
            }

            Action setAction = () => pi.SetValue(ctl, converted);
            if (ctl.InvokeRequired)
            {
                try
                {
                    ctl.BeginInvoke(setAction);
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                setAction();
            }

            return true;
        }
    }
}
