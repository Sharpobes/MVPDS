using System;
using System.Collections.Generic;

namespace MVPDS.Entities;

public partial class VoiceServer
{
    public int VoiceServersId { get; set; }

    public string ServerName { get; set; } = null!;

    public int OwnerId { get; set; }

    public virtual VoiceUser Owner { get; set; } = null!;
    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public virtual ICollection<VoiceChannel> VoiceChannels { get; set; } = new List<VoiceChannel>();
}
