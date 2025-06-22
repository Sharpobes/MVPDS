using MediatR;
using MVPDS.Services;
using MVPDS.Repositories;
using MVPDS.Entities;

namespace MVPDS.Commands;

public record JoinServerCommand(string ServerName, int UserId) : IRequest<Result>;

public class JoinServerHandler : IRequestHandler<JoinServerCommand, Result>
{
    private readonly IVoiceServerRepository _serverRepository;
    private readonly IVoiceChannelRepository _channelRepository;
    private readonly IVoiceChannelMemberRepository _memberRepository;

    public JoinServerHandler(
        IVoiceServerRepository serverRepository,
        IVoiceChannelRepository channelRepository,
        IVoiceChannelMemberRepository memberRepository)
    {
        _serverRepository = serverRepository;
        _channelRepository = channelRepository;
        _memberRepository = memberRepository;
    }

    public async Task<Result> Handle(JoinServerCommand request, CancellationToken ct)
    {
        var server = await _serverRepository.GetByNameAsync(request.ServerName);
        if (server == null)
            return Result.Failure($"Сервер с именем \"{request.ServerName}\" не найден.");

        bool alreadyMember = await _memberRepository
            .IsUserMemberOfServerAsync(request.UserId, server.VoiceServersId);

        if (!alreadyMember)
        {
            var defaultChannel = await _channelRepository
                .GetDefaultChannelByServerIdAsync(server.VoiceServersId);

            if (defaultChannel != null)
            {
                _memberRepository.Add(new VoiceChannelMember
                {
                    UserId = request.UserId,
                    ChannelId = defaultChannel.VoiceChannelsId,
                    JoinedAt = DateTime.Now
                });

                await _channelRepository.SaveAsync(ct);
            }
        }

        return Result.Success();
    }
}