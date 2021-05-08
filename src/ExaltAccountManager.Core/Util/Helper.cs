using ExaltAccountManager.Core.AccessToken;
using ExaltAccountManager.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace ExaltAccountManager.Core.Util
{
    public class Helper
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string encodedText)
        {
            var data = Convert.FromBase64String(encodedText);
            return Encoding.UTF8.GetString(data);
        }

        public async static void LaunchExaltClient(string exaltPath, string email, string password)
        {
            var deviceToken = GetDeviceToken();
            // get access token         
            AccessTokenRequest accessTokenRequest = new(email, password, deviceToken);
            AccessTokenResponse accessTokenResponse = await RequestHelper.RequestAccessToken(accessTokenRequest).ConfigureAwait(false);
                      
            if (string.IsNullOrEmpty(exaltPath))
            {
                throw new ExaltPathNotFoundException();
            }
            exaltPath += @"\RotMG Exalt.exe";
            try
            {
                ProcessStartInfo processStartInfo = new();
                processStartInfo.FileName = exaltPath;
                processStartInfo.Arguments = $"data:{{platform:Deca,guid:{Helper.Base64Encode(email)},token:{Helper.Base64Encode(accessTokenResponse.AccessToken)},tokenTimestamp:{Helper.Base64Encode(accessTokenResponse.AccessTokenTimestamp)},tokenExpiration:{Helper.Base64Encode(accessTokenResponse.AccessTokenExpiration.ToString())},env:4}}";
                Process.Start(processStartInfo);
            }
            catch (Exception)
            {
                throw new ExaltExeNotFoundException();
            }
        }

        private static string GetDeviceToken()
        {
            string hardWareInfo = "";
            foreach (var item in new List<string>() { "Win32_BaseBoard", "Win32_BIOS", "Win32_OperatingSystem" }) // needed in this order
            {
                foreach (var hwValues in new ManagementObjectSearcher($"SELECT * FROM {item}").Get())
                {
                    hardWareInfo += hwValues.GetPropertyValue("SerialNumber").ToString() ?? "";
                }
            }
            return Hash(hardWareInfo);
        }

        private static string Hash(string input)
        {
            using SHA1Managed sha1 = new();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
    }
}
