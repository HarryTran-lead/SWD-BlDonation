using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SWD_BLDONATION.Models;

public partial class BloodDonationContext : DbContext
{
    public BloodDonationContext()
    {
    }

    public BloodDonationContext(DbContextOptions<BloodDonationContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BlogPost> BlogPosts { get; set; }

    public virtual DbSet<BloodComponent> BloodComponents { get; set; }

    public virtual DbSet<BloodInventory> BloodInventories { get; set; }

    public virtual DbSet<BloodRequest> BloodRequests { get; set; }

    public virtual DbSet<BloodRequestInventory> BloodRequestInventories { get; set; }

    public virtual DbSet<BloodType> BloodTypes { get; set; }

    public virtual DbSet<DonationHistory> DonationHistories { get; set; }

    public virtual DbSet<DonationRequest> DonationRequests { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<RequestMatch> RequestMatches { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=localhost\\SQLEXPRESS;Initial Catalog=BloodDonation;User ID=ad;Password=123;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__BlogPost__3ED787668B69A509");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.BlogPosts).HasConstraintName("FK__BlogPost__user_i__6EF57B66");
        });

        modelBuilder.Entity<BloodComponent>(entity =>
        {
            entity.HasKey(e => e.BloodComponentId).HasName("PK__BloodCom__14A61BEFD91EAD6D");
        });

        modelBuilder.Entity<BloodInventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("PK__BloodInv__B59ACC49359E3CED");

            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.BloodComponent).WithMany(p => p.BloodInventories).HasConstraintName("FK__BloodInve__blood__5AEE82B9");

            entity.HasOne(d => d.BloodType).WithMany(p => p.BloodInventories).HasConstraintName("FK__BloodInve__blood__59FA5E80");
        });

        modelBuilder.Entity<BloodRequest>(entity =>
        {
            entity.HasKey(e => e.BloodRequestId).HasName("PK__BloodReq__0F0E510DE22697BE");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Fulfilled).HasDefaultValue(false);

            entity.HasOne(d => d.BloodComponent).WithMany(p => p.BloodRequests).HasConstraintName("FK__BloodRequ__blood__534D60F1");

            entity.HasOne(d => d.BloodType).WithMany(p => p.BloodRequests).HasConstraintName("FK__BloodRequ__blood__52593CB8");

            entity.HasOne(d => d.User).WithMany(p => p.BloodRequests).HasConstraintName("FK__BloodRequ__user___5165187F");
        });

        modelBuilder.Entity<BloodRequestInventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BloodReq__3213E83F6F3F0B08");

            entity.Property(e => e.AllocatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.AllocatedByNavigation).WithMany(p => p.BloodRequestInventories).HasConstraintName("FK__BloodRequ__alloc__619B8048");

            entity.HasOne(d => d.BloodRequest).WithMany(p => p.BloodRequestInventories).HasConstraintName("FK__BloodRequ__blood__5EBF139D");

            entity.HasOne(d => d.Inventory).WithMany(p => p.BloodRequestInventories).HasConstraintName("FK__BloodRequ__inven__5FB337D6");
        });

        modelBuilder.Entity<BloodType>(entity =>
        {
            entity.HasKey(e => e.BloodTypeId).HasName("PK__BloodTyp__56FFB8C8F2AED018");
        });

        modelBuilder.Entity<DonationHistory>(entity =>
        {
            entity.HasKey(e => e.DonationId).HasName("PK__Donation__296B91DCE12F47BB");

            entity.HasOne(d => d.BloodComponent).WithMany(p => p.DonationHistories).HasConstraintName("FK__DonationH__blood__46E78A0C");

            entity.HasOne(d => d.BloodType).WithMany(p => p.DonationHistories).HasConstraintName("FK__DonationH__blood__45F365D3");

            entity.HasOne(d => d.User).WithMany(p => p.DonationHistories).HasConstraintName("FK__DonationH__user___44FF419A");
        });

        modelBuilder.Entity<DonationRequest>(entity =>
        {
            entity.HasKey(e => e.DonateRequestId).HasName("PK__Donation__D517757ABCD67DE3");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.BloodComponent).WithMany(p => p.DonationRequests).HasConstraintName("FK__DonationR__blood__4CA06362");

            entity.HasOne(d => d.BloodType).WithMany(p => p.DonationRequests).HasConstraintName("FK__DonationR__blood__4BAC3F29");

            entity.HasOne(d => d.User).WithMany(p => p.DonationRequests).HasConstraintName("FK__DonationR__user___4AB81AF0");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842F40F16515");

            entity.Property(e => e.SentAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications).HasConstraintName("FK__Notificat__user___6A30C649");
        });

        modelBuilder.Entity<RequestMatch>(entity =>
        {
            entity.HasKey(e => e.MatchId).HasName("PK__RequestM__9D7FCBA3366FE302");

            entity.HasOne(d => d.BloodRequest).WithMany(p => p.RequestMatches).HasConstraintName("FK__RequestMa__blood__6477ECF3");

            entity.HasOne(d => d.DonationRequest).WithMany(p => p.RequestMatches).HasConstraintName("FK__RequestMa__donat__656C112C");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__B9BE370F82B4A317");

            entity.HasOne(d => d.BloodComponent).WithMany(p => p.Users).HasConstraintName("FK__User__blood_comp__403A8C7D");

            entity.HasOne(d => d.BloodType).WithMany(p => p.Users).HasConstraintName("FK__User__blood_type__3F466844");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
