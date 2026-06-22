using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.DataBaseModel
{
    /// <summary>
    /// KS查找指定充电枪试验数据
    /// </summary>
    /// <returns></returns>
    public class TemplateRuleModel
    {
        public string ItamName { get; set; }
        public string DataContent { get; set; }
        public string DataType { get; set; }
        public string Bookmark { get; set; }
        public string TrialType { get; set; }
        public string TrialName { get; set; }
        public string Other { get; set; }
    }
}
