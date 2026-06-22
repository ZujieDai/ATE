using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.Struct
{
    /// <summary>
    /// 方案总表信息结构
    /// </summary>
    [Serializable]
    public struct StSchemeInfo
    {
        public int SchemeID
        {
            get; set;
        }
        public string SchemeName { get; set; }

        public string Remarks { get; set; }

        public string CreatTime { get; set; }

        public string RES1 { get; set; }

        public string RES2 { get; set; }

        public string RES3 { get; set; }
    }
}
