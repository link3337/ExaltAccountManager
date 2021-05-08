namespace ExaltAccountManager.Core.AccessToken
{
    public class AccessTokenResponse
    {
        public string AccessToken { get; set; } = null!;
        public string AccessTokenTimestamp { get; set; } = null!; // this should be DateTimeOffset
        public int AccessTokenExpiration { get; set; }
    }
}
