using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    public class UserInfoModel
    {
        public int PrimarykeyID { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Level { get; set; }
        public string UserType { get; set; }
        public string Remarks { get; set; }
        public string CreatTime { get; set; }
    }
}
