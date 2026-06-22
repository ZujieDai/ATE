using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;
using SaiTer.ATE.DataModel.EnumModel;
using SaiTer.ATE.DataModel.Struct;
using SaiTer.ATE.IDAL.SQLiteIDAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Windows.Forms;

namespace SaiTer.ATE.IDAL
{
    public class ExportReport_Excel
    {
        static string strTitleDescripe = "测试描述";
        static string strTitleStandard = "技术要求";
        static string strUserParams = "用户设置参数";
        static string strTrialCondition = "测试条件记录";
        private static List<StTrialItem> lstTrialItems = new List<StTrialItem>();
        private static List<TrialDataModel> _LstTrialData = new List<TrialDataModel>();
        private static List<ChargerInfoModel> LstChargerInfo = new List<ChargerInfoModel>();

        public static List<TrialDataModel> LstTrialData
        {
            get { return _LstTrialData; }
            set
            {
                _LstTrialData = value;
                DicTrialTypeData = ParseData(_LstTrialData);
            }
        }
        /// <summary>
        /// (试验项枚举值，该项的检测数据>)
        /// </summary>
        static Dictionary<int, List<TrialDataModel>> DicTrialTypeData = new Dictionary<int, List<TrialDataModel>>();
        private static Dictionary<int, List<TrialDataModel>> ParseData(List<TrialDataModel> lstTrialData)
        {
            Dictionary<int, List<TrialDataModel>> dicTemp = new Dictionary<int, List<TrialDataModel>>();

            foreach (var item in lstTrialData)
            {
                if (!dicTemp.Keys.Contains((int)item.TrialType))
                {
                    dicTemp.Add((int)item.TrialType, new List<TrialDataModel>());
                }
                dicTemp[(int)item.TrialType].Add(item);
            }
            return dicTemp;
        }

        public static ICellStyle GetStyle1(IWorkbook wkBook)
        {
            ICellStyle style = wkBook.CreateCellStyle();
            style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            style.Alignment=NPOI.SS.UserModel.HorizontalAlignment.Center;

            return style; 
        }

        public static bool CreatDemo(ref string FilePath)
        {
            try
            {
                string GetNowLongData = System.DateTime.Now.ToString("yyyy-MM-dd");
                string path = System.AppDomain.CurrentDomain.BaseDirectory + "报表(勿删)\\Word" + "\\" + GetNowLongData;
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                FilePath = path + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                //1、创建工作簿对象
                IWorkbook wkBook = new XSSFWorkbook();
                //2、在该工作簿中创建工作表对象
                ISheet sheet = wkBook.CreateSheet("人员信息"); //Excel工作表的名称
                List<Person> list = new List<Person>() {
                  new Person(){Name="张三",Age="15",Email="123@qq.com" },
                  new Person(){Name="李四",Age="16",Email="456@qq.com" },
                  new Person(){Name="王五",Age="17",Email="789@qq.com" }};

                //2.1向工作表中插入行与单元格
                for (int i = 0; i < list.Count; i++)
                {
                    //在Sheet中插入创建一行
                    IRow row = sheet.CreateRow(i);
                    //在该行中创建单元格
                    //方式一
                    ICell cell = row.CreateCell(0);
                    cell.SetCellValue(list[i].Name);
                    cell.CellStyle = GetStyle1(wkBook);

                    sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(i, i, 1, 4));//合并单元格
                    cell = row.CreateCell(1);
                    cell.SetCellValue(list[i].Age);
                    cell.CellStyle = GetStyle1(wkBook);

                    cell = row.CreateCell(2);
                    cell.SetCellValue(list[i].Email);
                    cell.CellStyle = GetStyle1(wkBook);
                    //方式二
                    //row.CreateCell(0).SetCellValue(list[i].Name); //给单元格设置值：第一个参数(第几个单元格)；第二个参数(给当前单元格赋值)
                    //row.CreateCell(1).SetCellValue(list[i].Age);
                    //row.CreateCell(2).SetCellValue(list[i].Email);
                }
                //3、写入，把内存中的workBook对象写入到磁盘上
                FileStream fsWrite = new FileStream(FilePath, FileMode.Create);
                wkBook.Write(fsWrite, true);
                MessageBox.Show("写入成功！", "提示");
                fsWrite.Close(); //关闭文件流
                wkBook.Close();  //关闭工作簿
                fsWrite.Dispose(); //释放文件流

                return true;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return false;
            }
        }

        public static bool CreateFile(List<TrialDataModel> lstTrialDatas, ChargerInfoModel chargerInfos, List<string> lstSchemeName, ref string FilePath)
        {
            try
            {
                string strSchemeNames = "";
                for (int i = 0; i < lstSchemeName.Count; i++)
                {
                    strSchemeNames += "'" + lstSchemeName[i] + "',";
                }
                strSchemeNames = strSchemeNames.TrimStart('\'').TrimEnd(new char[] { ',', '\'' });
                lstTrialItems.Clear();
                TrialItemsManage.GetTrialSchemeFromSchemeName(strSchemeNames, ref lstTrialItems);
                LstTrialData = lstTrialDatas;

                //指定Word文档的路径和名称

                // string path = "docx\\demo.docx";
                string Pkid = lstTrialDatas[0].PKID.ToString();
                string GetNowLongData = System.DateTime.Now.ToString("yyyy-MM-dd");
                string path = System.AppDomain.CurrentDomain.BaseDirectory + "报表(勿删)\\Word" + "\\" + GetNowLongData;
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                FilePath = path + "\\" + Pkid + strSchemeNames + ".docx";

                //XWPFDocument doc = CreateHead(chargerInfos);

                //for (int k = 0; k < doc.Document.body.GetTblArray().Count(); k++)
                //{
                //    CT_Tbl m_CTTbl = doc.Document.body.GetTblArray()[k];
                //    m_CTTbl.AddNewTblPr().jc = new CT_Jc();
                //    m_CTTbl.AddNewTblPr().jc.val = ST_Jc.center;//表在页面水平居中
                //}
                //FileStream out1 = new FileStream(FilePath, FileMode.Create);
                //doc.Write(out1);
                //out1.Close();

                return true;
            }
            catch (Exception ex)
            {
                Log.Log.LogException(ex);
                return false;
            }
        }



    }
    public class Person
    {
        public string Name { get; set; }
        public string Age { get; set; }
        public string Email { get; set; }
    }

}
