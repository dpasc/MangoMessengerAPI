﻿using MangoAPI.Application.Interfaces;
using MangoAPI.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MangoAPI.BusinessLogic.DependencyInjection;

public static class MessengerServices
{
    public static IServiceCollection AddMessengerServices(
        this IServiceCollection services,
        string mangoBlobUrl,
        string mangoBlobContainerName,
        string mangoBlobAccess,
        string mangoJwtSignKey,
        string mangoJwtIssuer,
        string mangoJwtAudience,
        int mangoJwtLifetimeMinutes,
        int mangoRefreshTokenLifetimeDays,
        string mangoUserPassword)
    {
        services.AddAzureBlobServices(
            mangoBlobUrl,
            mangoBlobContainerName,
            mangoBlobAccess);

        services.AddJwtGeneratorServices(
            mangoJwtIssuer,
            mangoJwtAudience,
            mangoJwtSignKey,
            mangoJwtLifetimeMinutes,
            mangoRefreshTokenLifetimeDays);

        services.AddSingInManagerServices();
        services.AddPasswordHashServices();

        services.AddSingleton<IVersionService, VersionService>();
        
        services.AddSingleton<IMangoUserSettings, MangoUserSettings>(_ => new MangoUserSettings(mangoUserPassword));

        services.AddScoped<IAvatarService, AvatarService>();
        
        return services;
    }
}
