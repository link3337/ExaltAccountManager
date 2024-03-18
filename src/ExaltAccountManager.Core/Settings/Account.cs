namespace ExaltAccountManager.Core.Settings
{
    public class Account
    {

        public required string Name { get; set; }
        public required string Base64EMail { get; set; }
        public required string Base64Password { get; set; }
    }
}
