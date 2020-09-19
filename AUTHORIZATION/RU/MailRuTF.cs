using ElTennGenLocalization;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AUTHORIZATION
{
    public partial class MailRuTF : Form
    {
        public bool Passed;
        private readonly string Login;
        private readonly CookieCollection CookiesStorage;

        public MailRuTF(string Login, CookieCollection CookiesStorage)
        {
            this.Login = Login;
            this.CookiesStorage = CookiesStorage;

            InitializeComponent();
            textBox_login.Text = this.Login;
            WrongCode.Text = L10n.tfawrongcode;
            Application.DoEvents();
            WrongCode.Location = new Point(Width / 2 - WrongCode.Width / 2, WrongCode.Location.Y);
            LoginButton.Text = L10n.button2;
            SaveAccount.Text = L10n.tfasaveaccount;
            label1.Text = L10n.tfaentercode;
            groupBox1.Text = L10n.tfacaptchaenter;

            using (MailRu.ResponseContent Response = MailRu.SendRequest("https://auth.mail.ru/cgi-bin/secstep", MailRu.RequestMethods.Auto, false, CookiesStorage))
            {
                GetCSRFAndCaptcha(Response);
            }
        }

        private string CSRF, RedirectResp;
        private bool CaptchaRequired;
        private void LoginButton_Click(object sender, EventArgs e)
        {
            if (CodeTextBox.Text.Length == 6)
            {
                WrongCode.Visible = false;
                Application.DoEvents();
                Thread.Sleep(100);

                string Content = $"csrf={CSRF}&Login={Login}&AuthCode={CodeTextBox.Text}{(CaptchaRequired ? $"&Captcha={CaptchaTextBox.Text}" : null)}&Permanent=1";
                using (MailRu.ResponseContent Response = MailRu.SendRequest("https://auth.mail.ru/cgi-bin/secstep", MailRu.RequestMethods.Auto, false, CookiesStorage, Content))
                {
                    RedirectResp = Response["Location"];

                    if (!RedirectResp.Contains(Login))
                    {
                        WrongCode.Visible = true;
                        CaptchaTextBox.Clear();
                        CodeTextBox.Clear();
                        Application.DoEvents();
                        GetCSRFAndCaptcha(Response);
                        return;
                    }
                }

                Thread.Sleep(50);

                using (MailRu.ResponseContent Response = MailRu.SendRequest(RedirectResp, MailRu.RequestMethods.Auto, false, CookiesStorage))
                {
                    if (Response.Cookies["Mpop"] == null || Response.Cookies["ssdc"] == null || Response.Cookies["ssdc_info"] == null)
                    {
                        throw new Exception(L10n.oauthtokens_error);
                    }

                    Passed = true;

                    if (SaveAccount.Checked)
                    {
                        IniFile mailru_two_factor = new IniFile("mailru-two-factor");
                        mailru_two_factor.DeleteSection(Login);
                        mailru_two_factor.Write("Mpop", Response.Cookies["Mpop"].Value, Login);
                        mailru_two_factor.Write("ssdc", Response.Cookies["ssdc"].Value, Login);
                    }
                }

                Close();
            }
        }
        private void GetCSRFAndCaptcha(MailRu.ResponseContent Response)
        {
            CSRF = new Regex("\"csrf\":\"([^,]+)\"").Match(Response.Body).Groups[1].Value;
            string Captcha = new Regex("\"secstep_captcha\":\"([^,]+)\"").Match(Response.Body).Groups[1].Value;

            if (!string.IsNullOrEmpty(Captcha))
            {
                using (MailRu.ResponseContent CaptchaResponse = MailRu.SendRequest(Captcha, MailRu.RequestMethods.Auto, false, CookiesStorage))
                {
                    Height = 253;
                    if (CaptchaResponse.Content.Length == 0)
                    {
                        throw new Exception(L10n.tfacaptcha_error);
                    }

                    using (MemoryStream CaptchaStream = new MemoryStream(CaptchaResponse.Content))
                    {
                        pictureBox1.BackgroundImage = Image.FromStream(CaptchaStream);
                        CaptchaRequired = true;
                    }
                }
            }
        }
        private void CodeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoginButton_Click(null, null);
                e.SuppressKeyPress = false;
            }
        }
    }
}