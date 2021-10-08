﻿using MangoAPI.BusinessLogic.Models;
using MangoAPI.DataAccess.Database;
using MangoAPI.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MangoAPI.BusinessLogic.ApiQueries.Messages
{
    public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, GetMessagesResponse>
    {
        private readonly MangoPostgresDbContext _postgresDbContext;

        public GetMessagesQueryHandler(MangoPostgresDbContext postgresDbContext)
        {
            _postgresDbContext = postgresDbContext;
        }

        public async Task<GetMessagesResponse> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
        {
            var chat = await _postgresDbContext
                .Messages
                .AsNoTracking()
                .Include(x => x.Chat)
                .Where(x => x.ChatId == request.ChatId)
                .OrderBy(x => x.CreatedAt)
                .Select(messageEntity => new Message
                {
                    MessageId = messageEntity.Id,
                    ChatId = messageEntity.ChatId,
                    UserId = messageEntity.UserId,
                    MessageText = messageEntity.Content,
                    UpdatedAt = messageEntity.UpdatedAt.HasValue ? messageEntity.UpdatedAt.Value.ToShortTimeString() : null,
                    CreatedAt = messageEntity.CreatedAt.ToShortTimeString(),
                    UserDisplayName = messageEntity.User.DisplayName,
                    Self = messageEntity.User.Id == request.UserId,
                    IsEncrypted = messageEntity.IsEncrypted,
                    AuthorPublicKey = messageEntity.AuthorPublicKey,
                    MessageAuthorPictureUrl = messageEntity.User.Image != null ? $"{EnvironmentConstants.BackendAddress}Uploads/{messageEntity.User.Image}" : null,
                    MessageAttachmentUrl = messageEntity.Attachment != null ? $"{EnvironmentConstants.BackendAddress}Uploads/{messageEntity.Attachment}" : null,
                }).ToListAsync(cancellationToken);

            return GetMessagesResponse.FromSuccess(chat);
        }
    }
}