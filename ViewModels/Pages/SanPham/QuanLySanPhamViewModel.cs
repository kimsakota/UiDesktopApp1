using CommunityToolkit.Mvvm.Messaging;
using QuanLyKhoHang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiDesktopApp1.Models;
using Wpf.Ui;

namespace QuanLyKhoHang.ViewModels.Pages.SanPham
{
    public partial class QuanLySanPhamViewModel : ObservableObject
    {
        private readonly INavigationService _nav;
        private readonly AppDbContext _db;

        public QuanLySanPhamViewModel(INavigationService nav, AppDbContext db)
        {
            _nav = nav;
            _db = db;
        }

        [RelayCommand]
        private void Exit()
        {
            _nav.Navigate(typeof(UiDesktopApp1.Views.Pages.SanPhamPage));
        }
    }
}
