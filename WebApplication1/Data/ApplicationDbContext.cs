using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<SmsNotification> SmsNotifications { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure Appointment entity
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                    
                entity.Property(e => e.Description)
                    .HasMaxLength(1000);
                    
                entity.Property(e => e.ClientName)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(e => e.ClientPhone)
                    .HasMaxLength(15);
                    
                entity.Property(e => e.ClientEmail)
                    .HasMaxLength(100);
                    
                entity.Property(e => e.Location)
                    .HasMaxLength(200);
                    
                entity.Property(e => e.Notes)
                    .HasMaxLength(500);
                    
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("datetime('now')");
                
                // Foreign key relationship with ApplicationUser
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                // Index for better query performance
                entity.HasIndex(e => e.AppointmentDate);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ClientName);
            });

            // Configure Client entity
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.RegistrationNumber)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(15);
                    
                entity.Property(e => e.ValidityType)
                    .HasConversion<int>();
                    
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("datetime('now')");

                // Foreign key relationship with ApplicationUser
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Relationship with SmsNotifications
                entity.HasMany(e => e.SmsNotifications)
                    .WithOne(s => s.Client)
                    .HasForeignKey(s => s.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Index for better query performance
                entity.HasIndex(e => e.RegistrationNumber).IsUnique();
                entity.HasIndex(e => e.ExpiryDate);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.PhoneNumber);
            });

            // Configure SmsNotification entity
            modelBuilder.Entity<SmsNotification>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.ErrorMessage)
                    .HasMaxLength(500);

                entity.Property(e => e.Status)
                    .HasConversion<int>();

                entity.Property(e => e.Type)
                    .HasConversion<int>();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("datetime('now')");

                // Foreign key relationship with Client
                entity.HasOne(e => e.Client)
                    .WithMany(c => c.SmsNotifications)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign key relationship with ApplicationUser
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes for better query performance
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.ScheduledFor);

                // Unique constraint to prevent duplicate notifications
                entity.HasIndex(e => new { e.ClientId, e.Type, e.ExpiryDateSnapshot })
                    .HasDatabaseName("IX_SmsNotification_Client_Type_ExpirySnapshot")
                    .IsUnique();
            });
        }
    }
}