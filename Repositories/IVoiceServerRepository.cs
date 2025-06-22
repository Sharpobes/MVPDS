using MVPDS.Entities;
namespace MVPDS.Repositories;

public interface IVoiceServerRepository
{
    Task<VoiceServer?> GetWithChannelsByIdAsync(int id);
    Task<List<VoiceServer>> GetUserServersAsync(int userId);
    Task<VoiceServer?> GetByNameAsync(string name);
    void Add(VoiceServer server);
    void Remove(VoiceServer server);
    Task SaveAsync(CancellationToken ct = default);
}
