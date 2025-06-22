using MVPDS.Entities;
namespace MVPDS.Repositories;

public interface IVoiceChannelRepository
{
    Task<VoiceChannel?> GetByIdAsync(int id);
    Task<VoiceChannel?> GetDefaultChannelByServerIdAsync(int serverId);
    void Add(VoiceChannel channel);
    void Remove(VoiceChannel channel);
    void RemoveRange(IEnumerable<VoiceChannel> channels);
    Task SaveAsync(CancellationToken ct = default);
}