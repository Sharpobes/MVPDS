using Microsoft.AspNetCore.Mvc;
using MVPDS.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MVPDS.Services;
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
        {
            TempData["Error"] = "Сообщение не может быть пустым.";
            return RedirectToAction("Channels", "Voice", new { id = serverId });
        }

        var encryptedMessage = AesEncryptionService.Encrypt(message);

        var chatMessage = new ChatMessage
        {
            VoiceServers_Id = serverId,
            UserName = User.Identity?.Name ?? "Аноним",
            Chat_Message = encryptedMessage,
            Timestamp_Message = DateTime.UtcNow
        };

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        return RedirectToAction("Channels", "Voice", new { id = serverId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMessage(int messageId, int serverId)
    {
        var message = await _context.ChatMessages.FindAsync(messageId);

        if (message == null)
        {
            TempData["Error"] = "Сообщение не найдено.";
            return RedirectToAction("Channels", "Voice", new { id = serverId });
        }

        _context.ChatMessages.Remove(message);
        await _context.SaveChangesAsync();

        return RedirectToAction("Channels", "Voice", new { id = serverId });
    }

}
