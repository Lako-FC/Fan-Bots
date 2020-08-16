using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AUTHORIZATION
{
#if DEBUG
    internal static class MailRuLogUnpack
    {
        public static void Unpack(bool UnpackContent, bool UnpackCookies)
        {
            if (!File.Exists("
            ")) return;

            string[] Logs = File.ReadAllLines("MailRuAuthLog.log");

            File.Delete("MailRuAuthLog.log");

            foreach (string Log in Logs)
            {
                string PreUnpackedLog = Encoding.UTF8.GetString(Convert.FromBase64String(Log)) + Environment.NewLine;
                foreach (Match Match in Regex.Matches(PreUnpackedLog, @"(URL|Location) = ([^\n]+)"))
                {
                    PreUnpackedLog = PreUnpackedLog.Replace($"{Match.Groups[2].Value}",
                        Encoding.UTF8.GetString(Convert.FromBase64String(Match.Groups[2].Value)));
                }

                if (UnpackContent)
                {
                    foreach (Match Match in Regex.Matches(PreUnpackedLog, @"(Content) = ([^\n]+)"))
                    {
                        PreUnpackedLog = PreUnpackedLog.Replace($"{Match.Groups[2].Value}",
                            Encoding.UTF8.GetString(Convert.FromBase64String(Match.Groups[2].Value)));
                    }
                }

                if (UnpackCookies)
                {
                    foreach (Match Match in Regex.Matches(PreUnpackedLog, "Cookies\\\n *{([^}]+)}"))
                    {
                        foreach (Match Match2 in Regex.Matches(Match.Groups[1].Value, " *(\\S+)"))
                        {
                            PreUnpackedLog = PreUnpackedLog.Replace($"{Match2.Groups[1].Value}",
                                Encoding.UTF8.GetString(Convert.FromBase64String(Match2.Groups[1].Value)));
                        }
                    }
                }

                File.AppendAllText("MailRuAuthLog.log", PreUnpackedLog);
            }
        }
    }
#endif
}
