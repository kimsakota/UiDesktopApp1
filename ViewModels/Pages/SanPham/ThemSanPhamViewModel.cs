using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using UiDesktopApp1.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace UiDesktopApp1.ViewModels.Pages.SanPham
{
    public partial class ThemSanPhamViewModel : ObservableObject
    {
        //[ObservableProperty]
        //private BitmapImage? image;

        private readonly INavigationService _nav;
        private readonly AppDbContext _db;

        [ObservableProperty] private ProductModel product = new();   // bind form vào đây
        [ObservableProperty] private BitmapImage? image;             // ảnh preview

        public ThemSanPhamViewModel(INavigationService nav, AppDbContext db)
        {
            _nav = nav;
            _db = db;  

            ResetForm();
        }
        /*private void LoadDefaultImage()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/Assets/Images/logo-image.png", UriKind.Absolute);
                Product.Image = new BitmapImage(uri);
                Product.ImagePath = uri.ToString();
            }
            catch { }

        }*/


        private void ResetForm()
        {
            Product = new ProductModel
            {
                ProductCode = null,
                ProductName = null,
                InitialQty = 0,
                SafeQty = 0,
                CostPrice = 0,
                SalePrice = 0,
                ImagePath = "pack://application:,,,/Assets/Images/logo-image.png"
            };

            try
            {
                Image = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/logo-image.png", UriKind.Absolute));
            }
            catch
            {
                Image = null;
            }
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
            Product.ValidateAll();

            /*System.Diagnostics.Debug.WriteLine(
            $"Save: Code={Product.ProductCode ?? "<null>"}, Name={Product.ProductName ?? "<null>"}");*/

            if (Product.HasErrors)
            {
                // Gom các lỗi lại để hiển thị
                var allErrors = Product.GetErrors()
                                       .Select(e => e.ErrorMessage)
                                       .Where(msg => !string.IsNullOrWhiteSpace(msg))
                                       .Distinct();

                var errorText = string.Join("\n• ", allErrors);
                System.Windows.MessageBox.Show("Vui lòng sửa các lỗi sau trước khi lưu:\n• " + errorText,
                    "Dữ liệu chưa hợp lệ", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _db.Products.AddAsync(Product);
                await _db.SaveChangesAsync();

                //System.Windows.MessageBox.Show("Đã lưu sản phẩm thành công!", "KhoPro",
                //    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                ResetForm();
                //Điều hướng về trang danh sách
                _nav.Navigate(typeof(UiDesktopApp1.Views.Pages.SanPhamPage));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi lưu dữ liệu:\n{ex.Message}",
                    "KhoPro", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
            }

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

        [RelayCommand]
        private void GenerateRandomCode()
        {
            Product.ProductCode = $"SP-{DateTime.Now:yyMMddHHmmss}";
        }
    }
}