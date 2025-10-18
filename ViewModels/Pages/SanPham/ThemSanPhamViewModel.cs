using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using UiDesktopApp1.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace UiDesktopApp1.ViewModels.Pages.SanPham
{
    public partial class ThemSanPhamViewModel : ObservableValidator
    {
        //[ObservableProperty]
        //private BitmapImage? image;

        private readonly INavigationService _nav;
        private readonly AppDbContext _db;

        [ObservableProperty] private ProductModel product = new();   // bind form vào đây
        [ObservableProperty] private BitmapImage? image;             // ảnh preview
        [ObservableProperty] private bool isEditing = true;          // để nút "Thêm mới" enable

        public ThemSanPhamViewModel(INavigationService nav, AppDbContext db)
        {
            _nav = nav;
            _db = db;
            // Giá trị mặc định nếu cần
            Product.InitialQty = 0;
            Product.SafeQty = 0;
            Product.CostPrice = 0;
            Product.SalePrice = 0;
        }
        private void LoadDefaultImage()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/Assets/Images/logo-image.png", UriKind.Absolute);
                Product.Image = new BitmapImage(uri);
            }
            catch { }

        }

        [RelayCommand]
        private void OpenPicture()
        {
            var ofd = new OpenFileDialog()
            {
                Title = "Chọn ảnh sản phẩm",
                Filter = "Ảnh|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
                Multiselect = false
            };
            if (ofd.ShowDialog() == true)
            {
                Product.ImagePath = ofd.FileName;
                Image = LoadBitmap(ofd.FileName);
            }
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            /*if (Product is null)
                return;

            // ✅ Kiểm tra khi bấm nút
            Product.ValidateAll();

            if (Product.HasErrors)
            {
                // Ví dụ: hiện Snackbar WPF-UI


                return; // không lưu nếu có lỗi
            }*/

            await _db.Products.AddAsync(Product);
            //await _db.SaveChangesAsync();

            // Điều hướng về danh sách
            _nav.Navigate(typeof(UiDesktopApp1.Views.Pages.SanPhamPage));
        }


        [RelayCommand]
        private void Cancel()
        {
            _nav.Navigate(typeof(UiDesktopApp1.Views.Pages.SanPhamPage));
        }

        private static BitmapImage? LoadBitmap(string? path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
                return new BitmapImage(new Uri(Path.GetFullPath(path), UriKind.Absolute));
            }
            catch { return null; }
        }
    }
}
