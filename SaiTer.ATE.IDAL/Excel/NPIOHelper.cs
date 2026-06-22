using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows;

namespace SaiTer.ATE.IDAL
{
    public class NpioOperation
    {
        public static NPIOHelper ExcelWR = new NPIOHelper();
        public static string ExcelSaveFileName = null;
        public static string ExcelSaveSheetName = null;
        public static UInt16 ExcelSaveSheetNameCount = 0;
        public static string ExcelOpenFileName = null;
        public static string ExcelOpenTempFileName = null;
        public static string ExcelOpenSheetName = null;

        public static bool ExcelOpenFlag = false;
        public static string[] ExcelOpenSheetNames = null;

        public static byte Cout = 0;
        public static UInt16 RowNum = 0;
        public static bool NPIOOpState;

        //  public static FileStream ExcelFileStream = null;
        public static HSSFWorkbook ExcelWorkBook = null;
        public static ISheet ExcelSheet = null;
        public static IRow ExcelRow = null;
        public static IFont ExcelFont = null;
        public static ICellStyle ExcelCellStyle = null;

        ///////

        public static string AlarmSetup_ExcelOpenSheetName = null;
        public static string AlarmSetup_ExcelSaveSheetName = null;
        public static string AlarmSetup_ExcelSaveFileName = null;
        public static UInt16 AlarmSetup_RowNum = 0;
        public static byte AlarmSetup_Cout = 0;
        public static FileStream AlarmSetup_ExcelFileStream = null;
        public static HSSFWorkbook AlarmSetup_ExcelWorkBook = null;
        public static ISheet AlarmSetup_ExcelSheet = null;
        public static IRow AlarmSetup_ExcelRow = null;
        public static IFont AlarmSetup_ExcelFont = null;
        public static ICellStyle AlarmSetup_ExcelCellStyle = null;

        public static ICell[] SheetCell = null;
        public static ICell[] AlarmSetupSheetCell = null;
        //////

    }
    public class NPIOHelper
    {
        public static int S = 0;
        public static int SS = 0;



        public HSSFWorkbook CreatExcel2003(string FileNames, string SheetNames)
        {
            HSSFWorkbook WorkBook2003 = new HSSFWorkbook(); //新建一个xls工作薄
            WorkBook2003.CreateSheet(SheetNames.Replace("/", "")); //新建一个Sheet工作表
            using (FileStream File2003 = new FileStream(FileNames, FileMode.Create))
            {
                WorkBook2003.Write(File2003); //创建文件流
                WorkBook2003.Close(); //关闭xls工作薄
            }
            return WorkBook2003;
        }

        public void CreateSheet(string FileName, string SheetName)
        {
            try
            {
                using (FileStream ExcelFileStream = System.IO.File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    NpioOperation.ExcelWorkBook = new HSSFWorkbook(ExcelFileStream); //获取Excel工作表名称、

                    ISheet sheet = NpioOperation.ExcelWorkBook.GetSheet(SheetName);
                    if (sheet == null)
                    {

                        NpioOperation.ExcelSheet = NpioOperation.ExcelWorkBook.CreateSheet(SheetName);
                    }
                
                }
                using (FileStream file2003 = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    NpioOperation.ExcelWorkBook.Write(file2003);
                    NpioOperation.ExcelWorkBook.Close();
                }

            }
            catch(Exception ex)
            {

            }


        }


