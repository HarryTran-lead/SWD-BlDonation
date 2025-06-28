using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SWD_BLDONATION.Models.Generated;

public partial class BloodDonationDbContext : DbContext
{
    public BloodDonationDbContext()
    {
    }

    public BloodDonationDbContext(DbContextOptions<BloodDonationDbContext> options)
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
        => optionsBuilder.UseSqlServer("Data Source=localhost\\SQLEXPRESS;Initial Catalog=BloodDonation;User ID=ad;Password=123;TrustServerCertificate=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__BlogPost__3ED787666B962BA9");

            entity.ToTable("BlogPost");

            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("category");
            entity.Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.BlogPosts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__BlogPost__user_i__440B1D61");
        });

        modelBuilder.Entity<BloodComponent>(entity =>
        {
            entity.HasKey(e => e.BloodComponentId).HasName("PK__BloodCom__14A61BEFE76BC864");

            entity.ToTable("BloodComponent");

            entity.HasIndex(e => e.Name, "UQ_BloodComponent_name").IsUnique();

            entity.Property(e => e.BloodComponentId).HasColumnName("blood_component_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<BloodInventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("PK__BloodInv__B59ACC49C1B2E1D3");

            entity.ToTable("BloodInventory");

            entity.Property(e => e.InventoryId).HasColumnName("inventory_id");
            entity.Property(e => e.BloodComponentId).HasColumnName("blood_component_id");
            entity.Property(e => e.BloodTypeId).HasColumnName("blood_type_id");
            entity.Property(e => e.InventoryLocation)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("inventory_location");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("last_updated");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Unit)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("unit");

            entity.HasOne(d => d.BloodComponent).WithMany(p => p.BloodInventories)
                .HasForeignKey(d => d.BloodComponentId)
                .HasConstraintName("FK__BloodInve__blood__46E78A0C");

            entity.HasOne(d => d.BloodType).WithMany(p => p.BloodInventories)
                .HasForeignKey(d => d.BloodTypeId)
                .HasConstraintName("FK__BloodInve__blood__45F365D3");
        });

        modelBuilder.Entity<BloodRequest>(entity =>
        {
            entity.HasKey(e => e.BloodRequestId).HasName("PK__BloodReq__0F0E510DB8410CD1");

            entity.ToTable("BloodRequest");

            entity.Property(e => e.BloodRequestId).HasColumnName("blood_request_id");
            entity.Property(e => e.BloodComponentId).HasColumnName("blood_component_id");
            entity.Property(e => e.BloodTypeId).HasColumnName("blood_type_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Fulfilled)
                .HasDefaultValue(false)
                .HasColumnName("fulfilled");
            entity.Property(e => e.FulfilledSource)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("fulfilled_source");
            entity.Property(e => e.HealthInfo).HasColumnName("health_info");
            entity.Property(e => e.HeightCm)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("height_cm");
            entity.Property(e => e.IsEmergency).HasColumnName("is_emergency");
            entity.Property(e => e.Location).HasColumnName("location");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Status)
                .HasDefaultValue((byte)0)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WeightKg)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("weight_kg");

            entity.HasOne(d => d.BloodComponent).WithMany(p => p.BloodRequests)
                .HasForeignKey(d => d.BloodComponentId)
                .HasConstraintName("FK__BloodRequ__blood__4AB81AF0");

            entity.HasOne(d => d.BloodType).WithMany(p => p.BloodRequests)
                .HasForeignKey(d => d.BloodTypeId)
                .HasConstraintName("FK__BloodRequ__blood__49C3F6B7");

            entity.HasOne(d => d.User).WithMany(p => p.BloodRequests)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__BloodRequ__user___4D94879B");
        });

        modelBuilder.Entity<BloodRequestInventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BloodReq__3213E83FAA6AFCE1");

            entity.ToTable("BloodRequestInventory");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AllocatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("allocated_at");
            entity.Property(e => e.AllocatedBy).HasColumnName("allocated_by");
            entity.Property(e => e.BloodRequestId).HasColumnName("blood_request_id");
            entity.Property(e => e.InventoryId).HasColumnName("inventory_id");
            entity.Property(e => e.QuantityAllocated).HasColumnName("quantity_allocated");
            entity.Property(e => e.QuantityUnit).HasColumnName("quantity_unit");

            entity.HasOne(d => d.AllocatedByNavigation).WithMany(p => p.BloodRequestInventories)
                .HasForeignKey(d => d.AllocatedBy)
                .HasConstraintName("FK__BloodRequ__alloc__4F7CD00D");

            entity.HasOne(d => d.BloodRequest).WithMany(p => p.BloodRequestInventories)
                .HasForeignKey(d => d.BloodRequestId)
                .HasConstraintName("FK__BloodRequ__blood__5165187F");

            entity.HasOne(d => d.Inventory).WithMany(p => p.BloodRequestInventories)
                .HasForeignKey(d => d.InventoryId)
                .HasConstraintName("FK__BloodRequ__inven__534D60F1");
        });

        modelBuilder.Entity<BloodType>(entity =>
        {
            entity.HasKey(e => e.BloodTypeId).HasName("PK__BloodTyp__56FFB8C8DBA8E22D");

            entity.ToTable("BloodType");

            entity.Property(e => e.BloodTypeId).HasColumnName("blood_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.RhFactor)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("rh_factor");
        });

        modelBuilder.Entity<DonationHistory>(entity =>
        {
            entity.HasKey(e => e.DonationId).HasName("PK__Donation__296B91DCB8AFFADD");

            entity.ToTable("DonationHistory");

            entity.Property(e => e.DonationId).HasColumnName("donation_id");
            entity.Property(e => e.BloodComponentId).HasColumnName("blood_component_id");
            entity.Property(e => e.BloodTypeId).HasColumnName("blood_type_id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VolumeMl).HasColumnName("volume_ml");

            entity.HasOne(d => d.BloodComponent).WithMany(p => p.DonationHistories)
                .HasForeignKey(d => d.BloodComponentId)
                .HasConstraintName("FK__DonationH__blood__5629CD9C");

            entity.HasOne(d => d.BloodType).WithMany(p => p.DonationHistories)
                .HasForeignKey(d => d.BloodTypeId)
                .HasConstraintName("FK__DonationH__blood__5535A963");

            entity.HasOne(d => d.User).WithMany(p => p.DonationHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__DonationH__user___59063A47");
        });

        modelBuilder.Entity<DonationRequest>(entity =>
        {
            entity.HasKey(e => e.DonateRequestId).HasName("PK__Donation__D517757A35686082");

            entity.ToTable("DonationRequest");

            entity.Property(e => e.DonateRequestId).HasColumnName("donate_request_id");
            entity.Property(e => e.BloodComponentId).HasColumnName("blood_component_id");
            entity.Property(e => e.BloodTypeId).HasColumnName("blood_type_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.HealthInfo)
                .HasColumnType("text")
                .HasColumnName("health_info");
            entity.Property(e => e.HeightCm)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("height_cm");
            entity.Property(e => e.LastDonationDate).HasColumnName("last_donation_date");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("location");
            entity.Property(e => e.Note)
                .HasColumnType("text")
                .HasColumnName("note");
            entity.Property(e => e.PreferredDate).HasColumnName("preferred_date");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WeightKg)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("weight_kg");

            entity.HasOne(d => d.BloodComponent).WithMany(p => p.DonationRequests)
                .HasForeignKey(d => d.BloodComponentId)
                .HasConstraintName("FK__DonationR__blood__5BE2A6F2");

            entity.HasOne(d => d.BloodType).WithMany(p => p.DonationRequests)
                .HasForeignKey(d => d.BloodTypeId)
                .HasConstraintName("FK__DonationR__blood__5AEE82B9");

            entity.HasOne(d => d.User).WithMany(p => p.DonationRequests)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__DonationR__user___5EBF139D");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842F3E8ECE58");

            entity.ToTable("Notification");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.Message)
                .HasColumnType("text")
                .HasColumnName("message");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("sent_at");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__user___60A75C0F");
        });

        modelBuilder.Entity<RequestMatch>(entity =>
        {
            entity.HasKey(e => e.MatchId).HasName("PK__RequestM__9D7FCBA3EBFF3336");

            entity.ToTable("RequestMatch");

            entity.Property(e => e.MatchId).HasColumnName("match_id");
            entity.Property(e => e.BloodRequestId).HasColumnName("blood_request_id");
            entity.Property(e => e.DonationRequestId).HasColumnName("donation_request_id");
            entity.Property(e => e.MatchStatus)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("match_status");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.ScheduledDate).HasColumnName("scheduled_date");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("type");

            entity.HasOne(d => d.BloodRequest).WithMany(p => p.RequestMatches)
                .HasForeignKey(d => d.BloodRequestId)
                .HasConstraintName("FK__RequestMa__blood__628FA481");

            entity.HasOne(d => d.DonationRequest).WithMany(p => p.RequestMatches)
                .HasForeignKey(d => d.DonationRequestId)
                .HasConstraintName("FK__RequestMa__donat__6477ECF3");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__B9BE370FF17EABBA");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ_User_Email_NotNull")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.HasIndex(e => e.Identification, "UQ_User_Identification_NotNull")
                .IsUnique()
                .HasFilter("([Identification] IS NOT NULL)");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.BloodComponentId).HasColumnName("blood_component_id");
            entity.Property(e => e.BloodTypeId).HasColumnName("blood_type_id");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.HeightCm).HasColumnName("height_cm");
            entity.Property(e => e.Identification)
                .HasMaxLength(50)
                .HasColumnName("identification");
            entity.Property(e => e.MedicalHistory).HasColumnName("medical_history");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.RoleBit).HasColumnName("role_bit");
            entity.Property(e => e.StatusBit).HasColumnName("status_bit");
            entity.Property(e => e.UserName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("user_name");
            entity.Property(e => e.WeightKg).HasColumnName("weight_kg");

            entity.HasOne(d => d.BloodComponent).WithMany(p => p.Users)
                .HasForeignKey(d => d.BloodComponentId)
                .HasConstraintName("FK__User__blood_comp__66603565");

            entity.HasOne(d => d.BloodType).WithMany(p => p.Users)
                .HasForeignKey(d => d.BloodTypeId)
                .HasConstraintName("FK__User__blood_type__68487DD7");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
