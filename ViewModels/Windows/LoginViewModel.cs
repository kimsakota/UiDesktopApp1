using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang.Contracts;
using QuanLyKhoHang.Models;
using QuanLyKhoHang.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Controls;
using UiDesktopApp1.Models;
using Wpf.Ui;

namespace QuanLyKhoHang.ViewModels.Windows
{
    public partial class LoginViewModel : ObservableObject
    {
        public Action? CloseAction { get; set; }
        public bool IsLoginSuccessful { get; private set; } = false;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoggingIn = false;
        public SecureString? SecurePassword { get; set; }
        private readonly IAuthenticationService _authService;

        public LoginViewModel(IAuthenticationService authService)
        {
            _authService = authService;
        }

        private bool CanLogin() => !IsLoggingIn;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync() // Bỏ tham số `object parameter`
        {
            IsLoggingIn = true;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Tên đăng nhập không được để trống.";
                IsLoggingIn = false;
                return;
            }
            if (SecurePassword == null || SecurePassword.Length == 0)
            {
                ErrorMessage = "Mật khẩu không được để trống.";
                IsLoggingIn = false;
                return;
            }

            try
            {
                // Gọi Service để xác thực
                UserModel? user = await _authService.AuthenticateAsync(Username, SecurePassword);

                if (user != null)
                {
                    // Đăng nhập thành công!
                    IsLoginSuccessful = true;
                    CloseAction?.Invoke(); // Đóng cửa sổ Login
                }
                else
                {
                    ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi hệ thống: " + ex.Message;
            }
            finally
            {
                IsLoggingIn = false; // Luôn tắt "đang tải"
            }
        }
    }
}
