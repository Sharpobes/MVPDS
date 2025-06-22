using MVPDS.Entities;
namespace MVPDS.Repositories;

public interface IVoiceChannelMemberRepository
{
    Task<bool> IsUserMemberOfServerAsync(int userId, int serverId);
    void Add(VoiceChannelMember member);
    void RemoveRange(IEnumerable<VoiceChannelMember> members);
}