using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ASI.Basecode.WebApp
{
    // Other services configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configures the other services.
        /// </summary>
        private void ConfigureOtherServices()
        {
            // Framework
            this._services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            this._services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Common
            this._services.AddScoped<TokenProvider>();
            this._services.TryAddSingleton<TokenProviderOptionsFactory>();
            this._services.TryAddSingleton<TokenValidationParametersFactory>();
            this._services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            this._services.TryAddSingleton<TokenValidationParametersFactory>();
            this._services.AddScoped<IUserService, UserService>();
            this._services.AddScoped<IBookService, BookService>();
            this._services.AddScoped<IBorrowingService, BorrowingService>();
            this._services.AddScoped<IReviewService, ReviewService>();
            this._services.AddScoped<INotificationService, NotificationService>();
            this._services.AddScoped<IUserSettingService, UserSettingService>();
            this._services.AddScoped<IAnalyticsService, AnalyticsService>(); // ADVANCED FEATURE #1
            this._services.AddScoped<IReportingService, ReportingService>(); // ADVANCED FEATURE #2
            this._services.AddScoped<IRecommendationService, RecommendationService>(); // ADVANCED FEATURE #4
            this._services.AddScoped<IEmailService, EmailService>(); // Email Service for password reset

            // Repositories
            this._services.AddScoped<IUserRepository, UserRepository>();
            this._services.AddScoped<IBookRepository, BookRepository>();
            this._services.AddScoped<IBorrowingRepository, BorrowingRepository>();
            this._services.AddScoped<IReviewRepository, ReviewRepository>();
            this._services.AddScoped<INotificationRepository, NotificationRepository>();
            this._services.AddScoped<IUserSettingRepository, UserSettingRepository>();

            // Manager Class
            this._services.AddScoped<SignInManager>();

            this._services.AddHttpClient();
        }
    }
}
