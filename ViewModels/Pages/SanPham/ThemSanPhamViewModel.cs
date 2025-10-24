using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QuanLyKhoHang.Models;
using QuanLyKhoHang.Models.Messages;
using QuanLyKhoHang.Views.Pages.SanPham;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        [ObservableProperty] private ObservableCollection<CategoryModel> categories = new();
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private BitmapImage? image = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/logo-image.png", UriKind.Absolute));

        // --- State để thêm danh mục inline ---
        [ObservableProperty] private bool isAddingCategory;
        [ObservableProperty] private string? newCategoryName;


        public ThemSanPhamViewModel(INavigationService nav, AppDbContext db)
        {
            _nav = nav;
            _db = db;

            // Lắng nghe “đã tạo danh mục” từ trang ThemDanhMucPage
            WeakReferenceMessenger.Default.Register<CategoryCreatedMessage>(this, (r, m) =>
            {
                // Cập nhật danh sách + chọn ngay danh mục mới
                Categories.Add(m.Value);
                Product.CategoryId = m.Value.Id;
                OnPropertyChanged(nameof(Product));
            });
            _ = LoadCategoriesAsync();
        }

        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            IsBusy = true;
            try
            {
                Categories.Clear();
                var list = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
                foreach (var item in list)
                    Categories.Add(item);
            }
            finally { IsBusy = false; }
           
        }

        // Điều hướng sang trang thêm danh mục
        [RelayCommand]
        private void GoAddCategory()
        {
            _nav.Navigate(typeof(ThemDanhMucPage));
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

        private void ResetForm()
        {
            Product = new ProductModel();
            Image = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/logo-image.png", UriKind.Absolute));
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
                WeakReferenceMessenger.Default.Send(new ProductCreatedMessage(Product));

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
            ResetForm();
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

        // Regex chỉ cho phép số và dấu '/'
        private static readonly Regex Allowed = new(@"^[0-9/]+$");

        [RelayCommand]
        private void DatePreviewTextInput(TextCompositionEventArgs e)
        {
            if (e == null) return;
            e.Handled = !Allowed.IsMatch(e.Text);
        }

        [RelayCommand]
        private void DatePasting(DataObjectPastingEventArgs e)
        {
            if (e == null) return;

            if (e.DataObject.GetDataPresent(DataFormats.UnicodeText))
            {
                var text = (string)e.DataObject.GetData(DataFormats.UnicodeText);
                if (!Allowed.IsMatch(text))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

    }
}