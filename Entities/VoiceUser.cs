using System;
using System.Collections.Generic;

namespace MVPDS.Entities;

public partial class VoiceUser
{
    public int VoiceUsersId { get; set; }

    public string Username { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public virtual ICollection<VoiceChannelMember> VoiceChannelMembers { get; set; } = new List<VoiceChannelMember>();

    public virtual ICollection<VoiceServer> VoiceServers { get; set; } = new List<VoiceServer>();
}
