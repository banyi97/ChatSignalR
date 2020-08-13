using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Dtos;
using Chat.Models;

namespace Chat.Interfaces
{
    public interface IUserService
    {
        public bool TryFindPartner(UserDto dto, out string groupId);
        public string AddUserToWaitList(UserDto dto);
        public void RemoveUserFromWaitList(string connectionId);
    }
}
