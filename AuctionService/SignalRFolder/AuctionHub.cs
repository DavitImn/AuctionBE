using System.Text.RegularExpressions;
using AuctionService.DataContextDb;
using Microsoft.AspNetCore.SignalR;

namespace AuctionService.SignalRFolder
{
    public class AuctionHub : Hub
    {
       

        // Optional method to join a specific auction group
        public Task JoinAuctionGroup(string auctionId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, $"auction-{auctionId}");
        }

        // Optional method to leave group (not required in most cases)
        public Task LeaveAuctionGroup(string auctionId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"auction-{auctionId}");
        }
    }
}
