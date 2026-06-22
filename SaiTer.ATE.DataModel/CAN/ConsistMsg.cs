using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.CAN
{
    public class ConsistMsg
    {
        public int ObjectNo { get; set; }
        public string MsgName { get; set; }
        public string CreateTimestamp { get; set; }
        public int Dlc { get; set; }
        public string SPN2560 { get; set; }
        public string SPN3901 { get; set; }
        public string SPN3902 { get; set; }
        public string SPN3903 { get; set; }
        public string SPN3904 { get; set; }
        public string SPN3905 { get; set; }
        public string SPN3906 { get; set; }
        public string SPN3907 { get; set; }
        public string SPN2829 { get; set; }
        public string SPN2830 { get; set; }


        public string Version { get; set; }
        public string SPN2601 { get; set; }
        public string SPN3921 { get; set; }
        public string SPN3922 { get; set; }
        public string SPN3923 { get; set; }
        public string SPN3924 { get; set; }
        public string SPN3925 { get; set; }
        public string SPN3926 { get; set; }
        public string SPN3927 { get; set; }
        public string SPN3928 { get; set; }
        public string SPN3090 { get; set; }
        public string SPN3091 { get; set; }
        public string SPN3092 { get; set; }
        public string SPN3093 { get; set; }
        public string SPN3094 { get; set; }
        public string SPN3095 { get; set; }
        public string SPN3096 { get; set; }
        public string SPN3929 { get; set; }
        public int MutiLength { get; set; }
        public int PackageId { get; set; }
        public int IsFirstPackage { get; set; }
        public int IsPackageReady { get; set; }
        public int IsPackageEnd { get; set; }
        public int TextId { get; set; }
        //public int Package
        public string MsgData { get; set; }
    }
}
