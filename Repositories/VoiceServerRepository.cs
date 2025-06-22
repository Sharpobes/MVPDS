namespace MVPDS.Repositories;
using MVPDS.Entities;
using Microsoft.EntityFrameworkCore;
public class VoiceServerRepository : IVoiceServerRepository
{
    private readonly MvpdsContext _db;

    public VoiceServerRepository(MvpdsContext db) => _db = db;
    public Task SaveAsync(CancellationToken ct = default)
    {
        return _db.SaveChangesAsync(ct);
    }

    public async Task<VoiceServer?> GetWithChannelsByIdAsync(int id)
    {
        return await _db.VoiceServers
            .Include(s => s.VoiceChannels)
            .ThenInclude(c => c.VoiceChannelMembers)
            .FirstOrDefaultAsync(s => s.VoiceServersId == id);
    }

    public async Task<List<VoiceServer>> GetUserServersAsync(int userId)
    {
        return await _db.VoiceServers
            .Include(s => s.Owner)
            .Include(s => s.VoiceChannels)
            .ThenInclude(c => c.VoiceChannelMembers)
            .Where(s =>
                s.OwnerId == userId ||
                s.VoiceChannels.Any(c => c.VoiceChannelMembers.Any(m => m.UserId == userId)))
            .ToListAsync();
    }

    public async Task<VoiceServer?> GetByNameAsync(string name)
    {
        return await _db.VoiceServers
            .FirstOrDefaultAsync(s => s.ServerName.ToLower() == name.ToLower());
    }

    public void Add(VoiceServer server)
    {
        _db.VoiceServers.Add(server);
    }

    public void Remove(VoiceServer server)
    {
        _db.VoiceServers.Remove(server);
    }
}
