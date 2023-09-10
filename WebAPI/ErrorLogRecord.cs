using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using context = System.Web.HttpContext;

namespace WebAPI
{
    //all error log
    public class ErrorLogRecord
    {
        public static String ErrorLineNo, ErrorMessage, ExceptionOnError, ExceptionURL, ClientIP, ErrorLocation, HostAdd, BrowserName, OSString;

        public static void ErrorLogWrite(Exception ex)
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
                string name = "exception.log";
                string date2 = DateTime.Now.ToString("MM-dd-yyyy");
                string directoryPath = context.Current.Server.MapPath("~/event/") + YYYY + "\\" + MM + "\\" + DD;

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                using (StreamWriter sw = File.AppendText(directoryPath + "\\" + name))
                {
                    string error = "Log on Date:" + " " + DateTime.Now.ToString() + line + "Error Line Number :" + " " + ErrorLineNo + line + "Exception Error Message:" + " " + ErrorMessage + line + "Error Exception Type:" + " " + ExceptionOnError + line + "Error Location :" + " " + ErrorLocation + line + "Error ON URL:" + " " + ExceptionURL + line + "Client IP:" + " " + ClientIP + line + "Browser:" + " " + BrowserName + line + "OS Name :" + " " + OSString + line+"Client IP:" +clientIP + line;
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
}