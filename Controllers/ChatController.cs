using Microsoft.AspNetCore.Mvc;
using MVPDS.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace MVPDS.Controllers;

public class ChatController : Controller
{
    private readonly MvpdsContext _context;

    public ChatController(MvpdsContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(int serverId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return RedirectToAction("Details", "Servers", new { id = serverId });

        var chatMessage = new ChatMessage
        {
            VoiceServers_Id = serverId,
            UserName = User.Identity?.Name,
            Chat_Message = message,
            Timestamp_Message = DateTime.UtcNow
        };

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Servers", new { id = serverId });
    }
}
