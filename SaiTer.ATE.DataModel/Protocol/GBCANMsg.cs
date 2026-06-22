using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.Protocol
{
    public  class GBCANMsg
    {

        #region CST报文
        public Msg CST { get; set; }
        public Msg CST_SPN3521 { get; set; }
        public Msg CST_SPN3521_1 { get; set; }
        public Msg CST_SPN3521_2 { get; set; }
        public Msg CST_SPN3521_3 { get; set; }
        public Msg CST_SPN3521_4 { get; set; }

        public Msg CST_SPN3522 { get; set; }
        public Msg CST_SPN3522_1 { get; set; }
        public Msg CST_SPN3522_2 { get; set; }
        public Msg CST_SPN3522_3 { get; set; }
        public Msg CST_SPN3522_4 { get; set; }
        public Msg CST_SPN3522_5 { get; set; }
        public Msg CST_SPN3522_6 { get; set; }

        public Msg CST_SPN3523 { get; set; }
        public Msg CST_SPN3523_1 { get; set; }
        public Msg CST_SPN3523_2 { get; set; }
        #endregion

        public GBCANMsg()
        {
            CST=new Msg();
            CST_SPN3521 = new Msg();
            CST_SPN3521_1 = new Msg();
            CST_SPN3521_2 = new Msg();
            CST_SPN3521_3 = new Msg();
            CST_SPN3521_4 = new Msg();
            CST_SPN3522 = new Msg();
            CST_SPN3522_1 = new Msg();
            CST_SPN3522_2 = new Msg();
            CST_SPN3522_3 = new Msg();
            CST_SPN3522_4 = new Msg();
            CST_SPN3522_5 = new Msg();
            CST_SPN3522_6 = new Msg();
            CST_SPN3523 = new Msg();
            CST_SPN3523_1 = new Msg();
            CST_SPN3523_2 = new Msg();
        }

    }

    public class Msg
    {
        public string MsgID { get; set; }
        public string MsgName { get; set; }
        public string MsgData { get; set; }

        public string MsgText { get; set; }
    }
}
