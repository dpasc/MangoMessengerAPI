﻿namespace MangoAPI.BusinessLogic.ApiQueries.Messages
{
    using Domain.Constants;
    using Domain.Entities;
    using Models;
    using Responses;
    using System.Collections.Generic;
    using System.Linq;

    public record GetMessagesResponse : MessageResponseBase<GetMessagesResponse>
    {
        public List<Message> Messages { get; init; }

        public static GetMessagesResponse FromSuccess(IEnumerable<MessageEntity> messages, UserEntity user)
        {
            return new()
            {
                Message = ResponseMessageCodes.Success,

                Messages = messages.OrderBy(messageEntity => messageEntity.CreatedAt)
                    .Select(messageEntity => new Message
                    {
                        MessageId = messageEntity.Id,
                        MessageText = messageEntity.Content,
                        EditedAt = messageEntity.UpdatedAt?.ToShortTimeString(),
                        SentAt = messageEntity.CreatedAt.ToShortTimeString(),
                        UserDisplayName = messageEntity.User.DisplayName,
                        Self = messageEntity.User.Id == user.Id,
                    }).ToList(),

                Success = true,
            };
        }
    }
}