using System.Xml.Linq;
using ExaltAccountManager.Core.AccessToken;
using ExaltAccountManager.Core.Exceptions;

namespace ExaltAccountManager.Core.Util
{
    /// <summary>
    /// Helper class for making requests to the Realm of the Mad God API.
    /// </summary>
    public class RequestHelper
    {
        /// <summary>
        /// Requests an access token from the Realm of the Mad God API.
        /// </summary>
        /// <param name="accessTokenRequest">The request object containing the necessary parameters for the access token request.</param>
        /// <returns>The access token response.</returns>
        /// <exception cref="AccessTokenParseFailedException">Thrown when the response content cannot be parsed.</exception>
        /// <exception cref="AccessTokenRetrievalFailedException">Thrown when the access token retrieval fails.</exception>
        public static async Task<AccessTokenResponse> RequestAccessToken(AccessTokenRequest accessTokenRequest)
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
            List<KeyValuePair<string, string>> content =
            [
                new("clientToken", accessTokenRequest.DeviceToken!), // the actual device token doesn't really matter, "0" could be used here or any value literally, it is possible but very rare to get a "token for a different machine error" though.
                new("guid", accessTokenRequest.Guid),
                new("game_net", isSteam ? "Unity_steam" : "Unity"),
                new("play_platform", isSteam ? "Unity_steam" : "Unity"),
                new("game_net_user_id", steamId),
            ];

            if (isSteam)
            {
                content.Add(new("steamid", steamId));
                content.Add(new("secret", accessTokenRequest.Password!));
            }
            else
            {
                content.Add(new("password", accessTokenRequest.Password!));
            }

            HttpRequestMessage requestMessage = new()
            {
                RequestUri = new Uri("https://www.realmofthemadgod.com/account/verify"),
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(content!),
            };

            HttpResponseMessage response = await client
                .SendAsync(requestMessage)
                .ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // read result
                    string c = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    XElement xml = XElement.Parse(c);

                    return new AccessTokenResponse
                    {
                        AccessToken = xml.Descendants()
                            .First(node => node.Name == "AccessToken")
                            .Value,
                        AccessTokenExpiration = Convert.ToInt32(
                            xml.Descendants()
                                .First(node => node.Name == "AccessTokenExpiration")
                                .Value
                        ),
                        AccessTokenTimestamp = xml.Descendants()
                            .First(node => node.Name == "AccessTokenTimestamp")
                            .Value,
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
