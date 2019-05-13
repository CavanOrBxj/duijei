using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace GRPlatForm
{
    /// <summary>
    /// Http GET，POST处理过程类
    /// </summary>
    public class HttpProcessor
    {
        public TcpClient socket;
        public HttpServerBase srv;
        private Stream inputStream;
        public StreamWriter outputStream;

        public String http_method;
        public String http_url;
        public String http_protocol_versionstring;
        public Hashtable httpHeaders = new Hashtable();
        private string sSeparateString = string.Empty;

        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB
        //private const int BUF_SIZE = 1 * 1024 * 1024;

        public HttpProcessor(TcpClient s, HttpServerBase srv)
        {
            this.socket = s;
            this.srv = srv;
        }

        private string streamReadLine(Stream inputStream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }

        public void process()
        {
            // we can't use a StreamReader for input, because it buffers up extra data on us inside it's
            // "processed" view of the world, and we want the data raw after the headers
            inputStream = new BufferedStream(socket.GetStream());
            // we probably shouldn't be using a streamwriter for all output from handlers either
            outputStream = new StreamWriter(new BufferedStream(socket.GetStream()));
            try
            {
                if (parseRequest() == false)
                {
                    writeFailure();//返回失败标志
                    outputStream.Flush();
                    inputStream = null; outputStream = null;
                    socket.Close();
                    return;
                }
                readHeaders();
                if (http_method.Equals("GET"))
                {
                    handleGETRequest();
                }
                else if (http_method.Equals("POST"))
                {
                    handlePOSTRequest();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("处理请求错误: " + ex.Message);
                writeFailure();
            }
            outputStream.Flush();
            inputStream = null; outputStream = null;             
            socket.Close();
        }
        /// <summary>
        /// 验证处理请求
        /// </summary>
        /// <returns>处理成功标志</returns>
        public bool parseRequest()
        {
            String request = streamReadLine(inputStream);
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                //throw new Exception("invalid http request line");
                Console.WriteLine("头部验证错误，无法解析，丢弃处理！");
                return false;
            }
            http_method = tokens[0].ToUpper();
            http_url = tokens[1];
            http_protocol_versionstring = tokens[2];
            Console.WriteLine("头部验证字符串：" + request);
            return true;
        }

        public void readHeaders()
        {
            Console.WriteLine("readHeaders()");
            String line;
            sSeparateString = string.Empty;//初始化接收
            while ((line = streamReadLine(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    Console.WriteLine("got headers");
                    return;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    //throw new Exception("invalid http header line: " + line);
                    if (line != "platformtype" && (sSeparateString != "" && line.Contains(sSeparateString)))
                    {
                        Console.WriteLine("头部验证出错!");
                        return;
                    }
                    else
                    {
                        continue;
                    }
                }
                String name = line.Substring(0, separator);

                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++; // strip any spaces
                }
                string value = line.Substring(pos, line.Length - pos);
                if (name == "Content-Type" && sSeparateString == "")
                {
                    string[] sSeparateVaule = value.Split('=');
                    if (sSeparateVaule.Length > 1)
                    {
                        sSeparateString = "";
                    }
                }
                Console.WriteLine("头部: {0}:{1}", name, value);
                httpHeaders[name] = value;
            }
        }

        public void handleGETRequest()
        {
            srv.handleGETRequest(this);
        }

        private const int BUF_SIZE =10 * 1024 * 1024;

        public void handlePOSTRequest()
        {
            Console.WriteLine("获取POST数据开始:get post data start");
            int content_len = 0;
            MemoryStream ms = new MemoryStream();
            if (this.httpHeaders.ContainsKey("Content-Length"))
            {
                content_len = Convert.ToInt32(this.httpHeaders["Content-Length"]);
                if (content_len > MAX_POST_SIZE)
                {
                    throw new Exception(String.Format("POST Content-Length({0}) too big for this simple server",
                          content_len));

                }
                byte[] buf = new byte[BUF_SIZE];
                int to_read = content_len;
                while (to_read > 0)
                {
                    int numread = this.inputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
                    if (numread == 0)
                    {
                        if (to_read == 0)
                        {
                            break;
                        }
                        else
                        {
                            //throw new Exception("client disconnected during post");
                            Console.WriteLine("POST时客户端连接断开！");
                        }
                    }
                    to_read -= numread;
                    ms.Write(buf, 0, numread);
                }
                //把接收的文件保存起来
                string strSaveRecfilepath = ServerForm.sRevTarPath + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".tar";
                //FileStream fs = new FileStream(ServerForm.sTarPathName, FileMode.Create); //打开一个写入流
                FileStream fs = new FileStream(strSaveRecfilepath, FileMode.Create); //打开一个写入流
                ms.WriteTo(fs);
                fs.Close();
                ServerForm.sTmptarFileName = ServerForm.sTarPathName;
                Console.WriteLine("接收Tar文件成功！");
                ServerForm.lRevFiles.Add(strSaveRecfilepath);
                ms.Seek(0, SeekOrigin.Begin);
                writeSuccess();
            }
            Console.WriteLine("get post data end");
            srv.handlePOSTRequest(this, new StreamReader(ms));

        }

        public void writeSuccess(string content_type = "text/html")
        {
            outputStream.WriteLine("HTTP/1.0 200 OK");
            outputStream.WriteLine("Content-Type: " + content_type);
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }

        public void writeFailure()
        {
            outputStream.WriteLine("HTTP/1.0 404 File not found");
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }
    }
    /// <summary>
    /// Http服务基类
    /// </summary>
    public abstract class HttpServerBase
    {
        protected int port;
        protected IPAddress ipServer;
        TcpListener listener;
        bool is_active = true;

        public HttpServerBase(IPAddress ipserver, int port)
        {
            this.port = port;
            this.ipServer = ipserver;
        }

        public HttpServerBase(int port)
        {
            this.port = port;
        }

        public void listen()
        {
            if (ipServer == null)
            {
                listener = new TcpListener(port);//没有具体绑定IP
            }
            else
            {
                listener = new TcpListener(ipServer, port);//绑定具体IP
            }
            listener.Start();
            while (is_active)
            {
                TcpClient s = listener.AcceptTcpClient();
                HttpProcessor processor = new HttpProcessor(s, this);
                Thread thread = new Thread(new ThreadStart(processor.process));
                thread.Start();
                Thread.Sleep(1);
            }
        }

        public bool StopListen()
        {
            try
            {
                listener.Stop();
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public abstract void handleGETRequest(HttpProcessor p);

        public abstract void handlePOSTRequest(HttpProcessor p, StreamReader inputData);
    }
    /// <summary>
    /// Http服务的接口实现
    /// </summary>
    public class HttpServer : HttpServerBase
    {
        public HttpServer(int port)
            : base(port)
        {
        }

        public HttpServer(IPAddress ipaddr, int port)
            : base(ipaddr, port)
        {
        }
        public override void handleGETRequest(HttpProcessor p)
        {
            Console.WriteLine("request: {0}", p.http_url);
            p.writeSuccess();
        }

        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            Console.WriteLine("POST request: {0}", p.http_url);
            p.writeSuccess();
        }
    }


}
