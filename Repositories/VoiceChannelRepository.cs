namespace MVPDS.Repositories;
using MVPDS.Entities;
using Microsoft.EntityFrameworkCore;
public class VoiceChannelRepository : IVoiceChannelRepository
{
    private readonly MvpdsContext _db;

    public VoiceChannelRepository(MvpdsContext db) => _db = db;

    public Task<VoiceChannel?> GetByIdAsync(int id) =>
        _db.VoiceChannels.FirstOrDefaultAsync(c => c.VoiceChannelsId == id);

    public Task<VoiceChannel?> GetDefaultChannelByServerIdAsync(int serverId) =>
        _db.VoiceChannels.FirstOrDefaultAsync(c => c.ServerId == serverId);

    public void Add(VoiceChannel channel) => _db.VoiceChannels.Add(channel);
    public void Remove(VoiceChannel channel) => _db.VoiceChannels.Remove(channel);
    public void RemoveRange(IEnumerable<VoiceChannel> channels) => _db.VoiceChannels.RemoveRange(channels);
    public Task SaveAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}