using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Models
{
    [Flags]
    public enum Preference
    {
        Man = 1,
        Woman = 2,
        Both = 3
    }
    public class UserPreference
    {
    }
}
