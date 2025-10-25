using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang.Models;
using QuanLyKhoHang.Models.Messages;
using QuanLyKhoHang.Views.Pages.SanPham;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using UiDesktopApp1;
using UiDesktopApp1.Models;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;

namespace QuanLyKhoHang.ViewModels.Pages.SanPham
{
    public partial class QuanLySanPhamViewModel : ObservableObject, INavigationAware, IRecipient<ProductsNeedRefreshMessage>
    {
        private readonly INavigationService _navigationService;
        private readonly AppDbContext _db;
        private readonly ICollectionView _productsView;
        private bool _isInitialized = false;

        // Danh sách nguồn
        public ObservableCollection<ProductModel> Products { get; } = new();

        public ICollectionView ProductsView => _productsView;

        [ObservableProperty]
        private ObservableCollection<CategoryModel> categories = new();

        [ObservableProperty]
        private CategoryModel? selectedCategory;

        // Ô tìm kiếm
        [ObservableProperty] private string searchText = string.Empty;

        [ObservableProperty] private bool isBusy;

        [ObservableProperty] private int selectedCount = 0;

        [ObservableProperty]
        private bool _isAllItemsSelected;

        public QuanLySanPhamViewModel(INavigationService navigationService, AppDbContext db)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _db = db ?? throw new ArgumentNullException(nameof(db));

            // Sau khi có dữ liệu, tạo view & filter
            _productsView = CollectionViewSource.GetDefaultView(Products);
            _productsView.SortDescriptions.Add(new SortDescription(nameof(ProductModel.ProductName), ListSortDirection.Ascending));
            _productsView.Filter = FilterProducts;

            WeakReferenceMessenger.Default.Register<ProductsNeedRefreshMessage>(this);
            //Đăng ký nhận tin nhắn
            //WeakReferenceMessenger.Default.Register<ProductCreatedMessage>(this);

            Products.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (ProductModel item in e.NewItems)
                        item.PropertyChanged += Product_PropertyChanged;

                if (e.OldItems != null)
                    foreach (ProductModel item in e.OldItems)
                        item.PropertyChanged -= Product_PropertyChanged;

