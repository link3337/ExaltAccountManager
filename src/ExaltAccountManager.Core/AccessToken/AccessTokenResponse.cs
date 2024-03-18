namespace ExaltAccountManager.Core.AccessToken
{
    public class AccessTokenResponse
    {
        public required string AccessToken { get; set; }
        public required string AccessTokenTimestamp { get; set; } // this should be DateTimeOffset
        public int AccessTokenExpiration { get; set; }
    } 
}
