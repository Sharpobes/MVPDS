using System;
using System.Collections.Generic;

namespace MVPDS.Entities;

public partial class VoiceChannelMember
{
    public int ChannelId { get; set; }

    public int UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public virtual VoiceChannel Channel { get; set; } = null!;

    public virtual VoiceUser User { get; set; } = null!;
}
