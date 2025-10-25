using QuanLyKhoHang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyKhoHang.Services
{
    public interface IAuthenticationService
    {
        Task<UserModel?> AuthenticateAsync(string username, SecureString password);
    }
}
