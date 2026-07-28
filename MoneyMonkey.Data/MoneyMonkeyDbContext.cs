using Microsoft.EntityFrameworkCore;
using MoneyMonkey.Communication.Enums;
using MoneyMonkey.Data.Entities;

namespace MoneyMonkey.Data;

public class MoneyMonkeyDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Credential> Credentials => Set<Credential>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    public MoneyMonkeyDbContext(DbContextOptions<MoneyMonkeyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<UserType>("public", "user_type");
        modelBuilder.HasPostgresEnum<TransactionType>("public", "transaction_type");
        modelBuilder.HasPostgresEnum<PaymentMethod>("public", "payment_method");

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.UserId).HasColumnName("user_id");
            entity.Property(u => u.FirstName).HasColumnName("first_name");
            entity.Property(u => u.LastName).HasColumnName("last_name");
            entity.Property(u => u.Type).HasColumnName("type").HasColumnType("user_type");
        });

        modelBuilder.Entity<Credential>(entity =>
        {
            entity.ToTable("credentials");
            entity.HasKey(c => c.CredentialId);
            entity.Property(c => c.CredentialId).HasColumnName("credential_id");
            entity.Property(c => c.UserId).HasColumnName("user_id");
            entity.Property(c => c.Username).HasColumnName("username");
            entity.Property(c => c.Password).HasColumnName("password");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(c => c.CategoryId);
            entity.Property(c => c.CategoryId).HasColumnName("category_id");
            entity.Property(c => c.UserId).HasColumnName("user_id");
            entity.Property(c => c.Name).HasColumnName("name");
            entity.Property(c => c.Type).HasColumnName("type").HasColumnType("transaction_type");
            entity.Property(c => c.CreatedAt).HasColumnName("created_at").ValueGeneratedOnAdd();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.UserId);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("transactions");
            entity.HasKey(t => t.TransactionId);
            entity.Property(t => t.TransactionId).HasColumnName("transaction_id");
            entity.Property(t => t.UserId).HasColumnName("user_id");
            entity.Property(t => t.TransactionName).HasColumnName("transaction_name");
            entity.Property(t => t.Value).HasColumnName("value");
            entity.Property(t => t.Type).HasColumnName("type").HasColumnType("transaction_type");
            entity.Property(t => t.PaymentMethod).HasColumnName("payment_method").HasColumnType("payment_method");
            entity.Property(t => t.CategoryId).HasColumnName("category_id");
            entity.Property(t => t.TransactionDate).HasColumnName("transaction_date");
            entity.Property(t => t.CreatedAt).HasColumnName("created_at").ValueGeneratedOnAdd();
            entity.Property(t => t.UpdatedAt).HasColumnName("updated_at").ValueGeneratedOnAdd();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.UserId);

            entity.HasOne<Category>()
                .WithMany()
                .HasForeignKey(t => t.CategoryId);
        });
    }
}
