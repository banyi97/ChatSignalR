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
                this._userService.RemoveUserFromWaitList(this.Context.ConnectionId);
                await this.Clients.OthersInGroup(groupId as string).SendCoreAsync("partnerExit", null);
                await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, groupId as string);
            }
            await base.OnDisconnectedAsync(exception);
        }

        [HubMethodName("searchPartner")]
        public async Task SearchPartnerAsync(UserDto dto)
        {
            Log.Information("searchPartner");
            if (dto is null)
                return;

            Log.Information($"SearchPartner: Dto is not null");
            if (this._userService.TryFindPartner(dto, out var groupId))
            {
                Log.Information($"SearchPartner: Find a partner");
                this.Context.Items.Add("groupId", groupId);
                await this.Groups.AddToGroupAsync(this.Context.ConnectionId, groupId);
                await this.Clients.Group(groupId).SendCoreAsync("openSession", new object[]{});
                return;
            }
            Log.Information($"SearchPartner: Not find partner");
            groupId = this._userService.AddUserToWaitList(dto);
            this.Context.Items.Add("groupId", groupId);
            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, groupId);
        }

        [HubMethodName("cancel")]
        public async Task CancelSearchAsync()
        {
            Log.Information($"CancelSearch");
            if (this.Context.Items.TryGetValue("groupId", out var groupId))
            {
                this.Context.Items.Remove("groupId");
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
                return this.Clients.Group(groupId as string).SendCoreAsync("receiveMessage", new object[] { dto });
            }
            Log.Information("Not found");
            return Task.CompletedTask;
        }

        [HubMethodName("closeSession")]
        public async Task CloseSessionAsync()
        {
            Log.Information("closeSession");
            if (this.Context.Items.TryGetValue("groupId", out var groupId))
            {
                this.Context.Items.Remove("groupId");
                await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, groupId as string);
                await this.Clients.OthersInGroup(groupId as string).SendCoreAsync("partnerExit", new object[] {});
            }
        }
    }
}
