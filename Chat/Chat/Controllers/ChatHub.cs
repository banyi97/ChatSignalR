using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace Chat.Controllers
{
    public class ChatHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Log.Information($"Connected: {this.Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Log.Information($"Disconnected: {this.Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
