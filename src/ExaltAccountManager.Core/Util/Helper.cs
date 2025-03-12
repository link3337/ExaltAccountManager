using System.Diagnostics;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using ExaltAccountManager.Core.AccessToken;
using ExaltAccountManager.Core.Exceptions;

namespace ExaltAccountManager.Core.Util
{
    /// <summary>
    /// Provides helper methods for encoding, decoding, and launching the Exalt client.
    /// </summary>
    public class Helper
    {
        /// <summary>
        /// Encodes a plain text string to a Base64 encoded string.
        /// </summary>
        /// <param name="plainText">The plain text string to encode.</param>
        /// <returns>The Base64 encoded string.</returns>
        public static string Base64Encode(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Decodes a Base64 encoded string to a plain text string.
        /// </summary>
        /// <param name="encodedText">The Base64 encoded string to decode.</param>
        /// <returns>The decoded plain text string.</returns>
        public static string Base64Decode(string encodedText)
        {
            byte[] data = Convert.FromBase64String(encodedText);
            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Launches the Exalt client with the specified parameters.
        /// </summary>
        /// <param name="exaltPath">The path to the Exalt executable.</param>
        /// <param name="email">The email address for authentication.</param>
        /// <param name="password">The password for authentication.</param>
        /// <param name="manualDeviceToken">The manual device token, if any.</param>
        /// <exception cref="ExaltPathNotFoundException">Thrown when the Exalt path is not found.</exception>
        /// <exception cref="ExaltExeNotFoundException">Thrown when the Exalt executable is not found.</exception>
        public static async void LaunchExaltClient(string exaltPath, string email, string password, string manualDeviceToken)
        {
            string deviceToken = string.IsNullOrEmpty(manualDeviceToken) ? GetDeviceToken() : manualDeviceToken;
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
                ProcessStartInfo processStartInfo = new()
                {
                    FileName = exaltPath,
                    Arguments =
                        $"data:{{platform:Deca,guid:{Helper.Base64Encode(email)},token:{Helper.Base64Encode(accessTokenResponse.AccessToken)},tokenTimestamp:{Helper.Base64Encode(accessTokenResponse.AccessTokenTimestamp)},tokenExpiration:{Helper.Base64Encode(accessTokenResponse.AccessTokenExpiration.ToString())},env:4}}",
                };
                Process.Start(processStartInfo);
            }
            catch (Exception)
            {
                throw new ExaltExeNotFoundException();
            }
        }

        /// <summary>
        /// Retrieves the device token based on hardware information.
        /// </summary>
        /// <returns>The hashed device token.</returns>
        private static string GetDeviceToken()
        {
            string hardWareInfo = "";
            foreach (string item in new List<string>() { "Win32_BaseBoard", "Win32_BIOS", "Win32_OperatingSystem" }) // needed in this order
            {
                foreach (ManagementBaseObject? hwValues in new ManagementObjectSearcher($"SELECT * FROM {item}").Get())
                {
                    hardWareInfo += hwValues.GetPropertyValue("SerialNumber").ToString() ?? "";
                }
            }
            return Hash(hardWareInfo);
        }

        /// <summary>
        /// Computes the SHA1 hash of the input string.
        /// </summary>
        /// <param name="input">The input string to hash.</param>
        /// <returns>The SHA1 hash of the input string.</returns>
        private static string Hash(string input)
        {
            byte[] hash = SHA1.HashData(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
    }
}