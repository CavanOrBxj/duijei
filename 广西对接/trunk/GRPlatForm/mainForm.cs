using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using EBSignature;

namespace GRPlatForm
{
    public partial class mainForm : Form
    {
        public IniFiles ini;
        //子窗体定义
        private ServerIPSetForm setipFrm;
        private ServerSetForm setServerFrm;
        private TmpFolderSetForm tmpforldFrm;
        private InfoSetForm infoFrm;
        private ServerForm serverFrm;
        private From_Timetactics TimetacticsFrm;
        //
        public static dbAccess dba;
        public List<string> lTarPathName = new List<string>();//接收到的Tar包列表
        public static string sSendTarName = "";//发送Tar包名字

        //public bool MQStartFlag = false;
        public string strSourceType = "";
        public string strSourceName = "";
        public string strSourceID = "";
      
        public static SerialPort comm = new SerialPort();
        public static SerialPort sndComm = new SerialPort();//临时发送语音用

        public static bool bWaitOrNo = true;//等待 2016-04-01
        public static bool bMsgStatusFree = false;//

        private List<byte> lCommData = new List<byte>();
        private object oComm = new object();
        private Thread thComm;

       // public USBE usb = new USBE();
        public IntPtr phDeviceHandle = (IntPtr)1;

        public static bool usbflag = false;   //2018-05-25  密码器是否打开成功标记

        public mainForm()
        {
            InitializeComponent();
            ini = new IniFiles(@Application.StartupPath + "\\Config.ini");
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            try
            {
              SingletonInfo.GetInstance().m_UsbPwsSupport = ini.ReadValue("USBPSW", "USBPSWSUPPART");

                //打开密码器
                //if (m_UsbPwsSupport == "1")
                //{
                //    try
                //    {
                //        int nReturn = usb.USB_OpenDevice(ref phDeviceHandle);
                //        if (nReturn != 0)
                //        {
                //            usbflag = false;    //2018-05-25
                //            MessageBox.Show("密码器打开失败！");
                //        }
                //        else
                //            usbflag = true;    //2018-05-25
                //    }
                //    catch (Exception em)
                //    {
                //        usbflag = false;    //2018-05-25
                //        MessageBox.Show("密码器打开失败：" + em.Message);
                //    }
                //}

                //初始化写日志线程
                string sLogPath = Application.StartupPath + "\\Log";
                if (!Directory.Exists(sLogPath))
                    Directory.CreateDirectory(sLogPath);
                Log.Instance.LogDirectory = sLogPath + "\\";
                Log.Instance.FileNamePrefix = "EBD_";
                Log.Instance.CurrentMsgType = MsgLevel.Debug;
                Log.Instance.logFileSplit = LogFileSplit.Daily;
                Log.Instance.MaxFileSize = 2;
                Log.Instance.InitParam();

                GetAuditData();
                this.Text = "应急广播消息服务V" + Application.ProductVersion;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region  获取审核策略数据

        private void GetAuditData()
        {
            string sqlstr = "select * from EBTime_Strategy";
            DataTable dt = mainForm.dba.getQueryInfoBySQL(sqlstr);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                timestrategies tt = new timestrategies();
                tt.ID = dt.Rows[i][0].ToString();
                tt.StartTime = dt.Rows[i][1].ToString();
                tt.EndTime = dt.Rows[i][2].ToString();
                tt.EvenType = dt.Rows[i][3].ToString();
                SingletonInfo.GetInstance().audit.TimeList.Add(tt);
            }
        }
        #endregion

        #region 菜单响应

        private void mnuServerAddrSet_Click(object sender, EventArgs e)
        {
            if (setServerFrm == null || setServerFrm.IsDisposed)
            {
                setServerFrm = new ServerSetForm();
                setServerFrm.MdiParent = this;
                setServerFrm.Show();
            }
            else
            {
                if (setServerFrm.WindowState == FormWindowState.Minimized)
                {
                    setServerFrm.WindowState = FormWindowState.Normal;
                }
                else
                    setServerFrm.Activate();
            }
        }

        private void ServerIPSet_Click(object sender, EventArgs e)
        {
            if (setipFrm == null || setipFrm.IsDisposed)
            {
                setipFrm = new ServerIPSetForm();
                setipFrm.MdiParent = this;
                setipFrm.Show();
            }
            else
            {
                if (setipFrm.WindowState == FormWindowState.Minimized)
                {
                    setipFrm.WindowState = FormWindowState.Normal;
                }
                else
                    setipFrm.Activate();
            }
        }

        private void mnuFolderSet_Click(object sender, EventArgs e)
        {
            if (tmpforldFrm == null || tmpforldFrm.IsDisposed)
            {
                tmpforldFrm = new TmpFolderSetForm();
                tmpforldFrm.MdiParent = this;
                tmpforldFrm.Show();
            }
            else
            {
                if (tmpforldFrm.WindowState == FormWindowState.Minimized)
                {
                    tmpforldFrm.WindowState = FormWindowState.Normal;
                }
                else
                    tmpforldFrm.Activate();
            }
        }

        private void mnuSysInfoSet_Click(object sender, EventArgs e)
        {
            if (infoFrm == null || infoFrm.IsDisposed)
            {
                infoFrm = new InfoSetForm();
                infoFrm.MdiParent = this;
                infoFrm.Show();
            }
            else
            {
                if (infoFrm.WindowState == FormWindowState.Minimized)
                {
                    infoFrm.WindowState = FormWindowState.Normal;
                }
                else
                    infoFrm.Activate();
            }
        }

        private void mnuServerStart_Click(object sender, EventArgs e)
        {
            if (serverFrm == null || serverFrm.IsDisposed)
            {
                try
                {
                    serverFrm = new ServerForm();
                    serverFrm.MdiParent = this;
                    serverFrm.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "mainFormb");
                }
            }
            else
            {
                if (serverFrm.WindowState == FormWindowState.Minimized)
                {
                    serverFrm.WindowState = FormWindowState.Normal;
                }
                else
                    serverFrm.Activate();
            }
        }