        public String[] GetExcelSheetNames(string fileName)
        {
            OleDbConnection objConn = null;
            System.Data.DataTable dt = null;
            try
            {
                string connString = string.Empty;
                string FileType = fileName.Substring(fileName.LastIndexOf("."));
                if (FileType == ".xls")
                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;" +
                       "Data Source=" + fileName + ";Extended Properties=Excel 8.0;";
                else//.xlsx     
                    connString = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + fileName + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
                // 创建连接对象      
                objConn = new OleDbConnection(connString);
                // 打开数据库连接      
                objConn.Open();
                // 得到包含数据架构的数据表      
                dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                if (dt == null)
                {
                    return null;
                }
                String[] excelSheets = new String[dt.Rows.Count];
                int i = 0;
                // 添加工作表名称到字符串数组      
                foreach (DataRow row in dt.Rows)
                {
                    string strSheetTableName = row["TABLE_NAME"].ToString();
                    //过滤无效SheetName     
                    if (strSheetTableName.Contains("$") && strSheetTableName.Replace("'", "").EndsWith("$"))
                    {
                        strSheetTableName = strSheetTableName.Replace("$", string.Empty).Replace("'", string.Empty);
                        excelSheets[i] = strSheetTableName.Substring(0, strSheetTableName.Length - 0);//提取有效的sheet值  //MessageBox.Show(excelSheets[i]);  
                        i++;
                    }
                    //i++; //放在这里是错误的，   
                }
                return excelSheets;
            }
            catch (Exception ex)
            {
                throw;
                return null;
            }
            finally
            {
                // 清理      
                if (objConn != null)
                {
                    objConn.Close();
                    objConn.Dispose();
                }
                if (dt != null)
                {
                    dt.Dispose();
                }
            }
        }

        public void OpenExceWorkBook(string FileName, string SheetName)
        {
            try
            {
                using (FileStream ExcelFileStream = System.IO.File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    //System.IO.File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                    NpioOperation.ExcelWorkBook = new HSSFWorkbook(ExcelFileStream); //获取Excel工作表名称
                    NpioOperation.ExcelSheet = NpioOperation.ExcelWorkBook.GetSheet(SheetName);


                    ///  NpioOperation.ExcelFont = NpioOperation.ExcelWorkBook.CreateFont();//创建字体样式  
                    NpioOperation.ExcelFont.Color = HSSFColor.Red.Index;//设置字体颜色  
                    //   NpioOperation.ExcelCellStyle = NpioOperation.ExcelWorkBook.CreateCellStyle();//创建单元格样式  
                    NpioOperation.ExcelCellStyle.SetFont(NpioOperation.ExcelFont);//设置单元格样式中的字体样式  
                    NpioOperation.ExcelCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center; //水平对齐
                    NpioOperation.ExcelCellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直对齐
                    NpioOperation.ExcelCellStyle.WrapText = true;//自动换行

                }

            }
            catch
            {

            }


        }
        public void OpenExceWorkBook(string FileName, string SheetName, Queue<string> Q)
        {
            try
            {
                using (FileStream ExcelFileStream = System.IO.File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    NpioOperation.ExcelWorkBook = new HSSFWorkbook(ExcelFileStream); //获取Excel工作表名称
                    NpioOperation.ExcelSheet = NpioOperation.ExcelWorkBook.GetSheet(SheetName);

                    NpioOperation.ExcelFont = NpioOperation.ExcelWorkBook.CreateFont();//创建字体样式  
                    NpioOperation.ExcelFont.Color = HSSFColor.Red.Index;//设置字体颜色  

                    NpioOperation.ExcelCellStyle = NpioOperation.ExcelWorkBook.CreateCellStyle();//创建单元格样式  
                    NpioOperation.ExcelCellStyle.SetFont(NpioOperation.ExcelFont);//设置单元格样式中的字体样式  
                    NpioOperation.ExcelCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center; //水平对齐
                    NpioOperation.ExcelCellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直对齐
                    NpioOperation.ExcelCellStyle.WrapText = true;//自动换行

                    if(NpioOperation.ExcelSheet == null)
                    {
                        return;
                    }
                    NpioOperation.ExcelRow = NpioOperation.ExcelSheet.CreateRow(0);
                    NpioOperation.SheetCell = new ICell[Q.Count];


                    int QTemp = Q.Count;
                    for (int v = 0; v < QTemp; v++)
                    {
                        NpioOperation.SheetCell[v] = NpioOperation.ExcelRow.CreateCell(v); //为第0行v列创建单元格
                    }

                    for (int QT = 0; QT < QTemp; QT++)
                    {
                        NpioOperation.SheetCell[QT].SetCellValue(Q.Dequeue());  //写入
                    }

                }

            }
            catch
            {

            }


        }
        public void CloseExceWorkBook(string FileName)
        {
            try
            {
                using (FileStream file2003 = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    NpioOperation.ExcelWorkBook.Write(file2003);
                    NpioOperation.ExcelWorkBook.Close();
                }
            }
            catch
            {

            }


        }
        public void WriteExceWorkBookSetCellStyle(int RowNum, int ColoumNum)
        {
            try
            {
                NpioOperation.ExcelRow = NpioOperation.ExcelSheet.GetRow(RowNum);
                NpioOperation.ExcelRow.Cells[ColoumNum].CellStyle = NpioOperation.ExcelCellStyle; //为第RowNum行创建Q.Count个单元格

            }
            catch
            {
                //   System.Windows.MessageBox.Show("警告：保存的Excel工作表已打开，请关闭!");
            }

        }
        /// /////////////
        /// 

