using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVPDS.Entities;
using MVPDS.Models;
using MediatR;
using MVPDS.Commands;
using MVPDS.Queries;
namespace MVPDS.Controllers
{
    [Authorize]
    public class VoiceController : Controller
    {
        private readonly MvpdsContext _db;
        private readonly IMediator _mediator;
        public VoiceController(MvpdsContext db, IMediator mediator)
        {
            _db = db;
            _mediator = mediator;
        }
        [HttpPost]
        public async Task<IActionResult> CreateVoiceChannel(int serverId, string channelName)
        {
            var result = await _mediator.Send(new CreateVoiceChannelCommand(serverId, channelName));
    
            if (!result.IsSuccess)
                TempData["Error"] = result.Error;

            return RedirectToAction("Channels", new { id = serverId });
        }


        public async Task<IActionResult> Servers()
        {
            var userId = int.Parse(User.Claims.First(c => c.Type == "UserId")!.Value);

            var servers = await _db.VoiceServers
                .Include(s => s.Owner)
                .Include(s => s.VoiceChannels)
                .ThenInclude(c => c.VoiceChannelMembers)
                .Where(s =>
                    s.OwnerId == userId ||
                    s.VoiceChannels.Any(c => c.VoiceChannelMembers.Any(m => m.UserId == userId)))
                .ToListAsync();

            return View("Servers", servers);
        }
        [HttpPost]
        private async Task<IActionResult> CreateServer(string newServerName)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            var result = await _mediator.Send(new CreateServerCommand(newServerName, userId));

            if (!result.IsSuccess)
                TempData["Error"] = result.Error;

            return RedirectToAction("Servers");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVoiceChannel(int channelId)
        {
            var serverId = await _mediator.Send(new DeleteVoiceChannelCommand(channelId));

            if (serverId == null)
                return NotFound();

            return RedirectToAction("Channels", new { id = serverId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteServer(int serverId)
        {
            var result = await _mediator.Send(new DeleteServerCommand(serverId));

            if (!result.IsSuccess)
                TempData["Error"] = result.Error;

            return RedirectToAction("Servers");
        }


        [HttpPost]
        private async Task<IActionResult> JoinServer(string serverName)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            var result = await _mediator.Send(new JoinServerCommand(serverName, userId));

            if (!result.IsSuccess)
                TempData["Error"] = result.Error;

            return RedirectToAction("Servers");
        }

        public async Task<IActionResult> Channels(int id)
        {
            var model = await _mediator.Send(new GetChannelsQuery(id));

            if (model == null)
                return NotFound();

            return View("Channels", model);
        }



        public async Task<IActionResult> Channel(int id)
        {
            var channel = await _db.VoiceChannels
                .Include(c => c.Server)
                .FirstOrDefaultAsync(c => c.VoiceChannelsId == id);

            if (channel == null)
                return NotFound();

            ViewBag.ChannelId = channel.VoiceChannelsId;
            ViewBag.ChannelName = channel.Name;
            ViewBag.ServerName = channel.Server.ServerName;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> HandleServerAction(string serverName, string actionType)
        {
            if (string.IsNullOrWhiteSpace(serverName))
            {
                TempData["Error"] = "Название сервера не может быть пустым.";
                return RedirectToAction("Servers");
            }

            if (actionType == "join")
            {
                return await JoinServer(serverName);
            }
            else if (actionType == "create")
            {
                return await CreateServer(serverName);
            }

            TempData["Error"] = "Неизвестное действие.";
            return RedirectToAction("Servers");
        }
    }
}