        #endregion End

        private void mnuExit_Click(object sender, EventArgs e)
        {
            if (serverFrm != null)
            {
                serverFrm.Close();
                serverFrm.Dispose();
            }
            if (comm != null)
            {
                comm.Close();
                comm.Dispose();
            }
            if (sndComm != null)
            {
                sndComm.Close();
                sndComm.Dispose();
            }
            this.Dispose(true);//释放资源
            Application.Exit();
            Application.ExitThread();
            Environment.Exit(0);
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
            if (serverFrm != null)
                serverFrm.Hide();
            this.Visible = false;
            this.Hide();
            this.ShowInTaskbar = false;
            this.nIcon.Visible = true;
            //关闭密码器
            //if (m_UsbPwsSupport == "1")
            //{
            //    try
            //    {
            //        int nDeviceHandle = (int)phDeviceHandle;
            //        int nReturn = usb.USB_CloseDevice(ref nDeviceHandle);
            //    }
            //    catch (Exception em)
            //    {
            //        MessageBox.Show("密码器关闭失败：" + em.Message);
            //    }
            //}
            if (serverFrm != null)
            {
                serverFrm.Close();
                serverFrm.Dispose();
            }
            if (comm != null)
            {
                comm.Close();
                comm.Dispose();
            }
            if (sndComm != null)
            {
                sndComm.Close();
                sndComm.Dispose();
            }
            this.Dispose();                //释放资源
            Application.Exit();
            Application.ExitThread();
            Environment.Exit(0);
        }

        private void mnuShow_Click(object sender, EventArgs e)
        {
            if (!this.ShowInTaskbar)
            {
                this.Visible = true;
                this.Show();
                this.ShowInTaskbar = true;
                nIcon.Visible = false;
                foreach (Form frm in this.MdiChildren)
                {
                    if (!frm.IsDisposed & frm != null)
                        frm.Show();
                }
            }
        }

        private void mnuQuit_Click(object sender, EventArgs e)
        {
            if (serverFrm != null)
            {
                serverFrm.Close();
                serverFrm.Dispose();
            }
            if (comm != null)
            {
                comm.Close();
                comm.Dispose();
            }
            if (sndComm != null)
            {
                sndComm.Close();
                sndComm.Dispose();
            }
            if (thComm != null)
            {
                thComm.Abort();
                thComm = null;
            }
            this.Dispose(true);
            Application.ExitThread();
        }

