using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using FleetTracking.Models;

namespace FleetTracking.Hubs
{
    [Authorize]
    public class VehicleStatusHub : Hub
    {
        // Sends vehicle status update to all connected clients
        public async Task BroadcastVehicleStatus(VehicleStatus status)
        {
            await Clients.All.SendAsync("ReceiveVehicleStatus", status);
        }

        // Sends vehicle status update to a specific group (e.g., company-specific monitoring)
        public async Task SendVehicleStatusToGroup(string groupName, VehicleStatus status)
        {
            await Clients.Group(groupName).SendAsync("ReceiveVehicleStatus", status);
        }

        // Join a specific monitoring group
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Leave a specific monitoring group
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        // Notifies clients when a vehicle enters a geofence
        public async Task NotifyGeofenceEntry(int vehicleId, int geofenceId, string vehicleName, string geofenceName)
        {
            await Clients.All.SendAsync("GeofenceEntry", vehicleId, geofenceId, vehicleName, geofenceName, DateTime.UtcNow);
        }

        // Notifies clients when a vehicle exits a geofence
        public async Task NotifyGeofenceExit(int vehicleId, int geofenceId, string vehicleName, string geofenceName)
        {
            await Clients.All.SendAsync("GeofenceExit", vehicleId, geofenceId, vehicleName, geofenceName, DateTime.UtcNow);
        }

        // Notifies clients when a vehicle status changes (e.g., idle, moving, stopped)
        public async Task NotifyStatusChange(int vehicleId, string vehicleName, string oldStatus, string newStatus)
        {
            await Clients.All.SendAsync("VehicleStatusChange", vehicleId, vehicleName, oldStatus, newStatus, DateTime.UtcNow);
        }

        // Sends an alert to all connected clients
        public async Task SendAlert(string alertType, string message, int? vehicleId = null, int? driverId = null)
        {
            await Clients.All.SendAsync("ReceiveAlert", alertType, message, vehicleId, driverId, DateTime.UtcNow);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
} 