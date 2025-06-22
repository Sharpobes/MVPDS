using MediatR;
using MVPDS.Entities;
using MVPDS.Services;
using MVPDS.Repositories;

namespace MVPDS.Commands;

public record CreateVoiceChannelCommand(int ServerId, string ChannelName) : IRequest<Result>;

public class CreateVoiceChannelHandler : IRequestHandler<CreateVoiceChannelCommand, Result>
{
    private readonly IVoiceChannelRepository _channelRepository;

    public CreateVoiceChannelHandler(IVoiceChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<Result> Handle(CreateVoiceChannelCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ChannelName))
            return Result.Failure("Название канала не может быть пустым.");

        var channel = new VoiceChannel
        {
            ServerId = request.ServerId,
            Name = request.ChannelName
        };

        _channelRepository.Add(channel);
        await _channelRepository.SaveAsync(ct);

        return Result.Success();
    }
}