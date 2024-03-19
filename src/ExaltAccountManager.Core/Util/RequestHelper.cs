using ExaltAccountManager.Core.AccessToken;
using ExaltAccountManager.Core.Exceptions;
using System.Xml.Linq;

namespace ExaltAccountManager.Core.Util
{
    public static class RequestHelper
    {
        public async static Task<AccessTokenResponse> RequestAccessToken(AccessTokenRequest accessTokenRequest)
        {
            bool isSteam = false;
            string steamId = "";
            // check if it is a steam account
            if (accessTokenRequest.Guid!.Contains("steamworks:"))
            {
                isSteam = true;
                steamId = accessTokenRequest.Guid.Split(":")[1];
            }

            using HttpClient client = new();
            var content = new List<KeyValuePair<string, string>>
                {
                    new("clientToken", accessTokenRequest.DeviceToken!), // the actual device token doesn't really matter, "0" could be used here or any value literally, it is possible but very rare to get a "token for a different machine error" though.
                    new("guid", accessTokenRequest.Guid),
                    new("game_net", isSteam ? "Unity_steam" : "Unity"),
                    new("play_platform", isSteam ? "Unity_steam" : "Unity"),
                    new("game_net_user_id", steamId)
                };

            if (isSteam)
            {
                content.Add(new("steamid", steamId));
                content.Add(new("secret", accessTokenRequest.Password!));
            }
            else
            {
                content.Add(new("password", accessTokenRequest.Password!));
            }

            var requestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri("https://www.realmofthemadgod.com/account/verify"),
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(content!)
            };

            var response = await client.SendAsync(requestMessage).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // read result
                    var c = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    var xml = XElement.Parse(c);

                    return new AccessTokenResponse
                    {
                        AccessToken = xml.Descendants().First(node => node.Name == "AccessToken").Value,
                        AccessTokenExpiration = Convert.ToInt32(xml.Descendants().First(node => node.Name == "AccessTokenExpiration").Value),
                        AccessTokenTimestamp = xml.Descendants().First(node => node.Name == "AccessTokenTimestamp").Value
                    };
                }
                catch (Exception)
                {
                    throw new AccessTokenParseFailedException();
                }
            }
            throw new AccessTokenRetrievalFailedException();
        }
    }
}