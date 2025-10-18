using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using UiDesktopApp1.Models;
using UiDesktopApp1.Services;
using UiDesktopApp1.ViewModels.Pages;
using UiDesktopApp1.ViewModels.Pages.BaoCao;
using UiDesktopApp1.ViewModels.Pages.LienHe;
using UiDesktopApp1.ViewModels.Pages.SanPham;
using UiDesktopApp1.ViewModels.UserControls;
using UiDesktopApp1.ViewModels.UserControls.SanPham;
using UiDesktopApp1.ViewModels.Windows;
using UiDesktopApp1.Views.Pages;
using UiDesktopApp1.Views.Pages.BaoCao;
using UiDesktopApp1.Views.Pages.LienHe;
using UiDesktopApp1.Views.Pages.SanPham;
using UiDesktopApp1.Views.UserControls;
using UiDesktopApp1.Views.UserControls.SanPham;
using UiDesktopApp1.Views.Windows;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.DependencyInjection;

namespace UiDesktopApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => {
                // Nạp cấu hình & connection string
                c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory)!);
                c.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                // c.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true); // nếu cần
                // c.AddEnvironmentVariables(); // nếu cần
            })
            .ConfigureServices((context, services) =>
            {
                services.AddNavigationViewPageProvider();

                services.AddHostedService<ApplicationHostService>();

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // Main window with navigation
                services.AddSingleton<INavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();

                // EF Core registration
                // =========================
                var connStr = context.Configuration.GetConnectionString("DefaultConnection")
                              ?? "Server=localhost\\SQLEXPRESS;Database=QuanLyKhoHang;Trusted_Connection=True;TrustServerCertificate=True;";

                // CÁCH A (đơn giản): Inject thẳng AppDbContext vào ViewModel
                services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connStr));
                // CÁCH B (khuyên dùng Desktop): Factory (nếu ViewModel dùng IDbContextFactory<AppDbContext>)
                // services.AddDbContextFactory<AppDbContext>(opt => opt.UseSqlServer(connStr));
                // —> nếu dùng cách B, nhớ sửa constructor VM sang IDbContextFactory<AppDbContext>

                services.AddSingleton<TaiChinhPage>();
                services.AddTransient<TaiChinhViewModel>();
                services.AddSingleton<TonKhoPage>();
                services.AddTransient<TonKhoViewModel>();
                services.AddSingleton<Views.Pages.BaoCao.KhachHangPage>();
                services.AddTransient<ViewModels.Pages.BaoCao.KhachHangViewModel>();
                services.AddSingleton<SanPhamPage>();
                services.AddTransient<SanPhamViewModel>();
                services.AddSingleton<NhapKhoPage>();
                services.AddTransient<NhapKhoViewModel>();
                services.AddSingleton<XuatKhoPage>();
                services.AddTransient<XuatKhoViewModel>();
                services.AddSingleton<KiemKeKhoPage>();
                services.AddTransient<KiemKeKhoViewModel>();
                services.AddSingleton<ChuyenKhoPage>();
                services.AddTransient<ChuyenKhoViewModel>();
                services.AddSingleton<LichSuPage>();
                services.AddTransient<LichSuViewModel>();
                services.AddSingleton<ChiPhiPage>();
                services.AddTransient<ChiPhiViewModel>();

                services.AddSingleton<Views.Pages.LienHe.KhachHangPage>();
                services.AddTransient<ViewModels.Pages.LienHe.KhachHangViewModel>();
                services.AddSingleton<NhaCungCapPage>();
                services.AddTransient<NhaCungCapViewModel>();
                services.AddSingleton<NhanVienPage>();
                services.AddTransient<NhanVienViewModel>();

                services.AddSingleton<SettingsPage>();
                services.AddTransient<SettingsViewModel>();

                //Thành phần trong mục sản phẩm
                services.AddTransient<ThemSanPhamPage>();
                services.AddTransient<ThemSanPhamViewModel>();

                // Thêm dòng sau: Đăng ký Header và ViewModel của nó
                // Dùng AddTransient để mỗi lần gọi sẽ tạo một instance mới, phù hợp cho UserControl
                services.AddTransient<SanPhamPageHeader>();
                services.AddTransient<SanPhamPageHeaderViewModel>();
                services.AddTransient<NhapKhoPageHeader>();
                services.AddTransient<NhapKhoPageHeaderViewModel>();
                
                
                services.AddTransient<ThemSanPhamPageHeader>();
                services.AddTransient<ThemSanPhamPageHeaderViewModel>();
               
                //
            }).Build();

        /// <summary>
        /// Gets services.
        /// </summary>
        public static IServiceProvider Services
        {
            get { return _host.Services; }
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            await _host.StartAsync();

            ApplicationThemeManager.Apply(ApplicationTheme.Light);
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }
}
