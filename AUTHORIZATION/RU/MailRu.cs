using ElTennGenLocalization;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Button = System.Windows.Forms.Button;

namespace AUTHORIZATION
{
    public class MailRu
    {
        public static bool Logging = true;
        public string[] AuthMailRu(string Login, string Password, Button button, byte Server)
        {
            try
            {
                try { if (File.Exists("MailRuAuthLog.log")) File.Delete("MailRuAuthLog.log"); } catch { } //Временная заглушка

                ButtonWorked(button, false);

                string mc = "", sdcs = "";
                CookieCollection CookiesStorage = new CookieCollection();

                #region MailRu Auth

                if (Login.ToLower().Contains("@mail.ru") || Login.ToLower().Contains("@inbox.ru") || Login.ToLower().Contains("@list.ru") || Login.ToLower().Contains("@bk.ru"))
                {
                    //Getting state Cookie and ClientID
                    string state;
                    string clientid;
                    using (ResponseContent Response = SendRequest("https://auth-ac.my.games/social/mailru", RequestMethods.Auto, true, CookiesStorage))
                    {
                        clientid = Regex.Match(Response.Body, "client_id=([^&]+)").Groups[1].Value;
                        state = Regex.Match(Response.Body, "state=([^&]+)").Groups[1].Value;
                        if (string.IsNullOrEmpty(state) || string.IsNullOrEmpty(clientid)) throw new Exception(L10n.initialstatetokens_error);
                    }

                    //Getting act and mrcu Cookies
                    SendRequest("https://account.mail.ru", RequestMethods.Auto, true, CookiesStorage).Dispose();
                    if (string.IsNullOrEmpty(CookiesStorage["act"]?.Value) || string.IsNullOrEmpty(CookiesStorage["mrcu"]?.Value)) throw new Exception(L10n.initialbindingtokens_error);

                    //MailRu OAuth Step
                    string OAuthContent = $"Login={Login}&Password={Password}";
                    using (ResponseContent OAuthResponse = SendRequest("https://auth.mail.ru/cgi-bin/auth", RequestMethods.Auto, false, CookiesStorage, OAuthContent))
                    {
                        //check for login fail
                        if (OAuthResponse["Location"].Contains("fail=1")) throw new Exception(L10n.wrongpassword);

                        //check for two factor
                        if (OAuthResponse["Location"].Contains("secstep"))
                        {
                            #region Two Factor Auth

                            string secstep = OAuthResponse.Cookies["secstep"]?.Value;
                            if (string.IsNullOrEmpty(secstep)) throw new Exception(L10n.secstepempty_error);

                            bool PassedWithCache = false;
                            if (File.Exists("mailru-two-factor"))
                            {
                                //check for exists 2fa cache
                                IniFile MRTF = new IniFile("mailru-two-factor");
                                if (MRTF.KeyExists("Mpop", Login) && MRTF.KeyExists("ssdc", Login))
                                {
                                    CookiesStorage.Add(new Cookie("Mpop", MRTF.Read("Mpop", Login), "/", ".auth.mail.ru"));
                                    CookiesStorage.Add(new Cookie("ssdc", MRTF.Read("ssdc", Login), "/", ".auth.mail.ru"));

                                    //try to login with cache info
                                    using (ResponseContent Response = SendRequest("https://auth.mail.ru/cgi-bin/auth",
                                        RequestMethods.Auto, false, CookiesStorage, OAuthContent))
                                    {
                                        if (!string.IsNullOrEmpty(Response.Cookies["Mpop"]?.Value) && !string.IsNullOrEmpty(Response.Cookies["ssdc"]?.Value))
                                        {
                                            MRTF.Write("Mpop", Response.Cookies["Mpop"]?.Value, Login);
                                            MRTF.Write("ssdc", Response.Cookies["ssdc"]?.Value, Login);
                                            PassedWithCache = true;
                                        }
                                    }
                                }
                            }

                            if (!PassedWithCache)
                            {
                                //Starting 2fa form
                                MailRuTF MailRuTF = new MailRuTF(Login, CookiesStorage);
                                MailRuTF.ShowDialog();
                                if (!MailRuTF.Passed)
                                {
                                    throw new Exception(L10n.userabandone2fa);
                                }
                            }

                            #endregion
                        }
                    }

                    if (string.IsNullOrEmpty(CookiesStorage["Mpop"]?.Value) || string.IsNullOrEmpty(CookiesStorage["ssdc"]?.Value) ||
                        string.IsNullOrEmpty(CookiesStorage["ssdc_info"]?.Value))
                    {
                        throw new Exception(L10n.oauthtokens_error);
                    }

                    if (string.IsNullOrEmpty(CookiesStorage["o2csrf"]?.Value))
                    {
                        throw new Exception(L10n.autherror.Replace("%%var%%", "o2csrf"));
                    }

                    // MailRu Login
                    void MailRuLogin(bool TryToBind)
                    {
                        using (ResponseContent Response = SendRequest(
                           "https://o2.mail.ru/login?" +
                           $"client_id={clientid}&" +
                           "response_type=code&" +
                           $"redirect_uri={WebUtility.UrlEncode("https://auth-ac.my.games/social/mailru_callback/")}" +
                           $"&state={state}&" +
                           $"login={Login}",
                           RequestMethods.Auto, true, CookiesStorage))
                        {
                            //login stopped
                            if (Response.WebResponse.ResponseUri.ToString() != "https://my.games/")
                            {
                                //check for block
                                Match BlockedMatch = Regex.Match(Response.Body, @"blocked *= *(\d)");
                                Match CaptchaMatch = Regex.Match(Response.Body, @"captcha *= *(\d)");
                                if (BlockedMatch.Groups[1].Value == "1")
                                {
                                    throw new Exception(L10n.reqsblocked_error);
                                }

                                if (CaptchaMatch.Groups[1].Value == "1")
                                {
                                    throw new Exception(L10n.captchablock_error);
                                }

                                //Maybe mailru account has not binded to my.games
                                //trying to bind
                                if (!TryToBind)
                                {
                                    return;
                                }

                                using (ResponseContent AccBind = SendRequest("https://o2.mail.ru/login", RequestMethods.Auto, false, CookiesStorage, $"o2csrf={CookiesStorage["o2csrf"]?.Value}&login={Login}"))
                                {
                                    BlockedMatch = Regex.Match(AccBind.Body, @"blocked *= *(\d)");
                                    CaptchaMatch = Regex.Match(AccBind.Body, @"captcha *= *(\d)");
                                    if (BlockedMatch.Groups[1].Value == "1")
                                    {
                                        throw new Exception($"{L10n.reqsblocked_error}\n{L10n.autoaccbindblock_error}");
                                    }

                                    if (CaptchaMatch.Groups[1].Value == "1")
                                    {
                                        throw new Exception($"{L10n.captchablock_error}\n{L10n.autoaccbindblock_error}");
                                    }

                                    MailRuLogin(false);
                                }
                            }
                        }
                    }
                    MailRuLogin(true);

                    //Getting SDCS Token
                    SendRequest($"https://auth-ac.my.games/sdc?from={WebUtility.UrlEncode("https://api.my.games/social/profile/session")}",
                                RequestMethods.Auto, true, CookiesStorage).Dispose();

                    foreach (Cookie Cookie in CookiesStorage)
                    {
                        if (Cookie.Domain.Contains("my.games"))
                        {
                            if (Cookie.Name == "sdcs")
                            {
                                sdcs = Cookie.Value;
                            }

                            if (Cookie.Name == "mc")
                            {
                                mc = Cookie.Value;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(sdcs))
                    {
                        throw new Exception(L10n.autherror.Replace("%%var%%", "sdcs"));
                    }

                    if (string.IsNullOrEmpty(mc))
                    {
                        throw new Exception(L10n.autherror.Replace("%%var%%", "mc"));
                    }
                }
                else throw new Exception("E-mail not supported");

                #endregion MailRu Auth

                #region Game Auth

                string GameAccount, GameToken;
                int ProjectId = 1177, ChannelId = 35;

                //Creating my.games session
                string SessionKey;
                using (ResponseContent Response = SendRequest("https://authdl.my.games/gem.php?hint=Auth",
                    RequestMethods.Auto, false, null,
                    $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><Auth mc=\"{mc}\" sdcs=\"{sdcs}\" ChannelId=\"{ChannelId}\"/>")
                )
                {
                    //check if EULA not accepted
                    if (!Response.Body.Contains("TermsAccepted"))
                    {
                        void AcceptTerms()
                        {
                            //Getting EULA token
                            using (ResponseContent Response1 = SendRequest("https://api.my.games/social/profile/session",
                                RequestMethods.GET, false, CookiesStorage))
                            {
                                if (string.IsNullOrEmpty(Response1.Body))
                                    throw new Exception(L10n.termstoken_error.Replace("%%var%%", "Response1"));

                                string csrfmiddlewaretoken_jwt;
                                if (string.IsNullOrEmpty(csrfmiddlewaretoken_jwt = Regex.Match(Response1.Body, "token\":\"([^\"]+)").Groups[1].Value))
                                    throw new Exception(L10n.termstoken_error.Replace("%%var%%", "csrfmiddlewaretoken_jwt"));

                                //Accepting Terms
                                using (ResponseContent Response2 = SendRequest(
                                    "https://api.my.games/account/terms_accept/",
                                    RequestMethods.POST, false, CookiesStorage,
                                    $"csrfmiddlewaretoken_jwt={csrfmiddlewaretoken_jwt}&csrfmiddlewaretoken="))
                                {
                                    if (!Response2.Body.Contains("\"descr\": \"OK\"")) throw new Exception(L10n.autoacceptterms_error);
                                }
                            }
                        }

                        DialogResult Dialog = MessageBox.Show(L10n.notermsaccepted, "EULA Not Accepted", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                        if (Dialog == DialogResult.Yes)
                        {
                            Process.Start("https://documentation.my.games/terms/mygames_tou");
                            Process.Start("https://documentation.my.games/terms/mygames_privacy");
                            Process.Start("https://documentation.my.games/terms/mygames_cookies");
                            Process.Start("https://documentation.my.games/terms/mygames_eula");
                            Process.Start("https://documentation.my.games/terms/mygames_app_eula");

                            if (MessageBox.Show(L10n.autoacceptterms, "EULA Not Accepted", MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                AcceptTerms();
                            }
                            else
                            {
                                throw new Exception(L10n.userabandoneterms);
                            }
                        }
                        else if (Dialog == DialogResult.No)
                        {
                            AcceptTerms();
                        }
                        else
                        {
                            throw new Exception(L10n.userabandoneterms);
                        }
                    }

                    //Getting SessionKey
                    if (string.IsNullOrEmpty(SessionKey = Regex.Match(Response.Body, "SessionKey=\"([^\"]+)").Groups[1].Value))
                    {
                        throw new Exception(L10n.sessionkey_error);
                    }
                }

                //Registration Session to Portal
                using (ResponseContent Response = SendRequest("https://authdl.my.games/gem.php?hint=Portal",
                    RequestMethods.Auto, false, null,
                    $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><Portal SessionKey=\"{SessionKey}\" Url=\"http://authdl.my.games/robots.txt\"/>")
                )
                {
                    string RedirectUrl;
                    if (string.IsNullOrEmpty(RedirectUrl = Regex.Match(Response.Body, "RedirectUrl=\"([^\"]+)").Groups[1].Value.Replace("&amp;", "&")))
                        throw new Exception(L10n.portal_sesreg_error);

                    SendRequest(RedirectUrl, RequestMethods.Auto, false, null).Dispose();
                }

                //Login to game project
                using (ResponseContent Response = SendRequest("https://authdl.my.games/gem.php?hint=Login",
                    RequestMethods.Auto, false, null,
                    $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><Login SessionKey=\"{SessionKey}\" ProjectId=\"{ProjectId}\" ShardId=\"{Server}\"/>")
                )
                {
                    if (BanReason(Response.Body))
                        throw new Exception(L10n.account_banned);

                    if (Response.Body.Contains("ErrorCode=\"447\""))
                        throw new Exception(L10n.login_error_447);

                    if (string.IsNullOrEmpty(GameAccount = Regex.Match(Response.Body, "GameAccount=\"([^\"]+)").Groups[1].Value))
                        throw new Exception(L10n.autherror.Replace("%%var%%", "GameAccount"));

                    if (string.IsNullOrEmpty(GameToken = Regex.Match(Response.Body, "Code=\"([^\"]+)").Groups[1].Value))
                        throw new Exception(L10n.autherror.Replace("%%var%%", "GameToken"));
                }

                #endregion Getting Game Tokens

                ButtonWorked(button, true);
                return new[] { "true", GameAccount, GameToken };
            }
            catch (Exception e)
            {
                ButtonWorked(button, true);
                return new[] { "false", e.Message };
            }
        }

        #region Requests Engine

        public enum RequestMethods
        {
            Auto = 0,
            GET = 1,
            POST = 2
        }

        public static object RequestLock = new object();
        public static ResponseContent SendRequest(string URL, RequestMethods Method, bool Redirecting, CookieCollection Cookies, string Content = null)
        {
            lock (RequestLock)
            {
                HttpWebRequest Request = WebRequest.CreateHttp(URL);
                Request.Method = Method == RequestMethods.Auto ? string.IsNullOrEmpty(Content) ? "GET" : "POST" : Method == RequestMethods.GET ? "GET" : "POST";
                Request.ContentType = "application/x-www-form-urlencoded";

                if (Cookies != null)
                {
                    Request.CookieContainer = new CookieContainer();
                    Request.CookieContainer.Add(Cookies);
                }

                Request.AllowAutoRedirect = false;
                Request.Accept = "*/*";
                Request.UserAgent = "Downloader/15740";
                Request.Timeout = 30000;

                if (Content != null)
                {
                    Request.ContentLength = Content.Length;

                    byte[] Bytes = Encoding.UTF8.GetBytes(Content);
                    using (Stream RequestStream = Request.GetRequestStream()) RequestStream.Write(Bytes, 0, Bytes.Length);

                    Thread.Sleep(50);
                }

                ResponseContent Response = new ResponseContent(Request.GetResponse());

                //Log This Request
                if (Logging)
                {
                    string Log = $"Log - [{DateTime.Now}]\n{{\n" +
                                 $"    [{Request.Method}]\n" +
                                 "    Request {\n" +
                                 $"        URL = {Convert.ToBase64String(Encoding.UTF8.GetBytes(FilterURLForLog(URL)))}\n" +
                                 $"{(string.IsNullOrEmpty(Content) ? null : $"        Content = {Convert.ToBase64String(Encoding.UTF8.GetBytes(FilterContentForLog(Content)))}\n")}" +
                                 $"        Redirecting = {Redirecting}\n" +
                                 $"{GetCookiesForLog(Cookies, URL, 3)}" +
                                 "    }\n" +
                                 "    Response {\n" +
                                 $"{(Response.Content.Length == 0 ? null : $"        Content = {Convert.ToBase64String(Encoding.UTF8.GetBytes(FilterContentForLog(Response.Body)))}\n")}" +
                                 $"{(string.IsNullOrEmpty(Response["Location"]) ? null : $"        Location = {Convert.ToBase64String(Encoding.UTF8.GetBytes(FilterURLForLog(Response["Location"])))}\n")}" +
                                 $"{GetCookiesForLog(Response.Cookies, URL, 3)}" +
                                 "    }\n" +
                                 "}\n";
                    try { File.AppendAllText("MailRuAuthLog.log", Convert.ToBase64String(Encoding.UTF8.GetBytes(Log)) + Environment.NewLine); } catch { } //Временная заглушка
                }

                if (Cookies != null) foreach (Cookie Cookie in Response.Cookies) Cookies.Add(Cookie);

                if (Redirecting && !string.IsNullOrEmpty(Response["Location"]))
                    Response = SendRequest(Response["Location"], Method, true, Cookies, Content);

                return Response;
            }
        }

        private static string FilterContentForLog(string Content)
        {
            Match LogPassMatch = Regex.Match(Content, "Login=([^&]+)&Password=([^ ]+)");
            if (LogPassMatch.Success) Content = Content.Replace(LogPassMatch.Groups[1].Value, "**HiddenLogin**")
                                                       .Replace(LogPassMatch.Groups[2].Value, "**HiddenPassword**");

            foreach (Match Match in Regex.Matches(Content, "\"(user_id|login|alt_email|nickname|nick|token)\":([^,]+)"))
                if (Match.Success)
                    Content = Content.Replace(Match.Groups[2].Value, $"**Hidden_{Match.Groups[1].Value}{(Match.Groups[1].Value == "token" ? $"({Match.Groups[2].Value.Length})" : null)}**");

            foreach (Match Match in Regex.Matches(Content, "(sdcs|mc|SessionKey|GameAccount|Uid|Username|NickName|Code|PersId|state|token)=\"([^\"]+)\""))
                if (Match.Success)
                    Content = Content.Replace(Match.Groups[2].Value, $"**Hidden_{Match.Groups[1].Value}({Match.Groups[2].Value.Length})**");

            foreach (Match Match in Regex.Matches(Content, "(state|token)=([^&%]+)"))
                if (Match.Success)
                    Content = Content.Replace(Match.Groups[2].Value, $"**Hidden_{Match.Groups[1].Value}({Match.Groups[2].Value.Length})**");

            return Content;
        }

        private static string FilterURLForLog(string URL)
        {
            foreach (Match Match in Regex.Matches(URL, "(state|token|login|code)(=|%3D)([^&% \n]+)"))
                if (Match.Success) URL = URL.Replace(Match.Groups[3].Value, $"**Hidden_{Match.Groups[1].Value}({Match.Groups[3].Value.Length})**");

            return URL;
        }

        private static string GetCookiesForLog(CookieCollection Cookies, string URL, int Tabs)
        {
            if (Cookies == null) return null;

            Match Match = Regex.Match(URL, @"(http)?s?(:\/\/)?([^?\/]+)");
            string Domain = Match.Groups[3].Value;
            if (string.IsNullOrEmpty(Domain)) return null;

            Domain = "." + Domain;

            string CookiesMassive = "";
            foreach (Cookie Cookie in Cookies)
            {
                if (Domain.Contains(Cookie.Domain))
                {
                    string CookieString = $"{Cookie.Name} | " +
                                          $"{(Cookie.Name == "secstep" || Cookie.Name == "Mpop" || Cookie.Name == "ssdc" || Cookie.Name == "state" || Cookie.Name == "sdcs" || Cookie.Name == "mc" ? $"**HiddenCookie_{Cookie.Name}({Cookie.Value.Length})**" : Cookie.Value)}" +
                                          $" | {Cookie.Path} | {Cookie.Expires}";
                    CookiesMassive += $"{tabsgen(Tabs)}" + $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(CookieString))}\n";
                }
            }

            string tabsgen(int tabsc)
            {
                string tabs = "";
                for (int x = 0; x < tabsc; x++) tabs += "    ";

                return tabs;
            }

            if (!string.IsNullOrEmpty(CookiesMassive))
            {
                CookiesMassive = $"{tabsgen(Tabs - 1)}Cookies\n{tabsgen(Tabs - 1)}{{\n{CookiesMassive}{tabsgen(Tabs - 1)}}}\n";
            }

            return CookiesMassive;
        }

        public class ResponseContent : IDisposable
        {
            public ResponseContent(WebResponse WebResponse)
            {
                this.WebResponse = WebResponse;

                using (Stream ResponseStream = WebResponse.GetResponseStream())
                {
                    if (ResponseStream == null) Content = new byte[0];
                    else
                    {
                        using (MemoryStream MemoryStream = new MemoryStream())
                        {
                            ResponseStream.CopyTo(MemoryStream);
                            Content = MemoryStream.ToArray();
                        }
                    }
                }
            }

            public readonly WebResponse WebResponse;
            public HttpWebResponse HttpWebResponse => (HttpWebResponse)WebResponse;
            public string Body => Encoding.UTF8.GetString(Content);
            public byte[] Content;
            public string this[string Header] => HttpWebResponse.GetResponseHeader(Header);
            public CookieCollection Cookies => HttpWebResponse.Cookies;
            public string CookiesString => this["Set-Cookie"];

            public void Dispose() => WebResponse?.Dispose();
        }

        #endregion

        private bool BanReason(string Response)
        {
            if (Response.Contains("ErrorCode=\"793\""))
            {
                try
                {
                    string BanUrl = Regex.Match(Response, "SupportUrl=\"([^\"]+)").Groups[1].Value;
                    using (ResponseContent BanContent =
                        SendRequest("https://ruwf.my.games/dynamic/gamecenter/?a=unban&do=template",
                            RequestMethods.Auto, false, null,
                            Regex.Match(BanUrl, "(uid=.*)").Groups[1].Value.Replace("&amp;", "&")))
                    {
                        string Header = Regex.Match(BanContent.Body, "<div class=\"text\">([^<]+)<\\/div>").Groups[1].Value;
                        string Reason = Regex.Matches(BanContent.Body, "<h3>([^<]+)<\\/h3>")[1].Groups[1].Value;
                        MatchCollection Period = Regex.Matches(BanContent.Body.Replace("<b>", "").Replace("</b>", ""), "<h4>([^<]+)<\\/h4>");
                        string Date = Period[0].Groups[1].Value;
                        string Time = Period[1].Groups[1].Value;

                        MessageBox.Show($"{Header}\n{Reason}\n\n{Date}\n{Time}", Header);
                    }

                    return true;
                }
                catch (Exception e) { throw new Exception(L10n.banreason_error + Environment.NewLine + e.Message); }
            }
            return false;
        }

        #region Button Switching

        private void ButtonWorked(Button button, bool tmp)
        {
            button.Invoke((MethodInvoker)delegate
            {
                button.Enabled = tmp;
                button.Text = L10n.button2;
            });
        }

        #endregion
    }
}
