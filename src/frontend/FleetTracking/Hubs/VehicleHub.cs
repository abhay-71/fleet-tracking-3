using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace FleetTracking.Hubs
{
    public class VehicleHub : Hub
    {
        public async Task SendVehicleUpdate(string vehicleId, string status, string location)
        {
            await Clients.All.SendAsync("ReceiveVehicleUpdate", vehicleId, status, location);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", "Connected to vehicle hub");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
} 