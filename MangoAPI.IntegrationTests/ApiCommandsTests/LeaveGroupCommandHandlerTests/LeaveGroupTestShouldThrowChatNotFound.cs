﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MangoAPI.BusinessLogic.ApiCommands.UserChats;
using MangoAPI.Domain.Constants;
using MangoAPI.IntegrationTests.Helpers;
using Xunit;

namespace MangoAPI.IntegrationTests.ApiCommandsTests.LeaveGroupCommandHandlerTests;

public class LeaveGroupTestShouldThrowChatNotFound : IntegrationTestBase
{
    private readonly Assert<LeaveGroupResponse> _assert = new();

    [Fact]
    public async Task LeaveGroupTestShouldThrow_UserNotFound()
    {
        const string expectedMessage = ResponseMessageCodes.ChatNotFound;
        var expectedDetails = ResponseMessageCodes.ErrorDictionary[expectedMessage];
        var user =
            await MangoModule.RequestAsync(CommandHelper.RegisterPetroCommand(), CancellationToken.None);
        var command = new LeaveGroupCommand
        {
            UserId = user.Response.UserId,
            ChatId = Guid.NewGuid()
        };
        
        var result = await MangoModule.RequestAsync(command, CancellationToken.None);
            
        _assert.Fail(result, expectedMessage, expectedDetails);
    }
}