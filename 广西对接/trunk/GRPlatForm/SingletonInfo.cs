using System.Threading;
using System.Collections.Generic;
using System.Data;
using GRPlatForm;
using System;

namespace GRPlatForm
{
    public class SingletonInfo
    {
        private static SingletonInfo _singleton;


        public string Longitude;//经度
        public string Latitude;//纬度
        public string CurrentURL;//当前平台对县平台的url

        public string CheckEBMStatusFlag;//自动审核1 人工审核0

        //   public Dictionary<string, string> AreacodeDic;

        public strategytactics audit;

        public string CurrentCertNo;//当前所使用的证书号  用于签名

        public string VerifyCertFaultStr;//验签失败信息

        public LasttarInfo lasttarinfo;//上个tar信息

        public string EBDID;//

        public bool resendFlag;//一般等级包被顶后重新播放

        public  string m_UsbPwsSupport;//是否需要签名验签 1表示支持签名 2表示不支持
        private SingletonInfo()                                                                 
        {
            Longitude = "";
            Latitude = "";
            CurrentURL = "";
            CheckEBMStatusFlag = "";
            audit = new strategytactics();
            audit.TimeList = new List<timestrategies>();
            CurrentCertNo = "";
            VerifyCertFaultStr = "";
            lasttarinfo = new LasttarInfo();
            EBDID = "";
            resendFlag = false;
            m_UsbPwsSupport = "";
        }
        public static SingletonInfo GetInstance()
        {
            if (_singleton == null)
            {
                Interlocked.CompareExchange(ref _singleton, new SingletonInfo(), null);
            }
            return _singleton;
        }
    }
}