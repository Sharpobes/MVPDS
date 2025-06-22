using MVPDS.Entities;
using Microsoft.EntityFrameworkCore;
namespace MVPDS.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly MvpdsContext _db;

    public ChatMessageRepository(MvpdsContext db) => _db = db;

    public Task<List<ChatMessage>> GetByServerIdAsync(int serverId) =>
        _db.ChatMessages.Where(m => m.VoiceServers_Id == serverId).ToListAsync();

    public void RemoveRange(IEnumerable<ChatMessage> messages) =>
        _db.ChatMessages.RemoveRange(messages);
}
