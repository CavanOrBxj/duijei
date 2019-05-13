using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPlatForm
{

    public class timestrategies
    {
        /// <summary>
        /// 策略ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 策略开始时间
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// 策略结束时间
        /// </summary>
        public string EndTime { get; set; }
        /// <summary>
        /// 事件类型
        /// </summary>
        public string EvenType { get; set; }
    }


    public class strategytactics
    {
        public List<timestrategies> TimeList; 
    }


    public class LasttarInfo
    {

        /// <summary>
        /// 消息等级
        /// </summary>
        public string Severity { get; set; }

        public string  paramValue { get; set; }

        public string TsCmdStoreID { get; set; }

        public string sORG_ID2 { get; set; }

        public string EBDID { get; set; }

    }

    public class tarInfoSecondSend
    {

        public string paramValue { get; set; }

        public string TsCmdStoreID { get; set; }

        public string sORG_ID2 { get; set; }

        public int Countdown { get; set; }

    }



}