        /// <summary>
        /// 文件签名
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="strEBDID"></param>
        public void GenerateSignatureFile(string strPath, string strEBDID)
        {
            if (SingletonInfo.GetInstance().m_UsbPwsSupport != "1")
            {
                return;
            }
            string sSignFileName = "\\EBDB_" + strEBDID + ".xml";
            using (FileStream SignFs = new FileStream(strPath + sSignFileName, FileMode.Open))
            {
                StreamReader signsr = new StreamReader(SignFs, Encoding.UTF8);
                string xmlsign = signsr.ReadToEnd();
                signsr.Close();
                responseXML signrp = new responseXML();
                XmlDocument xmlSignDoc = new XmlDocument();
                try
                {
                    //对文件进行签名
                    int nDeviceHandle = (int)phDeviceHandle;
                    byte[] pucSignature = Encoding.UTF8.GetBytes(xmlsign);

                    string strSignture = "";
                    string strpucCounter = "";
                    string strpucSignCerSn = "000000000001";
                    string nReturn = "";//usb.Platform_CalculateSingature_String(nDeviceHandle, 1, pucSignature, pucSignature.Length, ref strSignture);
                    //生成签名文件
                    string xmlSIGNFileName = "\\EBDS_EBDB_" + strEBDID + ".xml";
                    xmlSignDoc = signrp.SignResponse(strEBDID, strpucCounter, strpucSignCerSn, nReturn);
                    CommonFunc cm = new CommonFunc();
                    cm.SaveXmlWithUTF8NotBOM(xmlSignDoc, strPath + xmlSIGNFileName);
                    if (cm != null)
                    {
                        cm = null;
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.LogWrite("签名文件错误：" + ex.Message);
                }
            }
        }
        /// <summary>
        /// 国标文件签名   2018-05-31
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="strEBDID"></param>
        public void GB_GenerateSignatureFile(string strPath, string strEBDID, EbmSignature ebsign, int sinIndex, string SignCerSn)
        {
            string sSignValue = "";
            string sSignFileName = "\\EBDB_" + strEBDID + ".xml";

            using (FileStream fsRead = new FileStream(strPath + sSignFileName, FileMode.Open))
            {
                int fsLen = (int)fsRead.Length;
                byte[] byteData = new byte[fsLen];
                int r = fsRead.Read(byteData, 0, byteData.Length);
                byte[] b3= new byte[64];

                ebsign.EbMsgSignWithoutUTC(byteData, byteData.Length, ref b3, sinIndex);   //
                sSignValue = Convert.ToBase64String(b3);
            }

            responseXML signrp = new responseXML();
            XmlDocument xmlSignDoc = new XmlDocument();
            try
            {
                string strSignture = "";
                string strpucCounter = "";
                string strpucSignCerSn = SignCerSn;
                //生成签名文件
                string xmlSIGNFileName = "\\EBDS_EBDB_" + strEBDID + ".xml";
                xmlSignDoc = signrp.SignResponse(strEBDID, strpucCounter, strpucSignCerSn, sSignValue);
                CommonFunc cm = new CommonFunc();
                cm.SaveXmlWithUTF8NotBOM(xmlSignDoc, strPath + xmlSIGNFileName);
                if (cm != null)
                {
                    cm = null;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.LogWrite("签名文件错误：" + ex.Message);
            }
        }


        /// <summary>
        /// 验签函数  20181118
        /// </summary>
        /// <param name="ebsign"></param>
        /// <param name="InputData"></param>
        /// <param name="signatureData"></param>
        /// <param name="CerNo"></param>
        /// <returns></returns>
        public bool GB_EbMsgSignVerifyWithoutUTC(EbmSignature ebsign,byte[] InputData,byte[] signatureData,string CerNo)
        {
            bool flag = false;
            List<byte> signatureDataList = new List<byte>();

            Convert.ToInt32(CerNo);
            int leng = CerNo.Length / 2;
            for (int i = 0; i < leng; i++)
            {
                byte pp = (byte)Convert.ToInt32(CerNo.Substring(2 * i, 2));
                signatureDataList.Add(pp);
            }
            signatureDataList.AddRange(signatureData);
            if (ebsign.EbMsgSignVerifyWithoutUTC(InputData, InputData.Length, signatureDataList.ToArray()))
            {
                flag = true;
            }
            return flag;
        }


        private void mnuauditpolicy_Click(object sender, EventArgs e)
        {
            try
            {
                if (TimetacticsFrm == null || TimetacticsFrm.IsDisposed)
                {
                    TimetacticsFrm = new From_Timetactics();
                    TimetacticsFrm.MdiParent = this;
                    TimetacticsFrm.Show();
                }
                else
                {
                    if (TimetacticsFrm.WindowState == FormWindowState.Minimized)
                    {
                        TimetacticsFrm.WindowState = FormWindowState.Normal;
                    }
                    else
                        TimetacticsFrm.Activate();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
