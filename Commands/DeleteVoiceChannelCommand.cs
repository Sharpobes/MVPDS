using MediatR;
using MVPDS.Repositories;

namespace MVPDS.Commands;

public record DeleteVoiceChannelCommand(int ChannelId) : IRequest<int?>;

public class DeleteVoiceChannelHandler : IRequestHandler<DeleteVoiceChannelCommand, int?>
{
    private readonly IVoiceChannelRepository _channelRepository;

    public DeleteVoiceChannelHandler(IVoiceChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<int?> Handle(DeleteVoiceChannelCommand request, CancellationToken ct)
    {
        var channel = await _channelRepository.GetByIdAsync(request.ChannelId);

        if (channel == null)
            return null;

        int serverId = channel.ServerId;

        _channelRepository.Remove(channel);
        await _channelRepository.SaveAsync(ct);

        return serverId;
    }
}