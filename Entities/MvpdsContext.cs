using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MVPDS.Entities;

public partial class MvpdsContext : DbContext
{
    public MvpdsContext()
    {
    }

    public MvpdsContext(DbContextOptions<MvpdsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<VoiceChannel> VoiceChannels { get; set; }

    public virtual DbSet<VoiceChannelMember> VoiceChannelMembers { get; set; }

    public virtual DbSet<VoiceServer> VoiceServers { get; set; }

    public virtual DbSet<VoiceUser> VoiceUsers { get; set; }
    
    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=MVPDS;User ID=mvpuser;Password=MvpUserPassword123!;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VoiceChannel>(entity =>
        {
            entity.HasKey(e => e.VoiceChannelsId).HasName("PK__VoiceCha__78CA87B71496CD80");

            entity.Property(e => e.VoiceChannelsId).HasColumnName("VoiceChannels_Id");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Server).WithMany(p => p.VoiceChannels)
                .HasForeignKey(d => d.ServerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VoiceChan__Serve__3D5E1FD2");
        });
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Chat_Message)
                .IsRequired();

            entity.Property(e => e.Timestamp_Message)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            entity.Property(e => e.VoiceServers_Id)
                .IsRequired();

            entity.HasOne(e => e.VoiceServer)
                .WithMany(s => s.ChatMessages)
                .HasForeignKey(e => e.VoiceServers_Id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ChatMessages_VoiceServers");
        });
        modelBuilder.Entity<VoiceChannelMember>(entity =>
        {
            entity.HasKey(e => new { e.ChannelId, e.UserId }).HasName("PK__VoiceCha__E9BB64D0C1A7BE97");

            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Channel).WithMany(p => p.VoiceChannelMembers)
                .HasForeignKey(d => d.ChannelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VoiceChan__Chann__403A8C7D");

            entity.HasOne(d => d.User).WithMany(p => p.VoiceChannelMembers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VoiceChan__UserI__412EB0B6");
        });

        modelBuilder.Entity<VoiceServer>(entity =>
        {
            entity.HasKey(e => e.VoiceServersId).HasName("PK__VoiceSer__8D7A427169153207");

            entity.Property(e => e.VoiceServersId).HasColumnName("VoiceServers_Id");
            entity.Property(e => e.ServerName)
                .HasMaxLength(100)
                .HasColumnName("Server_Name");

            entity.HasOne(d => d.Owner).WithMany(p => p.VoiceServers)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VoiceServ__Owner__3A81B327");
        });

        modelBuilder.Entity<VoiceUser>(entity =>
        {
            entity.HasKey(e => e.VoiceUsersId).HasName("PK__VoiceUse__9DA91BC30E27957D");

            entity.HasIndex(e => e.Username, "UQ__VoiceUse__536C85E44590DCB4").IsUnique();

            entity.Property(e => e.VoiceUsersId).HasColumnName("VoiceUsers_Id");
            entity.Property(e => e.UserPassword)
                .HasMaxLength(200)
                .HasColumnName("User_Password");
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
