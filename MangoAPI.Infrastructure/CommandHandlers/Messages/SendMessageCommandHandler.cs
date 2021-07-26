﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MangoAPI.Domain.Entities;
using MangoAPI.Domain.Enums;
using MangoAPI.DTO.ApiCommands.Messages;
using MangoAPI.DTO.Responses.Messages;
using MangoAPI.Infrastructure.Database;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MangoAPI.Infrastructure.CommandHandlers.Messages
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, SendMessageResponse>
    {
        private readonly MangoPostgresDbContext _mangoPostgresDbContext;
        private readonly UserManager<UserEntity> _userManager;

        public SendMessageCommandHandler(MangoPostgresDbContext mangoPostgresDbContext,
            UserManager<UserEntity> userManager)
        {
            _mangoPostgresDbContext = mangoPostgresDbContext;
            _userManager = userManager;
        }

        public async Task<SendMessageResponse> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user == null)
            {
                return SendMessageResponse.UserNotFound;
            }

            var chat = await _mangoPostgresDbContext.Chats.FirstOrDefaultAsync(x => x.Id == request.ChatId,
                cancellationToken);

            if (chat == null)
            {
                return SendMessageResponse.ChatNotFound;
            }

            var permitted = await CheckUserPermissions(user, chat, cancellationToken);

            if (!permitted)
            {
                return SendMessageResponse.PermissionDenied;
            }

            var messageEntity = new MessageEntity
            {
                Id = Guid.NewGuid().ToString(),
                ChatId = request.ChatId,
                UserId = request.UserId,
                Content = request.MessageText,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            await _mangoPostgresDbContext.Messages.AddAsync(messageEntity, cancellationToken);
            await _mangoPostgresDbContext.SaveChangesAsync(cancellationToken);

            return SendMessageResponse.FromSuccess(messageEntity.Id);
        }

        private async Task<bool> CheckUserPermissions(UserEntity user, ChatEntity chat,
            CancellationToken cancellationToken)
        {
            return chat.ChatType switch
            {
                ChatType.DirectChat => true,
                ChatType.PrivateChannel => await CheckPrivateChannelPermissions(user, chat, cancellationToken),
                ChatType.PublicChannel => await CheckPublicChannelPermissions(user, chat, cancellationToken),
                ChatType.ReadOnlyChannel => await CheckReadOnlyChannelPermissions(user, chat, cancellationToken),
                _ => false
            };
        }

        private async Task<bool> CheckReadOnlyChannelPermissions(UserEntity user, ChatEntity chat,
            CancellationToken cancellationToken)
        {
            return (await _mangoPostgresDbContext.UserChats
                    .Where(x => x.UserId == user.Id && x.ChatId == chat.Id)
                    .ToListAsync(cancellationToken))
                .Any(x => x.RoleId is UserRole.Moderator or UserRole.Admin or UserRole.Owner);
        }

        private async Task<bool> CheckPublicChannelPermissions(UserEntity user, ChatEntity chat,
            CancellationToken cancellationToken)
        {
            return await _mangoPostgresDbContext.UserChats
                .Where(x => x.UserId == user.Id && x.ChatId == chat.Id)
                .AnyAsync(cancellationToken);
        }

        private async Task<bool> CheckPrivateChannelPermissions(UserEntity user, ChatEntity chat,
            CancellationToken cancellationToken)
        {
            return await _mangoPostgresDbContext.UserChats
                .Where(x => x.UserId == user.Id && x.ChatId == chat.Id)
                .AnyAsync(cancellationToken);
        }
    }
}