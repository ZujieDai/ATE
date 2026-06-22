using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaiTer.ATE.DataModel;
using SaiTer.ATE.DataModel.DataBaseModel;

namespace SaiTer.ATE.IDAL.SQLiteIDAL
{
    public class StatisticsManage
    {
        public static StatisticsModel GetStatisticsInfo(string sStartTime, string sEndTime, string sModel)
        {
            StatisticsModel statisticsInfo = new StatisticsModel();

            //statisticsInfo.TestCount=ChargerInfoManage.GetTestCount(sStartTime, sEndTime);
            //statisticsInfo.PassCount = ChargerInfoManage.GetTestCount(sStartTime, sEndTime, 1);//这里因为以前的问题，没有更新总结论，所以暂时不用
            ////statisticsInfo.FailCount = ChargerInfoManage.GetTestCount(sStartTime, sEndTime, 2);
            //statisticsInfo.FailCount = statisticsInfo.TestCount - statisticsInfo.PassCount;

            List<TrialDataModel> FailResults = new List<TrialDataModel>();//不合格测试项的集合
            List<ChargerInfoModel> chargerInfoList = ChargerInfoManage.GetTestChargerInfo(sStartTime, sEndTime, sModel);
            statisticsInfo.TestCount = chargerInfoList.Count;
            foreach (ChargerInfoModel chargerInfo in chargerInfoList)
            {
                chargerInfo.CheckResult = DataModel.EnumModel.EmTrialResult.Pass;//默认合格
                List<TrialDataModel> itemResults = TrialItemResultTmpManage.GetTestItemResultFromPKID(chargerInfo.PKID.ToString());
                foreach (TrialDataModel itemResult in itemResults)
                {
                    if (itemResult.TrialFinalResult == DataModel.EnumModel.EmTrialResult.Fail)//不合格的测试项数据
                    {
                        chargerInfo.CheckResult = itemResult.TrialFinalResult;//有一项不合格，总结论就不合格
                        FailResults.Add(itemResult);//添加到不合格测试项的集合中
                    }
                }

                //重新统计合格数量
                if (chargerInfo.CheckResult == DataModel.EnumModel.EmTrialResult.Fail)
                {
                    statisticsInfo.FailCount++;
                }
                else if (chargerInfo.CheckResult == DataModel.EnumModel.EmTrialResult.Pass)
                {
                    statisticsInfo.PassCount++;
                }

            }

            //计算合格率
            statisticsInfo.PassRate = (double)statisticsInfo.PassCount * 100 / statisticsInfo.TestCount;
            statisticsInfo.FailRate = (double)statisticsInfo.FailCount * 100 / statisticsInfo.TestCount;

            int MaxItemNameLength = 5;
            List<FailResultInfo> failResults = new List<FailResultInfo>();
            //统计不合格测试项的详细信息
            foreach (TrialDataModel itemResult in FailResults)
            {
                int iIndex = failResults.FindIndex(x => x.ItemName.Equals(itemResult.TrialName));
                if (iIndex < 0)
                {
                    FailResultInfo fritmp = new FailResultInfo();
                    fritmp.ItemName = itemResult.TrialName;
                    fritmp.FailCount = 1;
                    fritmp.FailBarCodes = itemResult.BarCode;
                    failResults.Add(fritmp);
                }
                else
                {
                    failResults[iIndex].FailBarCodes = failResults[iIndex].FailBarCodes + "," + itemResult.BarCode;
                    failResults[iIndex].FailCount++;
                }

                if (itemResult.TrialName.Length > MaxItemNameLength)
                {
                    MaxItemNameLength = itemResult.TrialName.Length;
                }
            }

            //最后把统计信息编辑成描述文字
            StringBuilder sbtmp = new StringBuilder();
            string sHhf = "\r\n";
            sbtmp.Append("统计开始时间：" + sStartTime + sHhf);
            sbtmp.Append("统计结束时间：" + sEndTime + sHhf);
            if (sModel.Trim() != "")
            {
                sbtmp.Append("产品型号：" + sModel + sHhf);
            }
            sbtmp.Append("测试总数：" + statisticsInfo.TestCount.ToString() + sHhf);
            sbtmp.Append("测试合格：" + statisticsInfo.PassCount.ToString() + sHhf);
            sbtmp.Append("测试不合格：" + statisticsInfo.FailCount.ToString() + sHhf);
            sbtmp.Append("测试合格率：" + statisticsInfo.PassRate.ToString("F2") + "%" + sHhf);
            sbtmp.Append("测试不合格率：" + statisticsInfo.FailRate.ToString("F2") + "%" + sHhf);
            sbtmp.Append("测试项目不合格信息统计：" + sHhf);
            sbtmp.Append("测试项目不合格数量：" + failResults.Count.ToString() + sHhf);
            int iItemIndex = 0;
            foreach (var item in failResults)
            {
                iItemIndex++;
                sbtmp.Append("【" + iItemIndex.ToString() + "】"
                    + item.ItemName.PadRight(MaxItemNameLength + 6, ' ')
                    + "不合格次数：" + item.FailCount.ToString() + sHhf);
            }
            statisticsInfo.StatisticsDescriptionInfo = sbtmp.ToString();

            return statisticsInfo;
        }
    }

    public class FailResultInfo
    {
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 不通过数量
        /// </summary>
        public int FailCount { get; set; }
        /// <summary>
        /// 不合格条码
        /// </summary>
        public string FailBarCodes { get; set; }
    }
}
