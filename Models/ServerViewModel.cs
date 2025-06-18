using MVPDS.Entities;
namespace MVPDS.Models;

public class ServerViewModel
{
    public VoiceServer Server { get; set; }
    public List<ChatMessage> ChatMessages { get; set; } = new();

}
