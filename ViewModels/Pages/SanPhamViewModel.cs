using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using UiDesktopApp1.Models;
using UiDesktopApp1.ViewModels.Pages.SanPham;
using Wpf.Ui;

namespace UiDesktopApp1.ViewModels.Pages
{
    public partial class SanPhamViewModel : ObservableObject 
    {
        private readonly INavigationService _navigationService;
        private readonly AppDbContext _db;
        private readonly ICollectionView _productsView;

        // Danh sách nguồn
        public ObservableCollection<ProductModel> Products { get; } = new();

        public ICollectionView ProductsView => _productsView;

        // Ô tìm kiếm
        [ObservableProperty] private string searchText = string.Empty;

        public SanPhamViewModel(INavigationService navigationService, AppDbContext db)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _db = db ?? throw new ArgumentNullException(nameof(db));

            // 2️⃣ Sau khi có dữ liệu, tạo view & filter
            _productsView = CollectionViewSource.GetDefaultView(Products);
            _productsView.Filter = FilterProducts;
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            Products.Clear();
            SearchText = string.Empty.Trim();
            var items = await _db.Products
                .AsNoTracking()
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            foreach (var p in items)
            {
                p.Image = LoadBitmap(p.ImagePath);
                Products.Add(p);
            }

            _productsView.Refresh();
        }

        [RelayCommand]
        public void New()
        {
            _navigationService.Navigate(typeof(UiDesktopApp1.Views.Pages.SanPham.ThemSanPhamPage));
        }

        partial void OnSearchTextChanged(string value) => _productsView?.Refresh();

        private bool FilterProducts(object obj)
        {
            if (obj is not ProductModel p) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;

            return (p.ProductName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true)
                || (p.ProductCode?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);
        }

        private static BitmapImage? LoadBitmap(string? path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path)) return null;

                // Hỗ trợ cả Pack URI và file path
                Uri uri = path.StartsWith("pack://", StringComparison.OrdinalIgnoreCase)
                    ? new Uri(path, UriKind.Absolute)
                    : new Uri(System.IO.Path.GetFullPath(path), UriKind.Absolute);

                return new BitmapImage(uri);
            }
            catch { return null; }
        }
    }
}
