using Microsoft.EntityFrameworkCore;

namespace blazor_app.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<KitType> KitTypes => Set<KitType>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCharSet("utf8mb4");

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Label).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.ToTable("brands");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Label).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("teams");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Label).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<League>(entity =>
        {
            entity.ToTable("leagues");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Label).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<KitType>(entity =>
        {
            entity.ToTable("kit_types");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Label).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<Season>(entity =>
        {
            entity.ToTable("seasons");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.OldPrice).HasPrecision(18, 2);
            entity.Property(e => e.SourceUrl).HasMaxLength(1024).IsRequired();

            entity.HasOne(e => e.Brand).WithMany(b => b.Products)
                .HasForeignKey(e => e.BrandId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Category).WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Team).WithMany(t => t.Products)
                .HasForeignKey(e => e.TeamId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.League).WithMany(l => l.Products)
                .HasForeignKey(e => e.LeagueId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.KitType).WithMany(k => k.Products)
                .HasForeignKey(e => e.KitTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Season).WithMany(s => s.Products)
                .HasForeignKey(e => e.SeasonId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("product_images");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).HasMaxLength(512).IsRequired();
            entity.HasOne(e => e.Product).WithMany(p => p.Images)
                .HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(500).IsRequired();
            entity.Property(e => e.PayMethod)
                .HasConversion(v => v == PayMethod.Online ? "online" : "cash",
                               v => v == "online" ? PayMethod.Online : PayMethod.Cash)
                .HasMaxLength(32).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Password).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Role)
                .HasConversion(v => v == UserRole.Admin ? "admin" : "buyer",
                               v => v == "admin" ? UserRole.Admin : UserRole.Buyer)
                .HasMaxLength(32).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Address).HasMaxLength(500).IsRequired();
            entity.Property(e => e.PayMethod).HasMaxLength(32).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(32).IsRequired();
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.HasOne(e => e.User).WithMany(u => u.Orders)
                .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Items).WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.ToTable("order_details");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Size).HasConversion<string>().HasMaxLength(16).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasOne(e => e.Product).WithMany()
                .HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("cart_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Size).HasConversion<string>().HasMaxLength(16).IsRequired();
            entity.HasOne(e => e.User).WithMany(u => u.CartItems)
                .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product).WithMany()
                .HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.ProductId, e.Size }).IsUnique();
        });
    }
}
