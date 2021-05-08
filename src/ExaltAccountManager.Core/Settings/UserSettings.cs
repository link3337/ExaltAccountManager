using System.Collections.Generic;

namespace ExaltAccountManager.Core.Settings
{
    public class UserSettings
    {
        public string? ExaltPath { get; set; }
        public List<Account>? Accounts { get; set; }
    }
}
