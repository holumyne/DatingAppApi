using DatingAppApi.Data;
using DatingAppApi.Helpers;
using DatingAppApi.Interfaces;
using DatingAppApi.Services;
using DatingAppApi.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DatingAppApi.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationSevices(this IServiceCollection services, IConfiguration config)
        {
            //moved back to our program.cs class. Its easier to utilised there.
            //services.AddDbContext<DataContext>(opt =>
            //{
            //    opt.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            //    //    opt.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            //});
            services.AddCors();
            services.AddScoped<ITokenService, TokenService>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<LogUserActivity>();
            //services.AddScoped<ILikesRepository, LikesRepository>();
            //services.AddScoped<IMessageRepository, MessageRepository>();
            //services.AddScoped<IUserRepository, UserRepository>();

            services.AddSignalR();
            services.AddSingleton<PresenceTracker>();  //we use Singleton becos we want thetracker to leave as long as our app does
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
