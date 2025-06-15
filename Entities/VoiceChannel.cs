using System;
using System.Collections.Generic;

namespace MVPDS.Entities;

public partial class VoiceChannel
{
    public int VoiceChannelsId { get; set; }

    public int ServerId { get; set; }

    public string Name { get; set; } = null!;

    public virtual VoiceServer Server { get; set; } = null!;

    public virtual ICollection<VoiceChannelMember> VoiceChannelMembers { get; set; } = new List<VoiceChannelMember>();
}
