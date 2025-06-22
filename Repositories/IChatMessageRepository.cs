using MVPDS.Entities;
namespace MVPDS.Repositories;

public interface IChatMessageRepository
{
    Task<List<ChatMessage>> GetByServerIdAsync(int serverId);
    void RemoveRange(IEnumerable<ChatMessage> messages);
}