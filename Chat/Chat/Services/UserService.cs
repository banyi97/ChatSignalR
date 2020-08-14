using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Dtos;
using Chat.Interfaces;
using Chat.Models;
using Serilog;

namespace Chat.Services
{
    public class UserService: IUserService
    {
        public UserService()
        {
            _users = new List<ApplicationUser>();
        }
        private readonly object _lockObj = new object();

        private readonly IList<ApplicationUser> _users;


        public bool TryFindPartner(UserDto dto, out string groupId)
        {
            lock (_lockObj)
            {
                foreach (var user in this._users)
                {
                    if ((user.UserPreference & dto.Self) > 0 && (dto.UserPreference & user.Self) > 0)
                    {
                        groupId = user.GroupId;
                        this._users.Remove(user);
                        return true;
                    }
                }
            }

            groupId = null;
            return false;
        }

        public string AddUserToWaitList(UserDto dto)
        {
            var groupId = new Guid().ToString();
            lock (_lockObj)
            {   
                this._users.Add(new ApplicationUser()
                {
                    ConnectionId = dto.ConnectionId,
                    GroupId = groupId,
                    Self = dto.Self,
                    UserPreference = dto.UserPreference
                });
            }

            return groupId;
        }

        public void RemoveUserFromWaitList(string connectionId)
        {
            lock (_lockObj)
            {
                var user = this._users.FirstOrDefault(q => q.ConnectionId == connectionId);
                if(user != null)
                    this._users.Remove(user);
            }
        }
    }
}
