using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    public static class EnumHelper
    {
        /// <summary>
        /// 得到枚举的DescriptionAttribute值。
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        static public string GetEnumDescription<TEnum>(object value)
        { 
            Type enumType = typeof(TEnum);
            if (!enumType.IsEnum)
                throw new ArgumentException("不是枚举类型");
            var name = Enum.GetName(enumType, value);
            if (name == null)
                return string.Empty;
            object[] objs = enumType.GetField(name).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objs == null || objs.Length == 0)
                return string.Empty;
            DescriptionAttribute attr = objs[0] as DescriptionAttribute;
            return attr.Description;
        }


        /// <summary>
        /// 根据 Description 的值获取枚举值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="description"></param>
        /// <returns></returns>
        public static T GetEnumByDescription<T>(string description) where T : Enum
        {
            System.Reflection.FieldInfo[] fields = typeof(T).GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false); //获取描述属性
                if (objs.Length > 0 && (objs[0] as DescriptionAttribute).Description == description)
                {
                    return (T)field.GetValue(null);
                }
            }
            return default(T);
        }
    }

    public class DescriptionAttribute : Attribute
    {
        private string _description;

        public DescriptionAttribute(string desc)
        {
            _description = desc;
        }

        public string Description
        {
            get
            {
                return _description;
            }
        }
    }


}
