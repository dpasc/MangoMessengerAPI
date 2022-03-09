﻿using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MangoAPI.BusinessLogic.ApiQueries.KeyExchange;
using MangoAPI.BusinessLogic.Responses;
using MangoAPI.Domain.Constants;
using MangoAPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MangoAPI.Tests.ApiQueries.GetKeyExchangeRequestsQueryHandlerTests;

public class GetKeyExchangeRequestsTestSuccess : ITestable<GetKeyExchangeRequestsQuery, GetKeyExchangeResponse>
{
    private readonly MangoDbFixture _mangoDbFixture = new();
    private readonly Assert<GetKeyExchangeResponse> _assert = new ();

    [Fact]
    public async Task GetKeyExchangeRequestsTest_Success()
    {
        Seed();
        var query = new GetKeyExchangeRequestsQuery
        {
            UserId = SeedDataConstants.AmelitId
        };
        var handler = CreateHandler();

        var result = await handler.Handle(query, CancellationToken.None);
            
        _assert.Pass(result);
        result.Response.KeyExchangeRequests.Count.Should().Be(1);
        result.Response.KeyExchangeRequests[0].RequestId.Should().Be(_keyExchangeRequest.Id);
    }
        
    public bool Seed()
    {
        _mangoDbFixture.Context.AddRange(_sender, _receiver);
        _mangoDbFixture.Context.KeyExchangeRequests.Add(_keyExchangeRequest);

        _mangoDbFixture.Context.SaveChanges();
            
        _mangoDbFixture.Context.Entry(_sender).State = EntityState.Detached;
        _mangoDbFixture.Context.Entry(_receiver).State = EntityState.Detached;
        _mangoDbFixture.Context.Entry(_keyExchangeRequest).State = EntityState.Detached;
            
        return true;
    }

    public IRequestHandler<GetKeyExchangeRequestsQuery, Result<GetKeyExchangeResponse>> CreateHandler()
    {
        var responseFactory = new ResponseFactory<GetKeyExchangeResponse>();
        var handler = new GetKeyExchangeRequestsQueryHandler(_mangoDbFixture.Context, responseFactory);
        return handler;
    }
        
    private readonly UserEntity _sender = new()
    {
        DisplayName = "razumovsky r",
        Bio = "11011 y.o Dotnet Developer from $\"{cityName}\"",
        Id = SeedDataConstants.RazumovskyId,
        UserName = "razumovsky_r",
        Email = "kolosovp95@gmail.com",
        NormalizedEmail = "KOLOSOVP94@GMAIL.COM",
        EmailConfirmed = true,
        PhoneNumberConfirmed = true,
        Image = "razumovsky_picture.jpg"
    };

    private readonly UserEntity _receiver = new()
    {
        DisplayName = "Amelit",
        Bio = "Дипломат",
        Id = SeedDataConstants.AmelitId,
        UserName = "TheMoonlightSonata",
        Email = "amelit@gmail.com",
        NormalizedEmail = "AMELIT@GMAIL.COM",
        EmailConfirmed = true,
        PhoneNumberConfirmed = true,
        Image = "amelit_picture.jpg"
    };
        
    private readonly KeyExchangeRequestEntity _keyExchangeRequest = new()
    {
        Id = Guid.NewGuid(),
        SenderId = SeedDataConstants.RazumovskyId,
        SenderPublicKey = "Public Key",
        UserId = SeedDataConstants.AmelitId
    };
}