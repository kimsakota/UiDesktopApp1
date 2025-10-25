using Microsoft.EntityFrameworkCore;
using QuanLyKhoHang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using UiDesktopApp1.Models;

namespace QuanLyKhoHang.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public AuthenticationService(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<UserModel?> AuthenticateAsync(string username, SecureString securePassword)
        {
            // Tạo một DbContext mới chỉ cho lần gọi này
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            // 1. Tìm user
            var user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username.Equals(username));

            if (user == null)
                return null; // Không tìm thấy user

            // 2. Chuyển SecureString (an toàn) -> string (không an toàn)
            var password = ConvertSecureStringToString(securePassword);

            // 3. Xác thực bằng BCrypt
            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            // 4. Dọn dẹp password thô khỏi bộ nhớ
            password = null;

            if (isValid)
            {
                return user; // Đăng nhập thành công!
            }

            return null; // Sai mật khẩu
        }

        // Hàm trợ giúp để chuyển SecureString -> string
        private string ConvertSecureStringToString(SecureString securePassword)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}
