namespace ExaltAccountManager.Core.AccessToken
{
    public class AccessTokenRequest
    {
        public AccessTokenRequest(string guid, string password, string deviceToken)
        {
            Guid = guid;
            Password = password;
            DeviceToken = deviceToken;
        }

        public string? DeviceToken { get; set; }
        public string? Guid { get; set; }
        public string? Password { get; set; }
    }
}