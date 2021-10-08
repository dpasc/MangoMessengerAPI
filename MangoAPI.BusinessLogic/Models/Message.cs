﻿using MangoAPI.Domain.Constants;
using MangoAPI.Domain.Entities;
using System;
using System.ComponentModel;

namespace MangoAPI.BusinessLogic.Models
{
    public record Message
    {
        [DefaultValue("5547b250-4d14-4a4a-88fd-66a779d4f1d2")]
        public Guid MessageId { get; init; }

        [DefaultValue("00aed827-db8c-47de-bc81-78647030918f")]
        public Guid ChatId { get; init; }

        [DefaultValue("11aed827-db8a-47de-bc81-13337703091f")]
        public Guid UserId { get; init; }

        [DefaultValue("Amelit")]
        public string UserDisplayName { get; init; }

        [DefaultValue("Hello World")]
        public string MessageText { get; init; }

        [DefaultValue("12:56")]
        public string CreatedAt { get; init; }

        [DefaultValue("12:57")]
        public string UpdatedAt { get; init; }

        [DefaultValue(false)]
        public bool Self { get; init; }

        [DefaultValue(false)]
        public bool IsEncrypted { get; init; }

        [DefaultValue(569712)]
        public int AuthorPublicKey { get; init; }

        [DefaultValue("https://localhost:5001/Uploads/amelit_picture.jpg")]
        public string MessageAuthorPictureUrl { get; set; }

        [DefaultValue("https://localhost:5001/Uploads/message_attachment.pdf")]
        public string MessageAttachmentUrl { get; set; }
    }

    public static class MessageMapper
    {
        public static Message ToMessage(this MessageEntity message, UserEntity user)
        {
            var messageDto = new Message
            {
                MessageId = message.Id,
                UserId = message.UserId,
                ChatId = message.ChatId,
                UserDisplayName = user.DisplayName,
                MessageText = message.Content,
                CreatedAt = message.CreatedAt.ToShortTimeString(),
                UpdatedAt = message.UpdatedAt?.ToShortTimeString(),
                Self = true,
                IsEncrypted = message.IsEncrypted,
                AuthorPublicKey = message.AuthorPublicKey,
                MessageAuthorPictureUrl = user.Image != null
                    ? $"{EnvironmentConstants.BackendAddress}Uploads/{user.Image}"
                    : null,
                MessageAttachmentUrl = message.Attachment != null
                    ? $"{EnvironmentConstants.BackendAddress}Uploads/{message.Attachment}"
                    : null,
            };

            return messageDto;
        }
    }
}