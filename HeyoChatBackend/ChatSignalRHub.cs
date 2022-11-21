namespace HeyoChatBackend
{
    using Microsoft.AspNetCore.SignalR;

    /// <summary>
    /// Chat SignalR hub.
    /// </summary>
    public class ChatSignalRHub : Hub
    {
        private static string _botUser = "HeyoChat Bot";
        private readonly IDictionary<string, UserConnection> _connections;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatSignalRHub"/> class.
        /// </summary>
        /// <param name="connections">User connections.</param>
        public ChatSignalRHub(
            IDictionary<string, UserConnection> connections)
        {
            _connections = connections;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_connections.TryGetValue(
                Context.ConnectionId,
                out UserConnection userConnection))
            {
                _connections.Remove(Context.ConnectionId);
                Clients.Group(userConnection.Room)
                    .SendAsync(
                    "ReceiveMessage",
                    _botUser,
                    $"{userConnection.User} has left");
                SendUsersConnected(userConnection.Room);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(UserConnection userConnection)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                userConnection.Room);

            _connections[Context.ConnectionId] = userConnection;

            await Clients.Group(userConnection.Room)
                .SendAsync(
                "ReceiveMessage",
                _botUser,
                $"{userConnection.User} has joined {userConnection.Room}");

            await SendUsersConnected(userConnection.Room);
        }

        public async Task SendMessage(string message)
        {
            if (_connections.TryGetValue(
                Context.ConnectionId,
                out UserConnection userConnection))
            {
                await Clients.Group(userConnection.Room)
                    .SendAsync(
                    "ReceiveMessage",
                    userConnection.User,
                    message);
            }
        }

        public Task SendUsersConnected(string room)
        {
            var users = _connections.Values
                .Where(c => c.Room == room)
                .Select(c => c.User);

            return Clients.Group(room).SendAsync(
                "UsersInRoom",
                users);
        }
    }
}
