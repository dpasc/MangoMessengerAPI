﻿using MangoAPI.BusinessLogic;
using System.Threading;
using System.Threading.Tasks;
using MangoAPI.BusinessLogic.ApiCommands.Sessions;
using MangoAPI.BusinessLogic.Responses;
using MangoAPI.IntegrationTests.Helpers;
using Xunit;

namespace MangoAPI.IntegrationTests.ApiCommandsTests.LogoutAllCommandHandlerTests;

public class LogoutAllTestSuccess : IntegrationTestBase
{
    private readonly Assert<ResponseBase> assert = new();

    [Fact]
    public async Task LogoutAllCommandHandlerTestSuccessAsync()
    {
        var user = await MangoModule.RequestAsync(
            request: CommandHelper.RegisterPetroCommand(),
            cancellationToken: CancellationToken.None);
        var userId = user.Response.Tokens.UserId;
        await MangoModule.RequestAsync(
            request: CommandHelper.CreateLoginCommand("kolosovp95@gmail.com", "Bm3-`dPRv-/w#3)cw^97"),
            cancellationToken: CancellationToken.None);
        await MangoModule.RequestAsync(
            request: CommandHelper.CreateLoginCommand("kolosovp95@gmail.com", "Bm3-`dPRv-/w#3)cw^97"),
            cancellationToken: CancellationToken.None);
        var command = new LogoutAllCommand(UserId: userId);

        var result = await MangoModule.RequestAsync(command, CancellationToken.None);

        assert.Pass(result);
    }
}
