using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace BitTrader
{
    class MtGox : Exchange
    {
        public string _getbalance(string uname, string pass)
        {
            string postData = "name=" + uname + "&pass=" + pass;
            string uri = "https://mtgox.com/code/getFunds.php";
            return HttpPost(uri, postData);
        }


        string HttpPost(string uri, string parameters)
        {
            // parameters: name1=value1&name2=value2	
            //System.Net.ServicePointManager.CertificatePolicy = new MyPolicy();
            ServicePointManager.MaxServicePointIdleTime = 200;
            WebRequest webRequest = WebRequest.Create(uri);
            //webRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US) AppleWebKit/534.21 (KHTML, like Gecko) Chrome/11.0.682.0 Safari/534.21";
            ((HttpWebRequest)webRequest).UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";



            //string ProxyString = 
            //   System.Configuration.ConfigurationManager.AppSettings
            //   [GetConfigKey("proxy")];
            //webRequest.Proxy = new WebProxy (ProxyString, true);
            //Commenting out above required change to App.Config
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(parameters);
            Stream os = null;
            try
            { // send the Post
                webRequest.ContentLength = bytes.Length;   //Count bytes to send
                os = webRequest.GetRequestStream();
                os.Write(bytes, 0, bytes.Length);         //Send it
            }
            catch (WebException ex)
            {
                //MessageBox.Show(ex.Message, "HttpPost: Request error",
                //   MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (os != null)
                {
                    os.Close();
                }
            }

            try
            { // get the response
                WebResponse webResponse = webRequest.GetResponse();
                if (webResponse == null)
                { return null; }
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                return sr.ReadToEnd().Trim();
            }
            catch (WebException ex)
            {
                // MessageBox.Show(ex.Message, "HttpPost: Response error",
                //   MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        public override void UpdateBalance()
        {
            string tmp = _getbalance(UserName, Password);

            double usds, btcs;

            try
            {
                JObject o = JObject.Parse(tmp);

                balance[1] = double.Parse(o["usds"].ToString());
                balance[0] = double.Parse(o["btcs"].ToString());
            }
            catch
            {
                //ttStatus.Text = "TIMEOUT!";
            }
        }

        public override void Connect()
        {
            base.Connect();
            balance = new double[2];
        }
    }
}
