using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVPDS.Entities;

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
                .Distinct()
                .ToListAsync();

            return View(servers);
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
                .FirstOrDefaultAsync(s => s.VoiceServersId == id);

            if (server == null)
                return NotFound();

            return View(server);
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