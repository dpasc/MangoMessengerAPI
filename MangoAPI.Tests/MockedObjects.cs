﻿using System.Threading.Tasks;
using MangoAPI.Application.Interfaces;
using MangoAPI.BusinessLogic.HubConfig;
using MangoAPI.BusinessLogic.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace MangoAPI.Tests
{
    public static class MockedObjects
    {
        public static IHubContext<ChatHub, IHubClient> GetHubContextMock()
        {
            var hubMock = new Mock<IHubContext<ChatHub, IHubClient>>();

            hubMock
                .Setup(x => x.Clients.Group(It.IsAny<string>())
                    .BroadcastMessageAsync(It.IsAny<Message>()))
                .Returns(Task.CompletedTask);

            hubMock.Setup(x => x.Clients.Group(It.IsAny<string>())
                .UpdateUserChatsAsync(It.IsAny<Chat>())).Returns(Task.CompletedTask);

            hubMock
                .Setup(x => x.Clients.Group(It.IsAny<string>())
                    .NotifyOnMessageDeleteAsync(It.IsAny<MessageDeleteNotification>()))
                .Returns(Task.CompletedTask);

            hubMock
                .Setup(x => x.Clients.Group(It.IsAny<string>())
                    .NotifyOnMessageEditAsync(It.IsAny<MessageEditNotification>()))
                .Returns(Task.CompletedTask);

            return hubMock.Object;
        }

        public static IBlobService GetBlobServiceMock()
        {
            var blobServiceMock = new Mock<IBlobService>();

            const string blobName = "MOCKED_BLOB";

            blobServiceMock.Setup(x =>
                    x.UploadFileBlobAsync(
                        It.IsAny<string>(),
                        It.IsAny<IFormFile>(),
                        It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            blobServiceMock.Setup(x =>
                    x.GetBlobAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                .Returns(Task.FromResult(blobName));

            return blobServiceMock.Object;
        }
    }
}