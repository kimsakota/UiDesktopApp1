using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiDesktopApp1.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<ProductModel> Products => Set<ProductModel>();
        public DbSet<CategoryModel> Categories => Set<CategoryModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductModel>()
                .HasIndex(p => p.ProductCode)
                .IsUnique(false);

            modelBuilder.Entity<ProductModel>()
                .HasOne(p => p.Category)
                .WithMany(p => p.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull); // Xoá danh mục -> CategoryId của sản phẩm = NULL

            base.OnModelCreating(modelBuilder);
        }
    }
}