using MVPDS.Entities;
using Microsoft.EntityFrameworkCore;
namespace MVPDS.Repositories;

public class VoiceChannelMemberRepository : IVoiceChannelMemberRepository
{
    private readonly MvpdsContext _db;

    public VoiceChannelMemberRepository(MvpdsContext db) => _db = db;

    public Task<bool> IsUserMemberOfServerAsync(int userId, int serverId) =>
        _db.VoiceChannelMembers
            .AnyAsync(m => m.UserId == userId && m.Channel.ServerId == serverId);

    public void Add(VoiceChannelMember member) =>
        _db.VoiceChannelMembers.Add(member);

    public void RemoveRange(IEnumerable<VoiceChannelMember> members) =>
        _db.VoiceChannelMembers.RemoveRange(members);
}
