﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MangoAPI.DiffieHellmanConsole.Extensions;
using MangoAPI.DiffieHellmanConsole.Services;
using MangoAPI.Domain.Constants;

namespace MangoAPI.DiffieHellmanConsole.CngHandlers;

public class CngConfirmKeyExchangeRequestHandler
{
    private readonly KeyExchangeService _keyExchangeService;
    private readonly EcdhService _ecdhService;
    private readonly TokensService _tokensService;

    public CngConfirmKeyExchangeRequestHandler(
        KeyExchangeService keyExchangeService,
        EcdhService ecdhService,
        TokensService tokensService)
    {
        _keyExchangeService = keyExchangeService;
        _ecdhService = ecdhService;
        _tokensService = tokensService;
    }

    public async Task ConfirmKeyExchangeRequest(IReadOnlyList<string> args)
    {
        var tokensResponse = await _tokensService.GetTokensAsync();

        if (tokensResponse == null)
        {
            const string error = ResponseMessageCodes.TokensNotFound;
            var details = ResponseMessageCodes.ErrorDictionary[error];
            Console.WriteLine($"{error}. {details}");
            return;
        }

        var tokens = tokensResponse.Tokens;

        var requestId = Guid.Parse(args[1]);

        var getKeyExchangeResponse = await _keyExchangeService.GetKeyExchangesAsync();

        var exchangeRequest = getKeyExchangeResponse.KeyExchangeRequests
            .FirstOrDefault(x => x.RequestId == requestId);

        if (exchangeRequest == null)
        {
            const string error = ResponseMessageCodes.KeyExchangeRequestNotFound;
            var details = ResponseMessageCodes.ErrorDictionary[error];
            Console.WriteLine($"{error}. {details}");
            return;
        }

        var ecDiffieHellmanCng = _ecdhService.GenerateEcdhKeysPair(
            out var privateKeyBase64String,
            out var publicKeyBase64String);

        var requestPublicKeyBytes = exchangeRequest.SenderPublicKey.Base64StringAsBytes();

        var requestPublicKey = CngKey.Import(requestPublicKeyBytes, CngKeyBlobFormat.EccPublicBlob);

        var commonSecret = ecDiffieHellmanCng.DeriveKeyMaterial(requestPublicKey).AsBase64String();

        await _keyExchangeService.ConfirmOrDeclineKeyExchange(requestId, publicKeyBase64String);

        var keysFolderPath = Path.Combine(AppContext.BaseDirectory, $"Keys_{tokens.UserId}");

        var privateKeyPath = Path.Combine(
            keysFolderPath,
            $"PrivateKey_{tokens.UserId}_{exchangeRequest.SenderId}.txt");

        var publicKeyPath = Path.Combine(
            keysFolderPath,
            $"PublicKey_{tokens.UserId}_{exchangeRequest.SenderId}.txt");

        var commonSecretPath = Path.Combine(
            keysFolderPath,
            $"CommonSecret_{tokens.UserId}_{exchangeRequest.SenderId}.txt");

        if (!Directory.Exists(keysFolderPath))
        {
            Directory.CreateDirectory(keysFolderPath);
        }

        Console.WriteLine("Writing private key to file...");
        await File.WriteAllTextAsync(privateKeyPath, privateKeyBase64String);

        Console.WriteLine("Writing public key to file ...");
        await File.WriteAllTextAsync(publicKeyPath, publicKeyBase64String);

        Console.WriteLine("Writing common secret to file...");
        await File.WriteAllTextAsync(commonSecretPath, commonSecret);

        Console.WriteLine("Key exchange request confirmed successfully.\n");
    }
}