        public void WriteExceWorkBook(int tRowNum, string tColumn0, string tColumn1, string tColumn2, string tColumn3, string tColumn4, string tColumn5, string tColumn6, string tColumn7)
        {
            try
            {
                if (NpioOperation.ExcelSheet==null)
                {
                    return;
                }
                NpioOperation.ExcelRow = NpioOperation.ExcelSheet.CreateRow(tRowNum);

                NpioOperation.SheetCell = new ICell[8];

                for (int v = 0; v < 8; v++)
                {
                    NpioOperation.SheetCell[v] = NpioOperation.ExcelRow.CreateCell(v); //为第tRowNum行v列创建单元格
                }

                NpioOperation.SheetCell[0].SetCellValue(tColumn0);  //写入
                NpioOperation.SheetCell[1].SetCellValue(tColumn1);  //写入
                NpioOperation.SheetCell[2].SetCellValue(tColumn2);  //写入
                NpioOperation.SheetCell[3].SetCellValue(tColumn3);  //写入
                NpioOperation.SheetCell[4].SetCellValue(tColumn4);  //写入
                NpioOperation.SheetCell[5].SetCellValue(tColumn5);  //写入
                NpioOperation.SheetCell[6].SetCellValue(tColumn6);  //写入
                NpioOperation.SheetCell[7].SetCellValue(tColumn7);  //写入
            }
            catch
            {

            }


        }

