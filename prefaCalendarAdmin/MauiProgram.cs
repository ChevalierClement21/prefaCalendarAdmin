using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;
using prefaCalendarAdmin.Config;
using prefaCalendarAdmin.Services;

namespace prefaCalendarAdmin;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

        var dbConfig = new DatabaseConfig
        {
            ConnectionString = "Server=192.168.56.56;Port=3306;Database=prefaCalendar;User=homestead;Password=secret;"
        };
        builder.Services.AddSingleton(dbConfig);

        // Enregistrement des services
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
