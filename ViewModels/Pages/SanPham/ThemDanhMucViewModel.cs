using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QuanLyKhoHang.Models;
using QuanLyKhoHang.Models.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiDesktopApp1.Models;
using Wpf.Ui;

namespace QuanLyKhoHang.ViewModels.Pages.SanPham
{
    public partial class ThemDanhMucViewModel : ObservableObject
    {
        private readonly AppDbContext _db;
        private readonly INavigationService _nav;

        [ObservableProperty]
        private string? name;

        public ThemDanhMucViewModel(AppDbContext db, INavigationService nav)
        {
            _db = db;
            _nav = nav;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            var n = (Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(n))
            {
                MessageBox.Show("Không được để trống tên danh mục", "Dữ liệu chưa hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                // (tuỳ chọn) kiểm tra trùng tên
                var existed = await _db.Categories.AnyAsync(c => c.Name == n);
                if (existed)
                {
                    // TODO: bắn snackbar nếu bạn có service thông báo
                    MessageBox.Show("Đã tồn tại danh mục này", "Dữ liệu chưa hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var cat = new CategoryModel
                {
                    Name = n,
                };

                _db.Categories.Add(cat);
                await _db.SaveChangesAsync();

                // Gửi “tin nhắn” về trang trước để tự chọn danh mục vừa tạo
                WeakReferenceMessenger.Default.Send(new CategoryCreatedMessage(cat));

                _nav.Navigate(typeof(UiDesktopApp1.Views.Pages.SanPham.ThemSanPhamPage));
            }
            finally
            {
                
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            _nav.Navigate(typeof(UiDesktopApp1.Views.Pages.SanPham.ThemSanPhamPage));
        }
    }
}
