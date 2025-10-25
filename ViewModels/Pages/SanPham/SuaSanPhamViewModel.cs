using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QuanLyKhoHang.Models;
using QuanLyKhoHang.Models.Messages;
using QuanLyKhoHang.Views.Pages.SanPham;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using UiDesktopApp1.Models;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;

namespace UiDesktopApp1.ViewModels.Pages.SanPham
{
    public partial class SuaSanPhamViewModel : ObservableObject, IRecipient<EditProductMessage>
    {
        private readonly INavigationService _nav;
        private readonly AppDbContext _db;

        [ObservableProperty]
        private ProductModel product = new(); // Sản phẩm đang được chỉnh sửa

        [ObservableProperty]
        private ObservableCollection<CategoryModel> categories = new();

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private BitmapImage? image;

        public SuaSanPhamViewModel(INavigationService nav, AppDbContext db)
        {
            _nav = nav;
            _db = db;

            WeakReferenceMessenger.Default.Register<EditProductMessage>(this);

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

        public void Receive(EditProductMessage message)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                await LoadProductAsync(message.ProductId);
            });
        }

        public async Task LoadProductAsync(int productId)
        {
            IsBusy = true;

            var productFromDb = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if(productFromDb != null)
            {
                Product = productFromDb;
                Image = LoadBitmap(Product.ImagePath);
            } else
            {
                MessageBox.Show("Không tìm thấy sản phẩm.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                Cancel();
            }
            IsBusy = false;
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

        [RelayCommand]
        public async Task SaveAsync()
        {
            Product.ValidateAll();
            if (Product.HasErrors)
            {
                var allErrors = Product.GetErrors()
                                       .Select(e => e.ErrorMessage)
                                       .Where(msg => !string.IsNullOrWhiteSpace(msg))
                                       .Distinct();
                var errorText = string.Join("\n• ", allErrors);
                MessageBox.Show("Vui lòng sửa các lỗi sau trước khi lưu:\n• " + errorText,
                    "Dữ liệu chưa hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _db.Products.Update(Product);
                await _db.SaveChangesAsync();

                // Gửi tin nhắn để trang danh sách tự làm mới
                WeakReferenceMessenger.Default.Send(new ProductsNeedRefreshMessage());

                //Điều hướng về trang quản lý
                _nav.Navigate(typeof(QuanLySanPhamPage));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật dữ liệu:\n{ex.Message}",
                    "KhoPro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            // Hủy các thay đổi đang theo dõi
            var entry = _db.Entry(Product);
            if (entry.State == EntityState.Modified)
            {
                entry.Reload();
            }

            _nav.Navigate(typeof(QuanLySanPhamPage));
        }

        private static BitmapImage? LoadBitmap(string? path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path)) return null;

                // Hỗ trợ cả file path và pack URI
                if (path.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                {
                    return new BitmapImage(new Uri(path, UriKind.Absolute));
                }

                if (File.Exists(path))
                {
                    return new BitmapImage(new Uri(Path.GetFullPath(path), UriKind.Absolute));
                }

                // Nếu không phải cả hai, thử tải ảnh mặc định
                return new BitmapImage(new Uri("pack://application:,,,/Assets/Images/logo-image.png", UriKind.Absolute));
            }
            catch
            {
                return new BitmapImage(new Uri("pack://application:,,,/Assets/Images/logo-image.png", UriKind.Absolute));
            }
        }

        [RelayCommand]
        private void GenerateRandomCode()
        {
            Product.ProductCode = $"SP-{DateTime.Now:yyMMddHHmmss}";
        }
    }
}