                UpdateSelections(); // Cập nhật khi collection thay đổi
            };
        }


        private void Product_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProductModel.IsSelected))
            {
                UpdateSelections();
            }
        }

        private void UpdateSelections()
        {
            UpdateSelectedCount();
            UpdateIsAllItemsSelected();
        }

        private void UpdateSelectedCount()
        {
            SelectedCount = _productsView.Cast<ProductModel>().Count(p => p.IsSelected);
        }

        private void UpdateIsAllItemsSelected()
        {
            // Chỉ kiểm tra các item đang hiển thị trong View (đã filter)
            var viewItems = _productsView.Cast<ProductModel>().ToList();
            bool allSelected = viewItems.Count > 0 && viewItems.All(p => p.IsSelected);
            SetProperty(ref _isAllItemsSelected, allSelected, nameof(IsAllItemsSelected));
        }

        partial void OnIsAllItemsSelectedChanged(bool value)
        {
            // Lấy danh sách item *đang hiển thị* (đã filter) và set IsSelected
            var viewItems = _productsView.Cast<ProductModel>().ToList();
            foreach (var item in viewItems)
            {
                item.IsSelected = value;
            }
            // UpdateSelectedCount sẽ được trigger bởi các event PropertyChanged
        }

        #region Navigation
        public async Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
            {
                await LoadDataAsync();
                _isInitialized = true;
            }
        }
        public Task OnNavigatedFromAsync()
        {
            return Task.CompletedTask;
        }
        #endregion


        public void Receive(ProductsNeedRefreshMessage message)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                await LoadDataAsync();
            });
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            IsBusy = true;
            try
            {
                // THAY ĐỔI: Hủy đăng ký event cũ
                foreach (var p in Products)
                    p.PropertyChanged -= Product_PropertyChanged;

                Products.Clear();
                SearchText = string.Empty.Trim();
                var items = await _db.Products
                    .Include(p => p.Category)
                    .OrderBy(p => p.ProductName)
                    .ToListAsync();

                foreach (var p in items)
                {
                    p.Image = LoadBitmap(p.ImagePath);
                    p.PropertyChanged += Product_PropertyChanged; // THAY ĐỔI: Đăng ký event
                    Products.Add(p);
                }

                //_productsView.Refresh();
                SelectedCategory = Categories.FirstOrDefault();

                if (Categories.Count == 0)
                {
                    Categories.Add(new CategoryModel { Id = 0, Name = "Danh mục" });
                    var list = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
                    foreach (var cat in list)
                        Categories.Add(cat);
                    SelectedCategory = Categories.FirstOrDefault();
                }
            }
            finally
            {
                IsBusy = false;
                UpdateSelections(); // THAY ĐỔI: Cập nhật sau khi tải xong
            }
        }


        partial void OnSearchTextChanged(string value)
        {
            _productsView?.Refresh();
            DeselectHiddenItems();
            UpdateIsAllItemsSelected(); // THÊM MỚI: Cập nhật header checkbox khi filter
            UpdateSelectedCount();
        }

        private bool FilterProducts(object obj)
        {
            if (obj is not ProductModel p) return false;

            if (SelectedCategory != null && SelectedCategory.Id != 0)
                if (p.CategoryId != SelectedCategory.Id) return false;


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

        partial void OnSelectedCategoryChanged(CategoryModel? value)
        {
            _productsView?.Refresh();
            DeselectHiddenItems();
            UpdateIsAllItemsSelected();
            UpdateSelectedCount();
        }

        [RelayCommand]
        private void Exit()
        {
            _navigationService.Navigate(typeof(UiDesktopApp1.Views.Pages.SanPhamPage));
        }

        [RelayCommand]
        private void SelectAll()
        {
            // "Hủy chọn" -> Deselect All
            var itemsToDeselect = _productsView.Cast<ProductModel>().Where(p => p.IsSelected).ToList();
            foreach (var product in itemsToDeselect)
                product.IsSelected = false;
            // UpdateSelections() sẽ tự được gọi
        }
        [RelayCommand]
        private async Task DeleteSelected()
        {
            var selectedProducts = _productsView.Cast<ProductModel>().Where(p => p.IsSelected).ToList();

            var result = System.Windows.MessageBox.Show($"Bạn có chắc chắn muốn xóa {selectedProducts.Count} sản phẩm đã chọn không?",
                                                       "Xác nhận xóa",
                                                       System.Windows.MessageBoxButton.YesNo,
                                                       System.Windows.MessageBoxImage.Warning);
            if(result != System.Windows.MessageBoxResult.Yes) return;
            IsBusy = true;
            try
            {
                _db.Products.RemoveRange(selectedProducts);
                await _db.SaveChangesAsync();

                foreach (var product in selectedProducts)
                {
                    product.PropertyChanged -= Product_PropertyChanged;
                    Products.Remove(product);
                }

                WeakReferenceMessenger.Default.Send(new ProductsNeedRefreshMessage());

                UpdateSelections();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Đã xảy ra lỗi khi xóa sản phẩm:\n{ex.Message}", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                await LoadDataAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }
        private void DeselectHiddenItems()
        {
            // Lấy danh sách các mục đang được chọn (từ danh sách gốc)
            var currentlySelected = Products.Where(p => p.IsSelected).ToList();

            if (currentlySelected.Count == 0)
                return; // Không có gì để làm

            // Lấy danh sách các mục đang hiển thị (sau khi lọc)
            // Dùng HashSet để tăng tốc độ kiểm tra
            var visibleItems = _productsView.Cast<ProductModel>().ToHashSet();

            foreach (var item in currentlySelected)
            {
                // Nếu mục này đang được chọn, NHƯNG không có trong danh sách hiển thị...
                if (!visibleItems.Contains(item))
                {
                    // ... thì bỏ chọn nó.
                    item.IsSelected = false; // Thao tác này sẽ tự động kích hoạt Product_PropertyChanged
                }
            }
        }

        [RelayCommand]
        private void EditProduct(ProductModel? product)
        {
            if (product == null) return;

            _navigationService.Navigate(typeof(SuaSanPhamPage));

            WeakReferenceMessenger.Default.Send(new EditProductMessage(product.Id));
        }
    }
}