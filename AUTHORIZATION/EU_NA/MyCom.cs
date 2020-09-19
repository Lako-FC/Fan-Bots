using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace AUTHORIZATION
{
    public class MyCom
    {
        private readonly int ProjectId = 2000076;
        private int ShardId;
        private readonly int ChannelId = 35;

        public string[] SXml(string request, string URL)
        {
            string soapResult;
            try
            {
                string xmlRequest = request;
                HttpWebRequest webRequest = WebRequest.Create(URL) as HttpWebRequest;
                webRequest.Timeout = 30000;
                webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = xmlRequest.Length;
                webRequest.UserAgent = "Downloader/1950";
                webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream());
                requestWriter.Write(xmlRequest);
                requestWriter.Close();
                StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                soapResult = responseReader.ReadToEnd();
                responseReader.Close();
                webRequest.GetResponse().Close();
            }
            catch (Exception er) { return new[] { "false", "Ошибка в функции 'SubmitXML': \n" + er }; };
            return new[] { "true", soapResult };
        }

        /// <summary>
        /// Авторизация
        /// </summary>
        /// <param name="login">Логин.</param>
        /// <param name="password">Пароль.</param>
        /// <returns>Результат авторизации.</returns>
        public string[] AuthMyCom(string login, string password, byte s)
        {
            if (s == 1) ShardId = 1;
            else if (s == 2) ShardId = 2;

            string[] res = SXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Auth Username=\"" + login + "\" Password=\"" + password + "\" ChannelId=\"" + ChannelId + "\"/>", "https://authdl.my.com/mygc.php?hint=Auth");

            Regex SessionKeyRG = new Regex("SessionKey=\"(.*)\" Email=");
            string SessionKey = SessionKeyRG.Match(res[1]).Groups[1].Value;

            if (SessionKey == "") return new[] { "false", "Ошибка авторизации.." };

            res = SXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Login SessionKey=\"" + SessionKey + "\" ProjectId=\"" + ProjectId + "\" ShardId=\"" + ShardId + "\"/>", "https://authdl.my.com/mygc.php?hint=Login");

            Regex ReasonRG = new Regex("Reason=\"(.*)\" ReasonDE=");
            string reason = ReasonRG.Match(res[1]).Groups[1].Value.Replace("br/", "\n\n").Replace("&lt;", "").Replace("&lt;", "").Replace("&gt;", "").Replace("strong", "").Replace("/", "").Replace("&quot;", "\"");

            Regex GameAccountRG = new Regex("GameAccount=\"(.*)\" Expires=");
            string userID = GameAccountRG.Match(res[1]).Groups[1].Value;

            Regex CodeRG = new Regex("Code=\"(.*)\" GameAccount=");
            string token = CodeRG.Match(res[1]).Groups[1].Value;

            if (userID == "" || token == "") return new[] { "false", reason };
            else return new[] { "true", userID, token };
        }
    }
}