        /// //////////
        public void WriteExcel2003(string FileNames, string SheetNames, Queue<string> Q, int RowNum)
        {
            try
            {
                HSSFWorkbook WorkBook2003 = null; //新建工作薄
                ISheet Sheet = null;
                IRow Row = null;
                using (FileStream fileStream = System.IO.File.Open(FileNames, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    WorkBook2003 = new HSSFWorkbook(fileStream); //获取Excel工作表名称
                    Sheet = WorkBook2003.GetSheet(SheetNames);
                    Sheet.CreateRow(RowNum);  //创建第RowNum行
                    Row = Sheet.GetRow(RowNum);
                    ICell[] SheetCell = new ICell[Q.Count];
                    int temp = Q.Count;  //创建之后就可以赋值了 
                    for (int i = 0; i < temp; i++)
                    {
                        SheetCell[i] = Row.CreateCell(i); //为第RowNum行创建Q.Count个单元格
                    }
                    for (int i = 0; i < temp; i++)
                    {
                        SheetCell[i].SetCellValue(Q.Dequeue());  //从头部读取然后写入
                    }

                }
                using (FileStream file2003 = new FileStream(FileNames, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    WorkBook2003.Write(file2003);
                    WorkBook2003.Close();
                }
            }
            catch
            {
                throw;
            }


        }
        public void WriteExcel2003_SetCell(string FileNames, string SheetNames, int RowNum, int ColoumNum, string CellValue)
        {
            HSSFWorkbook WorkBook2003 = null; //新建工作薄
            ISheet Sheet = null;
            IRow Row = null;
            using (FileStream fileStream = System.IO.File.Open(FileNames, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                WorkBook2003 = new HSSFWorkbook(fileStream); //获取Excel工作表名称
                Sheet = WorkBook2003.GetSheet(SheetNames);
                Row = Sheet.GetRow(RowNum);
                Row.Cells[ColoumNum].SetCellValue(CellValue);

                using (FileStream file2003 = new FileStream(FileNames, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    WorkBook2003.Write(file2003);
                    WorkBook2003.Close();
                }
            }


        }



    }



    public class NpioOperationOne
    {
        public static NPIOHelperOne ExcelWR = new NPIOHelperOne();
        public static string ExcelSaveFileName = null;
        public static string ExcelSaveSheetName = null;
        public static UInt16 ExcelSaveSheetNameCount = 0;
        public static string ExcelOpenFileName = null;
        public static string ExcelOpenTempFileName = null;
        public static string ExcelOpenSheetName = null;

        public static bool ExcelOpenFlag = false;
        public static string[] ExcelOpenSheetNames = null;

        public static byte Cout = 0;
        public static UInt16 RowNum = 0;
        public static bool NPIOOpState;

        //  public static FileStream ExcelFileStream = null;
        public static HSSFWorkbook ExcelWorkBook = null;
        public static ISheet ExcelSheet = null;
        public static IRow ExcelRow = null;
        public static IFont ExcelFont = null;
        public static ICellStyle ExcelCellStyle = null;

        ///////

        public static string AlarmSetup_ExcelOpenSheetName = null;
        public static string AlarmSetup_ExcelSaveSheetName = null;
        public static string AlarmSetup_ExcelSaveFileName = null;
        public static UInt16 AlarmSetup_RowNum = 0;
        public static byte AlarmSetup_Cout = 0;
        public static FileStream AlarmSetup_ExcelFileStream = null;
        public static HSSFWorkbook AlarmSetup_ExcelWorkBook = null;
        public static ISheet AlarmSetup_ExcelSheet = null;
        public static IRow AlarmSetup_ExcelRow = null;
        public static IFont AlarmSetup_ExcelFont = null;
        public static ICellStyle AlarmSetup_ExcelCellStyle = null;

        public static ICell[] SheetCell = null;
        public static ICell[] AlarmSetupSheetCell = null;
        //////

    }

    public class NPIOHelperOne
    {
        public static int S = 0;
        public static int SS = 0;

        public HSSFWorkbook CreatExcel2003(string FileNames)
        {
            HSSFWorkbook WorkBook2003 = new HSSFWorkbook(); //新建一个xls工作薄
           // WorkBook2003.CreateSheet("Sheet1"); //新建一个Sheet工作表
            using (FileStream File2003 = new FileStream(FileNames, FileMode.Create))
            {
                WorkBook2003.Write(File2003); //创建文件流
                WorkBook2003.Close(); //关闭xls工作薄
            }
            return WorkBook2003;
        }
        public HSSFWorkbook CreatExcel2003(string FileNames, string SheetNames)
        {
            HSSFWorkbook WorkBook2003 = new HSSFWorkbook(); //新建一个xls工作薄
            WorkBook2003.CreateSheet(SheetNames.Replace("/", "")); //新建一个Sheet工作表
            using (FileStream File2003 = new FileStream(FileNames, FileMode.Create))
            {
                WorkBook2003.Write(File2003); //创建文件流
                WorkBook2003.Close(); //关闭xls工作薄
            }
            return WorkBook2003;
        }

        public void CreateSheet(string FileName, string SheetName)
        {
            try
            {
                using (FileStream ExcelFileStream = System.IO.File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    NpioOperationOne.ExcelWorkBook = new HSSFWorkbook(ExcelFileStream); //获取Excel工作表名称
                    NpioOperationOne.ExcelSheet = NpioOperationOne.ExcelWorkBook.CreateSheet(SheetName);
                }
                using (FileStream file2003 = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    NpioOperationOne.ExcelWorkBook.Write(file2003);
                    NpioOperationOne.ExcelWorkBook.Close();
                }

            }
            catch
            {

            }


        }


        public String[] GetExcelSheetNames(string fileName)
        {
            OleDbConnection objConn = null;
            System.Data.DataTable dt = null;
            try
            {
                string connString = string.Empty;
                string FileType = fileName.Substring(fileName.LastIndexOf("."));
                if (FileType == ".xls")
                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;" +
                       "Data Source=" + fileName + ";Extended Properties=Excel 8.0;";
                else//.xlsx     
                    connString = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + fileName + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
                // 创建连接对象      
                objConn = new OleDbConnection(connString);
                // 打开数据库连接      
                objConn.Open();
                // 得到包含数据架构的数据表      
                dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                if (dt == null)
                {
                    return null;
                }
                String[] excelSheets = new String[dt.Rows.Count];
                int i = 0;
                // 添加工作表名称到字符串数组      
                foreach (DataRow row in dt.Rows)
                {
                    string strSheetTableName = row["TABLE_NAME"].ToString();
                    //过滤无效SheetName     
                    if (strSheetTableName.Contains("$") && strSheetTableName.Replace("'", "").EndsWith("$"))
                    {
                        strSheetTableName = strSheetTableName.Replace("$", string.Empty).Replace("'", string.Empty);
                        excelSheets[i] = strSheetTableName.Substring(0, strSheetTableName.Length - 0);//提取有效的sheet值  //MessageBox.Show(excelSheets[i]);  
                        i++;
                    }
                    //i++; //放在这里是错误的，   
                }
                return excelSheets;
            }
            catch (Exception ex)
            {
                throw;
                return null;
            }
            finally
            {
                // 清理      
                if (objConn != null)
                {
                    objConn.Close();
                    objConn.Dispose();
                }
                if (dt != null)
                {
                    dt.Dispose();
                }
            }
        }

        public void OpenExceWorkBook(string FileName, string SheetName)
        {
            try
            {
                using (FileStream ExcelFileStream = System.IO.File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    //System.IO.File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                    NpioOperationOne.ExcelWorkBook = new HSSFWorkbook(ExcelFileStream); //获取Excel工作表名称
                    NpioOperationOne.ExcelSheet = NpioOperationOne.ExcelWorkBook.GetSheet(SheetName);


                    //NpioOperationOne.ExcelFont = NpioOperationOne.ExcelWorkBook.CreateFont();//创建字体样式  
                    NpioOperationOne.ExcelFont.Color = HSSFColor.Red.Index;//设置字体颜色  
                    //NpioOperationOne.ExcelCellStyle = NpioOperation.ExcelWorkBook.CreateCellStyle();//创建单元格样式  
                    NpioOperationOne.ExcelCellStyle.SetFont(NpioOperationOne.ExcelFont);//设置单元格样式中的字体样式  
                    NpioOperationOne.ExcelCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center; //水平对齐
                    NpioOperationOne.ExcelCellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直对齐
                    NpioOperationOne.ExcelCellStyle.WrapText = true;//自动换行

                }

            }
            catch
            {

            }


        }
        public void OpenExceWorkBook(string FileName, string SheetName, Queue<string> Q)
        {
            try
            {
                using (FileStream ExcelFileStream = System.IO.File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    NpioOperationOne.ExcelWorkBook = new HSSFWorkbook(ExcelFileStream); //获取Excel工作表名称
                    NpioOperationOne.ExcelSheet = NpioOperationOne.ExcelWorkBook.GetSheet(SheetName);

                    NpioOperationOne.ExcelFont = NpioOperationOne.ExcelWorkBook.CreateFont();//创建字体样式  
                    NpioOperationOne.ExcelFont.Color = HSSFColor.Red.Index;//设置字体颜色  

                    NpioOperationOne.ExcelCellStyle = NpioOperationOne.ExcelWorkBook.CreateCellStyle();//创建单元格样式  
                    NpioOperationOne.ExcelCellStyle.SetFont(NpioOperationOne.ExcelFont);//设置单元格样式中的字体样式  
                    NpioOperationOne.ExcelCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center; //水平对齐
                    NpioOperationOne.ExcelCellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直对齐
                    NpioOperationOne.ExcelCellStyle.WrapText = true;//自动换行

                    NpioOperationOne.ExcelRow = NpioOperationOne.ExcelSheet.CreateRow(0);
                    NpioOperationOne.SheetCell = new ICell[Q.Count];


                    int QTemp = Q.Count;
                    for (int v = 0; v < QTemp; v++)
                    {
                        NpioOperationOne.SheetCell[v] = NpioOperationOne.ExcelRow.CreateCell(v); //为第0行v列创建单元格
                    }

                    for (int QT = 0; QT < QTemp; QT++)
                    {
                        NpioOperationOne.SheetCell[QT].SetCellValue(Q.Dequeue());  //写入
                    }

                }

            }
            catch
            {

            }


        }
        public void CloseExceWorkBook(string FileName)
        {
            try
            {
                using (FileStream file2003 = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    NpioOperationOne.ExcelWorkBook.Write(file2003);
                    NpioOperationOne.ExcelWorkBook.Close();
                }
            }
            catch
            {

            }


        }
        public void WriteExceWorkBookSetCellStyle(int RowNum, int ColoumNum)
        {
            try
            {
                NpioOperationOne.ExcelRow = NpioOperationOne.ExcelSheet.GetRow(RowNum);
                NpioOperationOne.ExcelRow.Cells[ColoumNum].CellStyle = NpioOperationOne.ExcelCellStyle; //为第RowNum行创建Q.Count个单元格

            }
            catch
            {
                //   System.Windows.MessageBox.Show("警告：保存的Excel工作表已打开，请关闭!");
            }

        }
        /// /////////////
        /// 

        public void WriteExceWorkBook(int tRowNum, string tColumn0, string tColumn1, string tColumn2, string tColumn3, string tColumn4, string tColumn5, string tColumn6, string tColumn7)
        {
            try
            {

                NpioOperationOne.ExcelRow = NpioOperationOne.ExcelSheet.CreateRow(tRowNum);

                NpioOperationOne.SheetCell = new ICell[8];

                for (int v = 0; v < 8; v++)
                {
                    NpioOperationOne.SheetCell[v] = NpioOperationOne.ExcelRow.CreateCell(v); //为第tRowNum行v列创建单元格
                }

                NpioOperationOne.SheetCell[0].SetCellValue(tColumn0);  //写入
                NpioOperationOne.SheetCell[1].SetCellValue(tColumn1);  //写入
                NpioOperationOne.SheetCell[2].SetCellValue(tColumn2);  //写入
                NpioOperationOne.SheetCell[3].SetCellValue(tColumn3);  //写入
                NpioOperationOne.SheetCell[4].SetCellValue(tColumn4);  //写入
                NpioOperationOne.SheetCell[5].SetCellValue(tColumn5);  //写入
                NpioOperationOne.SheetCell[6].SetCellValue(tColumn6);  //写入
                NpioOperationOne.SheetCell[7].SetCellValue(tColumn7);  //写入
            }
            catch
            {

            }


        }

        

        /// //////////EqFieldsBase
        public void WriteExcel2003(string FileNames, string SheetNames, Queue<string> Q, int RowNum)
        {
            try
            {
                HSSFWorkbook WorkBook2003 = null; //新建工作薄
                ISheet Sheet = null;
                IRow Row = null;
                using (FileStream fileStream = System.IO.File.Open(FileNames, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    WorkBook2003 = new HSSFWorkbook(fileStream); //获取Excel工作表名称
                    Sheet = WorkBook2003.GetSheet(SheetNames);
                    Sheet.CreateRow(RowNum);  //创建第RowNum行
                    Row = Sheet.GetRow(RowNum);
                    ICell[] SheetCell = new ICell[Q.Count];
                    int temp = Q.Count;  //创建之后就可以赋值了 
                    for (int i = 0; i < temp; i++)
                    {
                        SheetCell[i] = Row.CreateCell(i); //为第RowNum行创建Q.Count个单元格
                    }
                    for (int i = 0; i < temp; i++)
                    {
                        SheetCell[i].SetCellValue(Q.Dequeue());  //从头部读取然后写入
                    }

                }
                using (FileStream file2003 = new FileStream(FileNames, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    WorkBook2003.Write(file2003);
                    WorkBook2003.Close();
                }
            }
            catch
            {
                throw;
            }


        }
        public void WriteExcel2003_SetCell(string FileNames, string SheetNames, int RowNum, int ColoumNum, string CellValue)
        {
            HSSFWorkbook WorkBook2003 = null; //新建工作薄
            ISheet Sheet = null;
            IRow Row = null;
            using (FileStream fileStream = System.IO.File.Open(FileNames, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                WorkBook2003 = new HSSFWorkbook(fileStream); //获取Excel工作表名称
                Sheet = WorkBook2003.GetSheet(SheetNames);
                Row = Sheet.GetRow(RowNum);
                Row.Cells[ColoumNum].SetCellValue(CellValue);

                using (FileStream file2003 = new FileStream(FileNames, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    WorkBook2003.Write(file2003);
                    WorkBook2003.Close();
                }
            }


        }
  


    }

}
