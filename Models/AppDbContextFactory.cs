﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiDesktopApp1.Models
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Lấy đường dẫn thư mục gốc đang chạy migration (thường là thư mục chứa .csproj)
            var basePath = Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                  ?? "Server=localhost\\SQLEXPRESS;Database=QuanLyKhoHang;Trusted_Connection=True;TrustServerCertificate=True";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
