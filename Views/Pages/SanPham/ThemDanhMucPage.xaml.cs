﻿using QuanLyKhoHang.ViewModels.Pages.SanPham;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuanLyKhoHang.Views.Pages.SanPham
{
    /// <summary>
    /// Interaction logic for ThemDanhMucPage.xaml
    /// </summary>
    public partial class ThemDanhMucPage : Page
    {
        public ThemDanhMucViewModel ViewModel { get; set; }
        public ThemDanhMucPage(ThemDanhMucViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = viewModel;
        }
    }
}
