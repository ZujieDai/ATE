using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    /// <summary>
    /// 工作流参数
    /// </summary>
    public class TestWorkParam
    {
        /// <summary>
        /// 充电枪ID编号
        /// </summary>
        public  List<int> lstIDs;
 
        /// <summary>
        /// 参数1
        /// </summary>
        public object s1ParamObj;
        /// <summary>
        /// 参数2
        /// </summary>
        public object s2ParamObj;
        /// <summary>
        /// 参数3
        /// </summary>
        public object s3ParamObj;
        /// <summary>
        /// 参数4
        /// </summary>
        public object s4ParamObj;
        /// <summary>
        /// 参数5
        /// </summary>
        public object s5ParamObj;
        /// <summary>
        /// 参数6
        /// </summary>
        public object s6ParamObj;
     

        /// <summary>
        /// 参数
        /// </summary>
        public List<string> Lst1Param;
        public List<string> Lst2Param;
        public List<string> Lst3Param;
        public List<string> Lst4Param;
       

        /// <summary>
        /// 构造函数
        /// </summary>
        public TestWorkParam()
        {
            lstIDs = new List<int>();
        }
        /// <summary>
        /// 数据类型，普通数据0，二类参数2
        /// </summary>
        public int dataSort;
    }
}
