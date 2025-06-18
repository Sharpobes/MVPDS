using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVPDS.Entities;
using MVPDS.Models;
namespace MVPDS.Controllers
{
    [Authorize]
    public class VoiceController : Controller
    {
        private readonly MvpdsContext _db;

        public VoiceController(MvpdsContext db)
        {
            _db = db;
        }
        [HttpPost]
        public async Task<IActionResult> CreateVoiceChannel(int serverId, string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
            {
                TempData["Error"] = "Название канала не может быть пустым.";
                return RedirectToAction("Channels", new { id = serverId });
            }

            var channel = new VoiceChannel
            {
                ServerId = serverId,
                Name = channelName
            };

            _db.VoiceChannels.Add(channel);
            await _db.SaveChangesAsync();

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
        public async Task<IActionResult> CreateServer(string newServerName)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            if (string.IsNullOrWhiteSpace(newServerName))
            {
                TempData["Error"] = "Название сервера не может быть пустым.";
                return RedirectToAction("Servers");
            }

            var server = new VoiceServer
            {
                ServerName = newServerName,
                OwnerId = userId
            };

            _db.VoiceServers.Add(server);
            await _db.SaveChangesAsync();

            var defaultChannel = new VoiceChannel
            {
                Name = "Общий",
                ServerId = server.VoiceServersId
            };

            _db.VoiceChannels.Add(defaultChannel);
            await _db.SaveChangesAsync();

            _db.VoiceChannelMembers.Add(new VoiceChannelMember
            {
                UserId = userId,
                ChannelId = defaultChannel.VoiceChannelsId,
                JoinedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            return RedirectToAction("Servers");
        }



        [HttpPost]
        public async Task<IActionResult> JoinServer(string serverName)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
    
            var server = await _db.VoiceServers
                .FirstOrDefaultAsync(s => s.ServerName.ToLower() == serverName.ToLower());


            if (server == null)
            {
                TempData["Error"] = $"Сервер с именем \"{serverName}\" не найден.";
                return RedirectToAction("Servers");
            }

            bool alreadyMember = await _db.VoiceChannelMembers
                .AnyAsync(m => m.UserId == userId && m.Channel.ServerId == server.VoiceServersId);

            if (!alreadyMember)
            {
                var defaultChannel = await _db.VoiceChannels
                    .FirstOrDefaultAsync(c => c.ServerId == server.VoiceServersId);

                if (defaultChannel != null)
                {
                    _db.VoiceChannelMembers.Add(new VoiceChannelMember
                    {
                        UserId = userId,
                        ChannelId = defaultChannel.VoiceChannelsId,
                        JoinedAt = DateTime.Now
                    });

                    await _db.SaveChangesAsync();
                }
            }

            return RedirectToAction("Servers");
        }

        public async Task<IActionResult> Channels(int id)
        {
            var server = await _db.VoiceServers
                .Include(s => s.VoiceChannels)
                .ThenInclude(vc => vc.VoiceChannelMembers)
                .FirstOrDefaultAsync(s => s.VoiceServersId == id);

            if (server == null)
                return NotFound();

            var chatMessages = await _db.ChatMessages
                .Where(m => m.VoiceServers_Id == id)
                .ToListAsync();

            var model = new ServerViewModel
            {
                Server = server,
                ChatMessages = chatMessages
            };

            return View("Channels", model); // ✅ Channels.cshtml ожидает ServerViewModel
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

    }
}