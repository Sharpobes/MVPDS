using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
namespace MVPDS.Services
{
    [Authorize]
    public class VoiceHub : Hub
    {
        private static readonly ConcurrentDictionary<string, HashSet<string>> _channelUsers = new();
        private static readonly ConcurrentDictionary<string, string> _userChannelMap = new();

        public async Task JoinChannel(string channelId)
        {
            Console.WriteLine($"[Hub] JoinChannel вызван для {Context.User?.Identity?.Name}, канал: {channelId}");
            var connectionId = Context.ConnectionId;
            var username = Context.User?.Identity?.Name ?? connectionId;

            if (_userChannelMap.TryGetValue(connectionId, out var prevChannelId) && prevChannelId != channelId)
            {
                await LeaveChannel(prevChannelId);
            }

            await Groups.AddToGroupAsync(connectionId, channelId);
            _userChannelMap[connectionId] = channelId;

            _channelUsers.AddOrUpdate(channelId,
                _ => new HashSet<string> { username },
                (_, users) =>
                {
                    users.Add(username);
                    return users;
                });
            await Clients.Caller.SendAsync("UserConnected", channelId);
            await SendUsersList(channelId);
        }

        public async Task LeaveChannel(string channelId)
        {
            var connectionId = Context.ConnectionId;
            var username = Context.User?.Identity?.Name ?? connectionId;

            await Groups.RemoveFromGroupAsync(connectionId, channelId);

            if (_channelUsers.TryGetValue(channelId, out var users))
            {
                users.Remove(username);
                if (users.Count == 0)
                {
                    _channelUsers.TryRemove(channelId, out _);
                }
            }

            _userChannelMap.TryRemove(connectionId, out _);
            await Clients.Caller.SendAsync("UserDisconnected", channelId);
            await SendUsersList(channelId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"[Hub] Отключен: {Context.ConnectionId}, причина: {exception?.Message}");

            if (_userChannelMap.TryRemove(Context.ConnectionId, out var channelId))
            {
                await LeaveChannel(channelId);
            }

            await base.OnDisconnectedAsync(exception);
        }


        private async Task SendUsersList(string channelId)
        {
            if (_channelUsers.TryGetValue(channelId, out var users))
            {
                await Clients.Group(channelId)
                    .SendAsync("UpdateUserList", channelId, users.ToList());
            }
        }
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"[Hub] Подключен: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }
        public async Task SendAudio(string channelId, string base64Audio)
        {
            try
            {
                var audioData = Convert.FromBase64String(base64Audio);

                Console.WriteLine($"[Hub] Получено аудио ({audioData.Length} байт) от {Context.User?.Identity?.Name} в канал {channelId}");

                await Clients.OthersInGroup(channelId)
                    .SendAsync("ReceiveAudio", Context.ConnectionId, base64Audio);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Hub ERROR] Ошибка в SendAudio: {ex.Message}");
            }
        }



    }
}
