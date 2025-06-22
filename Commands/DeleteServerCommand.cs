using MediatR;
using MVPDS.Services;
using MVPDS.Repositories;
using MVPDS.Entities;

namespace MVPDS.Commands;

public record DeleteServerCommand(int ServerId) : IRequest<Result>;

public class DeleteServerHandler : IRequestHandler<DeleteServerCommand, Result>
{
    private readonly IVoiceServerRepository _serverRepository;
    private readonly IVoiceChannelRepository _channelRepository;
    private readonly IVoiceChannelMemberRepository _memberRepository;
    private readonly IChatMessageRepository _chatRepository;

    public DeleteServerHandler(
        IVoiceServerRepository serverRepository,
        IVoiceChannelRepository channelRepository,
        IVoiceChannelMemberRepository memberRepository,
        IChatMessageRepository chatRepository)
    {
        _serverRepository = serverRepository;
        _channelRepository = channelRepository;
        _memberRepository = memberRepository;
        _chatRepository = chatRepository;
    }

    public async Task<Result> Handle(DeleteServerCommand request, CancellationToken ct)
    {
        var server = await _serverRepository.GetWithChannelsByIdAsync(request.ServerId);

        if (server == null)
            return Result.Failure("Сервер не найден.");

        _chatRepository.RemoveRange(server.ChatMessages);

        foreach (var channel in server.VoiceChannels)
        {
            _memberRepository.RemoveRange(channel.VoiceChannelMembers);
        }

        _channelRepository.RemoveRange(server.VoiceChannels);
        _serverRepository.Remove(server);

        await _serverRepository.SaveAsync(ct);

        return Result.Success();
    }
}