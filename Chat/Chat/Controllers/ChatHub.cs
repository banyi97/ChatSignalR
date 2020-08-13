using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Dtos;
using Chat.Interfaces;
using Chat.Models;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace Chat.Controllers
{
    public class ChatHub : Hub
    {
        private readonly IUserService _userService;
        public ChatHub(IUserService userService)
        {
            _userService = userService ?? throw new NullReferenceException();
        }
        public override Task OnConnectedAsync()
        {
            Log.Information($"Connected: {this.Context.ConnectionId}");
            return base.OnConnectedAsync();
        } 

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Log.Information($"Disconnected: {this.Context.ConnectionId}");
            if (this.Context.Items.TryGetValue("groupId",out var groupId))
            {
                await this.Clients.OthersInGroup(groupId as string).SendCoreAsync("partnerExit", null);
                await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, groupId as string);
            }
            await base.OnDisconnectedAsync(exception);
        }

        [HubMethodName("searchPartner")]
        public Task SearchPartnerAsync(UserDto dto)
        {
            Log.Information("searchPartner");
            if (dto is null)
                return Task.FromException(new NullReferenceException());
            Log.Information($"Id: {dto.ConnectionId}");
            Log.Information($"Self: {dto.Self}");
            Log.Information($"Pref: {dto.UserPreference}");

            Log.Information($"SearchPartner: Dto is not null");
            if (this._userService.TryFindPartner(dto, out var groupId))
            {
                Log.Information($"SearchPartner: Find a partner");
                this.Context.Items.Add("groupId", groupId);
                this.Groups.AddToGroupAsync(this.Context.ConnectionId, groupId).GetAwaiter().GetResult();
                return this.Clients.Group(groupId).SendCoreAsync("openSession", null);
            }
            Log.Information($"SearchPartner: Not find partner");
            groupId = this._userService.AddUserToWaitList(dto);
            this.Context.Items.Add("groupId", groupId);
            return this.Groups.AddToGroupAsync(this.Context.ConnectionId, groupId);
        }

        [HubMethodName("cancelSearch")]
        public async Task CancelSearchAsync()
        {
            Log.Information($"CancelSearch");
            if (this.Context.Items.TryGetValue("groupId", out var groupId))
            {
                await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, groupId as string);
            }
            this._userService.RemoveUserFromWaitList(this.Context.ConnectionId);
        }

        [HubMethodName("sendMessage")]
        public Task SendMessageAsync(MessageDto dto)
        {
            Log.Information($"SendMessage");
            if (this.Context.Items.TryGetValue("groupId", out var groupId))
            {
                Log.Information("Send");
                return this.Clients.OthersInGroup(groupId as string).SendCoreAsync("receiveMessage", new object[] { dto });
            }
            Log.Information("Not found");
            return Task.CompletedTask;
        }

        [HubMethodName("closeSession")]
        public Task CloseSessionAsync()
        {
            Log.Information("closeSession");
            if (this.Context.Items.TryGetValue("groupId", out var groupId))
            {
                this.Context.Items.Remove("groupId");
                return this.Clients.OthersInGroup(groupId as string).SendCoreAsync("partnerExit", null);
            }
            return Task.CompletedTask;
        }
    }
}
