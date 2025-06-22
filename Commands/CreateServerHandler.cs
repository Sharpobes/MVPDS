using MediatR;
using MVPDS.Entities;
using MVPDS.Services;
using MVPDS.Repositories;

namespace MVPDS.Commands;

public record CreateServerCommand(string ServerName, int UserId) : IRequest<Result>;

public class CreateServerHandler : IRequestHandler<CreateServerCommand, Result>
{
    private readonly IVoiceServerRepository _serverRepository;
    private readonly IVoiceChannelRepository _channelRepository;
    private readonly IVoiceChannelMemberRepository _memberRepository;

    public CreateServerHandler(
        IVoiceServerRepository serverRepository,
        IVoiceChannelRepository channelRepository,
        IVoiceChannelMemberRepository memberRepository)
    {
        _serverRepository = serverRepository;
        _channelRepository = channelRepository;
        _memberRepository = memberRepository;
    }

    public async Task<Result> Handle(CreateServerCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ServerName))
            return Result.Failure("Название сервера не может быть пустым.");

        var server = new VoiceServer
        {
            ServerName = request.ServerName,
            OwnerId = request.UserId
        };

        _serverRepository.Add(server);
        await _serverRepository.SaveAsync(ct);

        var defaultChannel = new VoiceChannel
        {
            Name = "Общий",
            ServerId = server.VoiceServersId
        };

        _channelRepository.Add(defaultChannel);
        await _channelRepository.SaveAsync(ct);

        _memberRepository.Add(new VoiceChannelMember
        {
            UserId = request.UserId,
            ChannelId = defaultChannel.VoiceChannelsId,
            JoinedAt = DateTime.Now
        });
        
        await _channelRepository.SaveAsync(ct);

        return Result.Success();
    }
}