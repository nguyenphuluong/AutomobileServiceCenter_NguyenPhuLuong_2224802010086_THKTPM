using Microsoft.AspNetCore.SignalR;

namespace ASC.Web.Hubs
{
    public class ServiceMessagesHub : Hub
    {
        // Join room theo ServiceRequestId
        public async Task JoinServiceRequest(
            string serviceRequestId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                serviceRequestId
            );
        }

        // Send message
        public async Task SendMessage(
            string serviceRequestId,
            string user,
            string message)
        {
            await Clients.Group(serviceRequestId)
                .SendAsync(
                    "ReceiveMessage",
                    user,
                    message
                );
        }

        // Online
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine(
                $"Connected: {Context.ConnectionId}"
            );

            await base.OnConnectedAsync();
        }

        // Offline
        public override async Task OnDisconnectedAsync(
            Exception? exception)
        {
            Console.WriteLine(
                $"Disconnected: {Context.ConnectionId}"
            );

            await base.OnDisconnectedAsync(exception);
        }
    }
}