using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Models;

namespace Chat.Dtos
{
    public class UserDto
    {
        public string ConnectionId { get; set; }
        public Preference Self { get; set; }
        public Preference UserPreference { get; set; }
    }
}
