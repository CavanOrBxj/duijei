using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GRPlatForm
{
    [Serializable]
    public class EBM
    {
        public string EBMVersion; //
        //public string EBEID; //      新没有2018-05-22

        public string EBMID;

        //public RelatedEBM RelatedEBM;//

        public RelatedInfo RelatedInfo;      //新 2018-05-22

        public string TestType;  //

        public string StartTime;

        public string EndTime;

        public string ProcessMethod;//

        public MsgBasicInfo MsgBasicInfo;//

        public MsgContent MsgContent;//

        public Dispatch Dispatch;
    }

    [Serializable]
    public class RelatedEBM  //
    {
        public string EBMID;
    }
    [Serializable]
    public class RelatedInfo    //新 2018-05-22
    {
        public string EBMID;
        public string EBIID;
    }

    [Serializable]
    public class MsgBasicInfo
    {
        public string MsgType;
        public string SenderName;
        public string SenderCode;
        //public string SentTime;
        public string SendTime;   //新 2018-05-22
        public string EventType;
        public string Severity;

        public string StartTime;
        public string EndTime;
    }

    [Serializable]
    public class MsgContent
    {
        public string LanguageCode;

        public string MsgTitle;

        public string MsgDesc;

        public string AreaCode;

        public string ProgramNum;     //新 2018-05-22   加

        public Auxiliary Auxiliary;
    }

    [Serializable]
    public class Auxiliary
    {
        public string AuxiliaryType;

        public string AuxiliaryDesc;

        public string Size;

        public string Digest;
    }

    [Serializable]
    public class Coverage
    {
       [XmlElement]
       public List<Area> Area;
    }

    [Serializable]
    public class Area
    {
        public string AreaName;
        public string AreaCode;
    }

    [Serializable]
    public class Dispatch
    {
        public string LanguageCode;
        public EBRPS EBRPS;   //新 2018-05-22   加
        public EBRTS EBRTS;   //新 2018-05-22   加
        //public EBEAS EBEAS;
        public EBRAS EBRAS;   //新 2018-05-22   改
        public EBEBS EBEBS;
    }

    [Serializable]
    public class EBRPS    //新 2018-05-22   加
    {
        public string EBRID;//应急广播平台编号
    }

    [Serializable]
    public class EBRTS    //新 2018-05-22   加
    {
        public string EBRID;//电台/电视台编号
    }

    //[Serializable]
    //public class EBEAS
    //{
    //    public string EBEID;//应急广播消息适配器ID
    //}
    [Serializable]
    public class EBRAS    //新 2018-05-22   改
    {
        public string EBRID;//应急广播消息适配器ID
    }

    [Serializable]
    public class EBEBS
    {
        public string BrdSysType;

        public string BrdSysInfo;
    }

}
