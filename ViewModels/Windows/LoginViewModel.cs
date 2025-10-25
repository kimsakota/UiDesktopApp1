using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Controls;
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

        private readonly INavigationService _navigationService;

        public LoginViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        private void Login(object parameter)
        {
            if (parameter is not PasswordBox passwordBox)
                return;
            
            var password = passwordBox.Password;

            if(Username == "admin" && password == "123")
            {
                IsLoginSuccessful = true;
                CloseAction?.Invoke();
            } else
            {
                ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
            }
        }
    }
}
