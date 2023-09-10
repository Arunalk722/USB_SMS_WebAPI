using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using context = System.Web.HttpContext;

namespace WebAPI
{
    public partial class Index : System.Web.UI.Page
    {
        string connectionString = "Data Source=127.0.0.1,1433;Initial Catalog=SMS_GW;Persist Security Info=True;User ID=DBUserName;Password=YourPassword";
        SqlConnection connection = null;

        public void SMSCRUD(string QueryStr, string MessageStr)
        {
            try
            {
                connection = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand();
                connection.Open();
                cmd.Connection = connection;
                cmd.CommandText = QueryStr;
                SqlDataReader sqlDataReader;
                sqlDataReader = cmd.ExecuteReader();
                connection.Close();
            }
            catch (Exception ex)
            {
                ErrorLogRecord.ErrorLogWrite(ex);
            }
        }
  
        SerialPort serialport = new SerialPort();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                try
                {
                    string UserName = Request.QueryString["User"];
                    string Hash = Request.QueryString["Hash"];
                    string Message = Request.QueryString["Msg"];
                    string Recever = Request.QueryString["Recever"];
                    LblMessage.Text = Message;
                    LblRecerver.Text = Recever;

                    // Declare and initialize a HttpContext variable
                    HttpContext context = HttpContext.Current;

                    // Make sure the context is not null before accessing its Request property
                    if (context != null)
                    {
                        string ReqURL = context.Request.Url.ToString();
                        string localUrl = context.Request.Url.Scheme + "://" + "127.0.0.1" + ":" + context.Request.Url.Port + context.Request.Url.PathAndQuery;

                        string clientIP = HttpContext.Current.Request.UserHostAddress;
                        APIEvent(Message, Recever, UserName, Hash, localUrl, clientIP);
                    }
                }
                catch (Exception ex)
                {
                    DBerror.Errorlog(ex);
                }
            }
        }

        public void APIEvent(string Message, string Recever, string UserName, string Hash, string ReqURL, string clientIP)
        {
            try
            {        
                string UserNameKey = "ArunaLK722";
                string HashKey = "AccessHashKey";
                string message = string.Empty;

                if (UserNameKey == UserName && HashKey == Hash)
                {
                    LblMsg.Text = "Access token valid";
                    string response = SendSMS(Message, Recever);
                    if (response.Contains("OK"))
                    {
                        LblMsg.Text = "SMS sent successfully! Code :" +response;
                        message = LblMsg.Text;
                        string SQLQuery = "INSERT INTO [dbo].[TBL_SuccessSMS] ([SMSURL], [Recever], [Message], [State], [DateTime], [IPAddress]) VALUES ('" + ReqURL + "', '" + Recever + "','" + Message.Replace("'", "''")+"', '0', '" + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "', '" + clientIP + "')"; SMSCRUD(SQLQuery, "Send MSG Successfull: " + response);
                        MessageRec.Tracrt(Message, Recever, message);
                    }
                    else
                    {
                        LblMsg.Text = "SMS sent Failed! Error: " + response;
                        message = LblMsg.Text;

                        string SQLQuery = "INSERT INTO [dbo].[TBL_FaildSMS] ([SMSURL] ,[Recever] ,[Message] ,[State] ,[DateTime] ,[IPAddress])     VALUES ('" + ReqURL + "' ,'" + Recever + "' ,'" + Message.Replace("'", "''") + "' ,'999','" + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "','" + clientIP + "')";                      
                        SMSCRUD(SQLQuery, message);
                        MessageRecErr.Tracrt(Message, Recever, message);                        
                    }
                    string logMessage = string.Format("SMS send response: {0}", response);
                    MessageResp.Res(Message, Recever, logMessage);
                }
                else
                {
                    LblMsg.Text = "Please check access token";
                    message = LblMsg.Text;
                    MessageRecErr.Tracrt(Message, Recever, message);
                }
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + message + "');", true);
            }
            catch (Exception ex)
            {
                string SQLQuery = "INSERT INTO [dbo].[TBL_FaildSMS] ([SMSURL] ,[Recever] ,[Message] ,[State] ,[DateTime] ,[IPAddress])     VALUES ('" + ReqURL + "' ,'" + Recever + "' ,'" + Message.Replace("'", "''") + "' ,'999','" + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "','" + clientIP + "')";
                SMSCRUD(SQLQuery, "Exception Error");
                ErrorLogRecord.ErrorLogWrite(ex);
            }
        }
    public string SendSMS(string Msg, string Number)
        {
            try
            {
                string response = string.Empty;
                int mSpeed = 1;
                //change port as USB mordem
                serialport.PortName = "COM4"; 
                //port rate
                serialport.BaudRate = 96000;
                serialport.Parity = Parity.None;
                serialport.DataBits = 8;
                serialport.StopBits = StopBits.One;
                serialport.Handshake = Handshake.XOnXOff;
                serialport.DtrEnable = true;
                serialport.RtsEnable = true;
                serialport.NewLine = Environment.NewLine;
                if (serialport.IsOpen)
                {
                    serialport.WriteLine("AT+CMGF=1" + Environment.NewLine);
                    System.Threading.Thread.Sleep(200);
                    serialport.WriteLine("AT+CSCS=GSM" + Environment.NewLine);
                    System.Threading.Thread.Sleep(200);
                    serialport.WriteLine("AT+CMGS=" + (char)34 + Number + (char)34 + "\n");    //Set Recievers Phone Number
                    System.Threading.Thread.Sleep(200);
                    serialport.WriteLine(Msg + (char)26);
                    System.Threading.Thread.Sleep(mSpeed);
                     response = serialport.ReadExisting();
                }
                else
                {
                    serialport.Open();
                    serialport.WriteLine("AT+CMGF=1" + Environment.NewLine);
                    System.Threading.Thread.Sleep(200);
                    serialport.WriteLine("AT+CSCS=GSM" + Environment.NewLine);
                    System.Threading.Thread.Sleep(200);
                    serialport.WriteLine("AT+CMGS=" + (char)34 + Number + (char)34 + "\n");    //Set Recievers Phone Number
                    System.Threading.Thread.Sleep(200);
                    serialport.WriteLine(Msg + (char)26);
                    System.Threading.Thread.Sleep(mSpeed);
                    response = serialport.ReadExisting();
                }
              
                serialport.Close();
                return response;            
            }
            catch (Exception ex)
            {
                ErrorLogRecord.ErrorLogWrite(ex);
                return "";
            }
        }
    }

    //log on database error
    public class DBerror
    {
        public static String ErrorLineNo, ErrorMessage, ExceptionOnError, ExceptionURL, ClientIP, ErrorLocation, HostAdd, BrowserName, OSString;

        public static void Errorlog(Exception ex)
        {
            var line = Environment.NewLine + Environment.NewLine;
            ErrorLineNo = ex.StackTrace.Substring(ex.StackTrace.Length - 7, 7);
            ErrorMessage = ex.GetType().Name.ToString();
            ExceptionOnError = ex.GetType().ToString();
            ExceptionURL = context.Current.Request.Url.ToString();
            ErrorLocation = ex.Message.ToString();
            BrowserName = HttpContext.Current.Request.Browser.Browser;
            OSString = HttpContext.Current.Request.UserAgent;
            string clientIP = HttpContext.Current.Request.UserHostAddress;

            try
            {
                string MM = DateTime.Now.ToString("MMMM");
                string DD = DateTime.Now.ToString("dd");
                string YYYY = DateTime.Now.ToString("yyyy");
                string name = "DBError.log";
                string date2 = DateTime.Now.ToString("MM-dd-yyyy");
                string directoryPath = context.Current.Server.MapPath("~/event/") + YYYY + "\\" + MM + "\\" + DD;

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                using (StreamWriter sw = File.AppendText(directoryPath + "\\" + name))
                {
                    string error = "Log on Date:" + " " + DateTime.Now.ToString() + line + "Error Line Number :" + " " + ErrorLineNo + line + "Exception Error Message:" + " " + ErrorMessage + line + "Error Exception Type:" + " " + ExceptionOnError + line + "Error Location :" + " " + ErrorLocation + line + "Error ON URL:" + " " + ExceptionURL + line + "Client IP:" + " " + ClientIP + line + "Browser:" + " " + BrowserName + line + "OS Name :" + " " + OSString + line + "Client IP:" + clientIP + line;
                    sw.WriteLine("-----------Exception Details on " + " " + DateTime.Now.ToString() + "-----------------");
                    sw.WriteLine("-------------------------------------------------------------------------------------");
                    sw.WriteLine(line);
                    sw.WriteLine(error);
                    sw.WriteLine("--------------------------------*End*------------------------------------------");
                    sw.WriteLine(line);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                e.ToString();

            }
        }
    }

    //message send
    public class MessageRec
    {
        public static void Tracrt(string msg, string recnumber, string msgstatus)
        {
            try
            {
                string ReqURL = context.Current.Request.Url.ToString();

                string clientIP = HttpContext.Current.Request.UserHostAddress;
                string MM = DateTime.Now.ToString("MMMM");
                string DD = DateTime.Now.ToString("dd");
                string YYYY = DateTime.Now.ToString("yyyy");
                string name = "SuccessLog.log";
                string date = DateTime.Now.ToString("MM-dd-yyyy");
                string directoryPath = context.Current.Server.MapPath("~/event/") + YYYY + "\\" + MM + "\\" + DD;
                if (!File.Exists(directoryPath))
                {
                    //log path create           
                    Directory.CreateDirectory(directoryPath);
                }
                using (StreamWriter sw = File.AppendText(directoryPath + "\\" + name))
                {
                    string time = DateTime.Now.ToString("HH:mm:ss");
                    string Date = DateTime.Now.ToString("MM-dd-yyyy");
                    string hostname = Environment.MachineName;                    
                    string TraceLogs = "Client IP:\t\t"+clientIP + Environment.NewLine + "Date:\t\t" + date+ Environment.NewLine + "Time:\t\t" + time + Environment.NewLine + "Req URL:\t\t"+ReqURL +Environment.NewLine+ "Recerver Number:\t" + recnumber + Environment.NewLine + "Message:\t\t" + msg + Environment.NewLine + "Message State:\t\t" + msgstatus + Environment.NewLine+ "\n--------------------------------------------------------------------------------------------" + Environment.NewLine;
                    sw.WriteLine(TraceLogs);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorLogRecord.ErrorLogWrite(ex);
            }
        }
    }
    //message error
    public class MessageRecErr
    {
        public static void Tracrt(string msg, string recnumber, string msgstatus)
        {
            try
            {
                string ReqURL = context.Current.Request.Url.ToString();
                string clientIP = HttpContext.Current.Request.UserHostAddress;
                string MM = DateTime.Now.ToString("MMMM");
                string DD = DateTime.Now.ToString("dd");
                string YYYY = DateTime.Now.ToString("yyyy");
                string name = "error.log";
                string date = DateTime.Now.ToString("MM-dd-yyyy");
                string directoryPath = context.Current.Server.MapPath("~/event/") + YYYY + "\\" + MM + "\\" + DD;
                if (!File.Exists(directoryPath))
                {       
                    Directory.CreateDirectory(directoryPath);
                }
                using (StreamWriter sw = File.AppendText(directoryPath + "\\" + name))
                {
                    string time = DateTime.Now.ToString("HH:mm:ss");
                    string Date = DateTime.Now.ToString("MM-dd-yyyy");
                    string hostname = Environment.MachineName;
                    string TraceLogs = "Client IP:\t\t" + clientIP + Environment.NewLine + "Date:\t\t" + date + Environment.NewLine + "Time:\t\t" + time + Environment.NewLine + "Req URL:\t\t" + ReqURL + Environment.NewLine + "Recerver Number:\t" + recnumber + Environment.NewLine + "Message:\t\t" + msg + Environment.NewLine + "Message State:\t\t" + msgstatus + Environment.NewLine + "\n--------------------------------------------------------------------------------------------" + Environment.NewLine;
                    sw.WriteLine(TraceLogs);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorLogRecord.ErrorLogWrite(ex);
            }
        }
    }

    //response log
    public class MessageResp
    {
        public static void Res(string msg, string recnumber, string msgstatus)
        {
            try
            {
                string ReqURL = context.Current.Request.Url.ToString();
                string clientIP = HttpContext.Current.Request.UserHostAddress;
                string MM = DateTime.Now.ToString("MMMM");
                string DD = DateTime.Now.ToString("dd");
                string YYYY = DateTime.Now.ToString("yyyy");
                string name = "Response.log";
                string date = DateTime.Now.ToString("MM-dd-yyyy");
                string directoryPath = context.Current.Server.MapPath("~/event/") + YYYY + "\\" + MM + "\\" + DD;
                if (!File.Exists(directoryPath))
                { 
                 Directory.CreateDirectory(directoryPath);
                }
                using (StreamWriter sw = File.AppendText(directoryPath + "\\" + name))
                {
                    string time = DateTime.Now.ToString("HH:mm:ss");
                    string Date = DateTime.Now.ToString("MM-dd-yyyy");
                    string hostname = Environment.MachineName;
                    string TraceLogs = "Client IP:\t\t" + clientIP + Environment.NewLine + "Date:\t\t" + date + Environment.NewLine + "Time:\t\t" + time + Environment.NewLine + "Req URL:\t\t" + ReqURL + Environment.NewLine + "Recerver Number:\t" + recnumber + Environment.NewLine + "Message:\t\t" + msg + Environment.NewLine + "Message State:\t\t" + msgstatus + Environment.NewLine + "\n--------------------------------------------------------------------------------------------" + Environment.NewLine;
                    sw.WriteLine(TraceLogs);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorLogRecord.ErrorLogWrite(ex);
            }
        }
    }
}