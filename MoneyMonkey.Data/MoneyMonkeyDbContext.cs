using Microsoft.EntityFrameworkCore;
using MoneyMonkey.Data.Entities;

namespace MoneyMonkey.Data;

public class MoneyMonkeyDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Credential> Credentials => Set<Credential>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<CreditCard> CreditCards => Set<CreditCard>();
    public DbSet<CreditCardPurchase> CreditCardPurchases => Set<CreditCardPurchase>();
    public DbSet<CreditCardInstallment> CreditCardInstallments => Set<CreditCardInstallment>();

    public MoneyMonkeyDbContext(DbContextOptions<MoneyMonkeyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.UserId).HasColumnName("user_id");
            entity.Property(u => u.FirstName).HasColumnName("first_name");
            entity.Property(u => u.LastName).HasColumnName("last_name");
            entity.Property(u => u.Type).HasColumnName("type").HasConversion<string>();
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
            entity.Property(c => c.Type).HasColumnName("type").HasConversion<string>();
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
            entity.Property(t => t.Type).HasColumnName("type").HasConversion<string>();
            entity.Property(t => t.PaymentMethod).HasColumnName("payment_method").HasConversion<string>();
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

        modelBuilder.Entity<CreditCard>(entity =>
        {
            entity.ToTable("credit_cards");
            entity.HasKey(c => c.CreditCardId);
            entity.Property(c => c.CreditCardId).HasColumnName("credit_card_id");
            entity.Property(c => c.UserId).HasColumnName("user_id");
            entity.Property(c => c.Name).HasColumnName("name");
            entity.Property(c => c.Brand).HasColumnName("brand").HasConversion<string>();
            entity.Property(c => c.LastFourDigits).HasColumnName("last_four_digits");
            entity.Property(c => c.ClosingDay).HasColumnName("closing_day");
            entity.Property(c => c.DueDay).HasColumnName("due_day");
            entity.Property(c => c.CreditLimit).HasColumnName("credit_limit");
            entity.Property(c => c.CreatedAt).HasColumnName("created_at").ValueGeneratedOnAdd();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.UserId);
        });

        modelBuilder.Entity<CreditCardPurchase>(entity =>
        {
            entity.ToTable("credit_card_purchases");
            entity.HasKey(p => p.CreditCardPurchaseId);
            entity.Property(p => p.CreditCardPurchaseId).HasColumnName("credit_card_purchase_id");
            entity.Property(p => p.UserId).HasColumnName("user_id");
            entity.Property(p => p.CreditCardId).HasColumnName("credit_card_id");
            entity.Property(p => p.Description).HasColumnName("description");
            entity.Property(p => p.TotalValue).HasColumnName("total_value");
            entity.Property(p => p.PurchaseDate).HasColumnName("purchase_date");
            entity.Property(p => p.InstallmentsCount).HasColumnName("installments_count");
            entity.Property(p => p.CategoryId).HasColumnName("category_id");
            entity.Property(p => p.IsSubscription).HasColumnName("is_subscription");
            entity.Property(p => p.CreatedAt).HasColumnName("created_at").ValueGeneratedOnAdd();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.UserId);

            entity.HasOne<CreditCard>()
                .WithMany()
                .HasForeignKey(p => p.CreditCardId);

            entity.HasOne<Category>()
                .WithMany()
                .HasForeignKey(p => p.CategoryId);
        });

        modelBuilder.Entity<CreditCardInstallment>(entity =>
        {
            entity.ToTable("credit_card_installments");
            entity.HasKey(i => i.CreditCardInstallmentId);
            entity.Property(i => i.CreditCardInstallmentId).HasColumnName("credit_card_installment_id");
            entity.Property(i => i.CreditCardPurchaseId).HasColumnName("credit_card_purchase_id");
            entity.Property(i => i.InstallmentNumber).HasColumnName("installment_number");
            entity.Property(i => i.Value).HasColumnName("value");
            entity.Property(i => i.InvoiceMonth).HasColumnName("invoice_month");
            entity.Property(i => i.InvoiceYear).HasColumnName("invoice_year");

            entity.HasOne<CreditCardPurchase>()
                .WithMany()
                .HasForeignKey(i => i.CreditCardPurchaseId);
        });
    }
}
