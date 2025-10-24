﻿using CommunityToolkit.Mvvm.ComponentModel;
using QuanLyKhoHang.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace UiDesktopApp1.Models
{
    public partial class ProductModel : ObservableValidator
    {
        [Key]
        public int Id { get; set; }

        [ObservableProperty]
        private string? imagePath = "pack://application:,,,/Assets/Images/logo-image.png";

        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc")]
        [ObservableProperty]
        private string? productCode;

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [ObservableProperty]
        private string? productName;

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng ban đầu không hợp lệ.")]
        [ObservableProperty]
        private int initialQty;

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Giá vốn không hợp lệ.")]
        public decimal CostPrice { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Giá bán không hợp lệ.")]
        public decimal SalePrice { get; set; }

        [ObservableProperty]
        private DateTime? expiryDate = null;

        [ObservableProperty]
        private string? description;

        [ObservableProperty]
        private int? categoryId;

        [ForeignKey(nameof(CategoryId))]
        public CategoryModel? Category { get; set; }

        [NotMapped]
        public BitmapImage? Image { get; set; }

        [NotMapped]
        
        private bool isSelected = false;
        public void ValidateAll() => base.ValidateAllProperties();

    }
}