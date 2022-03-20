﻿using System;
using System.Threading.Tasks;
using MangoAPI.DiffieHellmanConsole.CngHandlers;
using MangoAPI.DiffieHellmanConsole.Extensions;
using MangoAPI.DiffieHellmanConsole.Handlers;
using MangoAPI.DiffieHellmanConsole.OpenSslHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace MangoAPI.DiffieHellmanConsole;

public static class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Unrecognized command.");
            return;
        }

        var serviceProvider = new ServiceCollection()
            .AddAuthHandlers()
            .AddServices()
            .AddCngHandlers()
            .AddOpenSslHandlers()
            .BuildServiceProvider();

        var method = args[0];

        switch (method)
        {
            case "login":
            {
                var handler = serviceProvider.GetService<LoginHandler>() ??
                              throw new ArgumentException(
                                  $"Handler is null. Register it in dependency injection. {nameof(LoginHandler)}");

                await handler.LoginAsync(args);
                break;
            }
            case "refresh-token":
            {
                var refreshHandler = serviceProvider.GetService<RefreshTokenHandler>() ??
                                     throw new ArgumentException(
                                         $"Handler is null. Register it in dependency injection. {nameof(RefreshTokenHandler)}");

                await refreshHandler.RefreshTokensAsync();
                break;
            }
            case "cng-key-exchange":
            {
                var handler = serviceProvider.GetService<CngRequestKeyExchangeHandler>() ??
                              throw new ArgumentException(
                                  $"Handler is null. Register it in dependency injection. {nameof(CngRequestKeyExchangeHandler)}");

                await handler.RequestKeyExchange(args);
                break;
            }
            case "cng-key-exchange-requests":
            {
                var handler = serviceProvider.GetService<CngPrintKeyExchangeListHandler>() ??
                              throw new ArgumentException(
                                  $"Handler is null. Register it in dependency injection. {nameof(CngPrintKeyExchangeListHandler)}");

                await handler.PrintKeyExchangesListAsync();
                break;
            }
            case "cng-confirm-key-exchange":
            {
                var handler = serviceProvider.GetService<CngConfirmKeyExchangeRequestHandler>() ??
                              throw new ArgumentException(
                                  $"Handler is null. Register it in dependency injection. {nameof(CngConfirmKeyExchangeRequestHandler)}");

                await handler.ConfirmKeyExchangeRequest(args);
                break;
            }
            case "cng-print-public-keys":
            {
                var handler = serviceProvider.GetService<CngPrintPublicKeysHandler>() ??
                              throw new ArgumentException(
                                  $"Handler is null. Register it in dependency injection. {nameof(CngPrintPublicKeysHandler)}");

                await handler.PrintPublicKeysAsync();
                break;
            }
            case "cng-create-common-secret":
            {
                var handler = serviceProvider.GetService<CngCreateCommonSecretHandler>() ??
                              throw new ArgumentException(
                                  $"Handler is null. Register it in dependency injection. {nameof(CngCreateCommonSecretHandler)}");

                await handler.CreateCommonSecret(args);
                break;
            }
            case "openssl-generate-dh-parameters":
            {
                var handler = serviceProvider.GetService<OpenSslCreateDhParametersHandler>() ??
                              throw new ArgumentException(
                                  $"Handler is null. Register it in dependency injection. {nameof(OpenSslCreateDhParametersHandler)}");

                await handler.CreateDhParametersAsync();
                break;
            }
            case "openssl-upload-dh-parameters":
            {
                var handler = serviceProvider.GetService<OpenSslUploadDhParametersHandler>() ??
                              throw new ArgumentException(
                                  $"Handler is null. Register it in dependency injection. {nameof(OpenSslUploadDhParametersHandler)}");

                await handler.UploadDhParametersAsync();
                break;
            }
            default:
            {
                Console.WriteLine("Unrecognized command.");
                break;
            }
        }
    }
}