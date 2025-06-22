using MediatR;
using MVPDS.Entities;
using MVPDS.Models;
using Microsoft.EntityFrameworkCore;

namespace MVPDS.Queries;

public record GetChannelsQuery(int ServerId) : IRequest<ServerViewModel?>;

public class GetChannelsHandler : IRequestHandler<GetChannelsQuery, ServerViewModel?>
{
    private readonly MvpdsContext _db;

    public GetChannelsHandler(MvpdsContext db) => _db = db;

    public async Task<ServerViewModel?> Handle(GetChannelsQuery request, CancellationToken ct)
    {
        var server = await _db.VoiceServers
            .Include(s => s.VoiceChannels)
            .ThenInclude(vc => vc.VoiceChannelMembers)
            .FirstOrDefaultAsync(s => s.VoiceServersId == request.ServerId, ct);

        if (server == null)
            return null;

        var chatMessages = await _db.ChatMessages
            .Where(m => m.VoiceServers_Id == request.ServerId)
            .ToListAsync(ct);

        return new ServerViewModel
        {
            Server = server,
            ChatMessages = chatMessages
        };
    }
}