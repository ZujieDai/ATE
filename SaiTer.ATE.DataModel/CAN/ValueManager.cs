using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.CAN
{
    public  class ValueManager
    {
        public DateTime PreMsgCreateTime { get; set; } = DateTime.Parse("2000-01-01 12:00:00.000");
        /// <summary>
        /// 录波板的绝对时间ms
        /// </summary>
        public int ALTime_ms { get; set; } = -1;
        public void Reset()
        {
            PreMsgCreateTime = DateTime.Parse("2000-01-01 12:00:00.000");
            FirstCreateTime = DateTime.Parse("2000-01-01 00:00:00.000");
            ALTime_ms = -1;
        }
        public DateTime FirstCreateTime { get; set; } = DateTime.Parse("2000-01-01 00:00:00.000");
        public bool IsFirstMsg()//add for 实时时间
        {
            if (PreMsgCreateTime.Year == 2000)
                return true;
            else
                return false;
        }

        public bool EnableTranslate { get; set; }

    }
}
