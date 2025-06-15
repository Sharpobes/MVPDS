namespace MVPDS.Entities;

public class ChatMessage
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Chat_Message { get; set; } = null!;

    public DateTime Timestamp_Message { get; set; }

    public int VoiceServers_Id { get; set; }

    public virtual VoiceServer VoiceServer { get; set; } = null!;
}
