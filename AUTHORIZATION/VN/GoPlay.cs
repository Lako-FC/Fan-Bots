using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AUTHORIZATION
{
    public class GoPlay
    {
        private readonly int cpId = 1000;
        private readonly string 
            client_id = "AAER47Ux4Yb1BCeoPGxODVEjGq25cKwOOklTHEIE", 
            client_secret = "7YzKxfLpp3HYnyQY0HMeRXE8ijIblNsJ5adnABe3O0iHvAdnAClQRXs3vcAoMu", 
            redirect_uri = "http://goplay.vn";

        /// <summary>
        /// Отправка POST запроса.
        /// </summary>
        private string POST(string Url, string Data)
        {
            WebRequest req = WebRequest.Create(Url);
            req.Method = "POST";
            req.Timeout = 30000;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Headers.Add("useragent: libcurl-agent/1.0");
            byte[] sentData = Encoding.GetEncoding(1251).GetBytes(Data);
            req.ContentLength = sentData.Length;
            Stream sendStream = req.GetRequestStream();
            sendStream.Write(sentData, 0, sentData.Length);
            sendStream.Close();
            WebResponse res = req.GetResponse();
            Stream ReceiveStream = res.GetResponseStream();
            StreamReader sr = new StreamReader(ReceiveStream, Encoding.UTF8);
            char[] read = new char[256];
            int count = sr.Read(read, 0, 256);
            string Out = string.Empty;
            while (count > 0)
            {
                string str = new string(read, 0, count);
                Out += str;
                count = sr.Read(read, 0, 256);
            }
            return Out;
        }
        /// <summary>
        /// Отправка GET запроса.
        /// </summary>
        private string GET(string Url, string Data)
        {
            WebRequest req = WebRequest.Create(Url + "?" + Data);
            req.Headers.Add("useragent: libcurl-agent/1.0");
            WebResponse resp = req.GetResponse();
            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string Out = sr.ReadToEnd();
            sr.Close();
            return Out;
        }
        private static string MD5_Hash(string text)
        {
            byte[] hash = Encoding.ASCII.GetBytes(text);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hashenc = md5.ComputeHash(hash);
            string result = "";
            foreach (byte b in hashenc) result += b.ToString("x2");
            return result;
        }

        /// <summary>
        /// Авторизация
        /// </summary>
        /// <param name="login">Логин.</param>
        /// <param name="password">Пароль.</param>
        /// <returns>Результат авторизации.</returns>
        public string[] AuthGoPlay(string login, string password)
        {
            string CID = "?client_id=" + client_id + "&client_secret=" + client_secret;
            string ip = GET("http://share.goplay.vn/Launcherservice/checkip.aspx", "");

            string l = "username=" + login;

            string SaltRes = POST("http://billing.graph.go.vn/authentication/salt" + CID, l);
            Regex _codeRG = new Regex("_code\":(.*),\"_data");
            string _code = _codeRG.Match(SaltRes.ToString()).Groups[1].Value;
            Regex _messageRG = new Regex("_message\":\"(.*)\"}");
            string _message = _messageRG.Match(SaltRes.ToString()).Groups[1].Value;

            if (_code == "1")
            {
                Regex _dataRG = new Regex("_data\":\"(.*)\",");
                string _data = _dataRG.Match(SaltRes.ToString()).Groups[1].Value;

                string hashed_pwd = MD5_Hash(login + MD5_Hash(password) + _data);

                l = "username=" + login + "&password=" + hashed_pwd + "&cpId=" + cpId + "&ip=" + ip;
                string AuthRes = POST("http://billing.graph.go.vn/authentication/login" + CID, l);

                Regex _codeRG2 = new Regex("_code\":(.*),\"_data");
                string _code2 = _codeRG2.Match(AuthRes.ToString()).Groups[1].Value;
                Regex _messageRG2 = new Regex("_message\":\"(.*)\"}");
                string _message2 = _messageRG2.Match(AuthRes.ToString()).Groups[1].Value;

                if (_code2 == "1")
                {
                    Regex _codeAuthRG = new Regex("code\":\"(.*)\",");
                    string _codeAuth = _codeAuthRG.Match(AuthRes.ToString()).Groups[1].Value;

                    l = "code=" + _codeAuth + "&redirect_uri=" + redirect_uri + "&clinet_id=" + client_id + "&client_secret=" + client_secret;
                    string TokenRes = POST("http://billing.graph.go.vn/oauth/access_token" + CID, l);

                    Regex tokenRG = new Regex("access_token\":\"(.*)\",\"code");
                    string token = tokenRG.Match(TokenRes.ToString()).Groups[1].Value;

                    if (token == "")
                    {
                        Regex mRG = new Regex("_message\":\"(.*)\"}");
                        string m = mRG.Match(TokenRes.ToString()).Groups[1].Value;

                        return new[] { "false", m };
                    }
                    else
                    {
                        Regex UserIdRG = new Regex("UserId\":(.*),\"");
                        string userID = UserIdRG.Match(TokenRes.ToString()).Groups[1].Value;

                        if (userID == "") return new[] { "false", "userID == NULL" };
                        else return new[] { "true", userID, token };
                    }
                }
                else return new[] { "false", _message2 };
            }
            else return new[] { "false", _message };
        }
    }
}
