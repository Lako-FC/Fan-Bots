using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace START
{
    public partial class One_Message : Form
    {
        #region Переменные
        private string no_ok = "Вы не согласились с условиями, значит вы не можете использовать Fan-Bot's\n(вы можете перезапустить Fan-Bot's и изменить своё решение)";
        private byte g_id = 0;
        private int timer_sec = 30;
        private readonly Random random = new Random();

        private readonly List<Color> colors = new List<Color>() { Color.Green, Color.Red, Color.Purple, Color.Coral };
        #endregion

        public One_Message()
        {
            InitializeComponent();
            Size = new Size(478, 376);
            richTextBox_info.ReadOnly = true;
            richTextBox_use_conditions.ReadOnly = true;
        }
        private void Exit(object sender, EventArgs e) => Environment.Exit(0);
        private void ChangePage(byte id)
        {
            g_id = id;
            timer_check.Stop();
            foreach (Panel panel in Controls) panel.Visible = false;

            Point point = new Point(0, 0);
            if (id == 0)
            {
                panel_page_0.Location = point;
                panel_page_0.Visible = true;
                return;
            }
            else if (id == 1)
            {
                panel_page_1.Location = point;
                panel_page_1.Visible = true;
            }
            else if (id == 2)
            {
                panel_page_2.Location = point;
                panel_page_2.Visible = true;
            }
            else if (id == 3)
            {
                if (radioButton_use_yes.Checked)
                {
                    if (File.Exists(Program.one_message)) File.Delete(Program.one_message);
                    Start();
                }
                else Program.MSB_Error(no_ok);

                Close();
            }

            button_next2.Enabled = false;
            panel_page_dp.Location = new Point(-7, 295);
            panel_page_dp.Visible = true;
            label_info_sec.LinkColor = Color.Black;
            label_info_sec.Text = "0";
            timer_sec = 30;
        }
        private void timer_check_Tick(object sender, EventArgs e)
        {
            timer_sec--;

            label_info_sec.LinkColor = colors[random.Next(0, colors.Count)];
            if (timer_sec == 0)
            {
                button_next2.Enabled = true;
                label_info_sec.LinkColor = Color.Black;
                label_info_sec.Text = "0";
                System.Media.SystemSounds.Hand.Play();
                timer_sec = 30;
                button_next2.Enabled = true;
                timer_check.Stop();
            }
            else
            {
                label_info_sec.Text = timer_sec.ToString();
                button_next2.Enabled = false;
            }
        }
        private void radioButton_english_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_english.Checked) Translation_English();
        }
        private void radioButton_russian_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_russian.Checked && button_exit1.Text != "Отмена")
            {
                Process.Start(Application.ProductName + ".exe");
                Environment.Exit(0);
            }
        }

        private void button_next1_Click(object sender, EventArgs e)
        {
            ChangePage(1);
            timer_check.Start();
        }
        private void button_next2_Click(object sender, EventArgs e)
        {
            ChangePage(g_id == 1 ? (byte)2 : (byte)3);
            timer_check.Start();
        }
        private void button_exit2_Click(object sender, EventArgs e)
        {
            ChangePage(g_id == 1 ? (byte)0 : (byte)1);
            timer_check.Stop();
            button_next2.Enabled = true;
        }

        private void Translation_English()
        {
            label_select_lang.Text = "Language";
            label_info.Text = "- is a FREE software that will help Warface players launch alone on maps, " +
                              "for farming victories or different achievements, " +
                              "launching bots that can not only launch, " +
                              "but also execute various commands (read COMMANDS.txt).";
            label_info_funcode.Text = "Belongs to organization: «Fun-Code»";
            button_exit1.Text = "Cancel";
            button_next1.Text = "Next";

            label_info_button.Text = "Button 'Next >' - will be available via:";
            label_sec.Text = "sec";
            button_exit2.Text = "< Back";
            button_next2.Text = "Next >";
            richTextBox_info.Text = "> Use conditions\n" +
            "I understand that:\n" +
            "1. The Developer / publisher is not responsible for my accounts and for blocking these accounts, and is not obligated to do anything.\n" +
            "2. It is Recommended to run Fan-Bot's on a virtual machine (where there is no Warface, Game Center, and anti-cheat mrac) or on a 2nd PC (where there is no Warface, Game Center, and anti-cheat mrac).\n" +
            "3. Fan-Bot's is free and does not give any guarantees, as well as technical support.\n" +
            "4. Fan-Bot's open source code, so you can make sure that there are no viruses, and starting from v3. 0 you have access to the source code on github.\n" +
            "5. Fan-Bot's does not change or touch Warface game files in any way.\n" +
            "6. Fan-Bot's is not sand, you can not farm kills / kills in the game Warface.\n" +
            "7. All responsibility for using Fan-Bot's lies with the Fan-Bot's user.\n" +
            "8. If I sell Fan-Bot's, I will be blocked in the social network.VK networks (in groups), limiting the use of Fan-Bot's and understanding that I was selling for free.\n" +
            "9. If I modify (change the code) Fan-Bot's, I will have to leave a link to the source of the modification (change the code), or I will be blocked in the social network.VK networks (in groups) and will be restricted in using Fan-Bot's.\n" +
            "___________________________________________________________________________________\n" +
            "> License\n" +
            "Fan-Bot's is licensed under the GNU Affero General Public License v3.0";

            label2.Text = "Please read the following important information:";
            radioButton_use_yes.Text = "I agree to the terms of use and license.";
            radioButton_use_no.Text = "I do not agree to the terms of use and license.";
            richTextBox_use_conditions.Text = "> What is Fan-Bot's?\n" +
            "Fan-Bot's is a modification (fork) of the 'warfacebot levak' project, its code is open source and has a source on github, which means that everyone can view the SOFTWARE code and make sure that this SOFTWARE does not have any virus lines of code for the user.\n" +
            "Fan-Bot's free due to the use of 'warfacebot levak' and lack of technical support.\n" +
            "___________________________________________________________________________________\n" +
            "[REMINDER]\n" +
            "1. Now you have 3 options to launch Fan-bot's: a) DANGEROUS B) SAFE C) SAFE+.\n" +
            "A-launch Fan-Bot's on the main machine (PC), where there is Warface, anti-cheat MRAC and IC.\n" +
            "The chance of a ban is high, since you can be burned by signature analysis.\n" +
            "B-launch Fan-Bot's on a virtual machine (on a 2nd PC or on a dedicated server), where Warface, anti-cheat MRAC and IC are NOT present.\n" +
            "The chance of being banned is average, since you can't be burned by signature analysis, but you can be burned by IP.\n" +
            "C-launch Fan-Bot's on a virtual machine (on a 2nd PC or on a dedicated server), where there is no Warface, anti-cheat MRAC and IC, but you will also use a VPN/PROXY.\n" +
            "The chance of a ban is small, since you can't be burned by signature analysis and IP.\n" +
            "2. Fan-Bot's supports Windows 7-10 (x64 & x86 (x32)).\n" +
            "3. We are not responsible for your accounts, as you violate the rules of the game Warface, with which you agreed, all at your OWN RISK!\n" +
            "4. Links to the source code of the modification (wb.exe) and the source codes of the launchers can be obtained in the file: SOURCE_CODE.FANBOT\n" +
            "5. Technical support-no!\n" +
            "___________________________________________________________________________________\n" +
            "> Are account data sent anywhere? Is there a risk of losing your account when using 'Fan-Bot's'?\n" +
            "1) Our software and build does not steal your data, they are only needed for the authorization of bots, the data is stored with you (in the file - accounts.ini).\n" +
            "2) when transferring the build, delete the 'account.ini' file or delete it via the authorization launcher.\n" +
            "___________________________________________________________________________________\n\n" +

            "Recommended email addresses:\n" +
            "@mail.ru / @inbox.ru / @list.ru / @bk.ru / @yandex.ru / @google.ru / @gmail.com\n\n" +

            "[BOTS CAN'T GO INTO THE MATCH ITSELF - IT'S NOT SAND! THEY JUST LAUNCH THE LOBBY!]\n" +
            "[WE ARE NOT RESPONSIBLE FOR YOUR ACCOUNTS! ALL AT YOUR OWN RISK!]\n" +
            "___________________________________________________________________________________\n" +
            "> License\n" +
            "Fan-Bot's is licensed under the GNU Affero General Public License v3.0";

            no_ok = "You didn't agree, so you can't use Fan-Bot's.\nOf course you can restart Fan-Bot's and change your mind.";
        }
        public void Start()
        {
            try
            {
                for (int i = 0; i < Program.files.Length; i++)
                    foreach (Process tmp_process in Process.GetProcessesByName(Program.files[i].Replace(".exe", string.Empty))) tmp_process.Kill();

                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Program.dir,
                    FileName = $@"{Application.StartupPath}\{Program.dir}{Program.files[0]}"
                };
                Process.Start(processStartInfo);
            }
            catch (Exception er) { Program.MSB_Error(er.ToString()); Console.ReadKey(); }
        }
    }
}
