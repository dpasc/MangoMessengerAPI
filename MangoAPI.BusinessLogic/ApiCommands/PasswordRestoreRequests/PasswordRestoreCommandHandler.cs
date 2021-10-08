﻿using MangoAPI.BusinessLogic.BusinessExceptions;
using MangoAPI.BusinessLogic.Responses;
using MangoAPI.DataAccess.Database;
using MangoAPI.DataAccess.Database.Extensions;
using MangoAPI.Domain.Constants;
using MangoAPI.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace MangoAPI.BusinessLogic.ApiCommands.PasswordRestoreRequests
{
    public class PasswordRestoreCommandHandler : IRequestHandler<PasswordRestoreCommand, ResponseBase>
    {
        private readonly MangoPostgresDbContext _postgresDbContext;
        private readonly UserManager<UserEntity> _userManager;

        public PasswordRestoreCommandHandler(MangoPostgresDbContext postgresDbContext,
            UserManager<UserEntity> userManager)
        {
            _postgresDbContext = postgresDbContext;
            _userManager = userManager;
        }

        public async Task<ResponseBase> Handle(PasswordRestoreCommand request, CancellationToken cancellationToken)
        {
            if (request.NewPassword != request.RepeatPassword)
            {
                throw new BusinessException(ResponseMessageCodes.PasswordsAreNotSame);
            }

            var restorePasswordRequest =
                await _postgresDbContext.PasswordRestoreRequests.FindPasswordRestoreRequestByIdAsync(request.RequestId,
                    cancellationToken);

            if (restorePasswordRequest is not { IsValid: true })
            {
                throw new BusinessException(ResponseMessageCodes.InvalidOrExpiredRestorePasswordRequest);
            }

            var user = await _postgresDbContext.Users.FindUserByIdAsync(restorePasswordRequest.UserId,
                cancellationToken);

            if (user is null)
            {
                throw new BusinessException(ResponseMessageCodes.UserNotFound);
            }

            await _userManager.RemovePasswordAsync(user);

            var result = await _userManager.AddPasswordAsync(user, request.NewPassword);

            if (!result.Succeeded)
            {
                throw new BusinessException(ResponseMessageCodes.WeakPassword);
            }

            _postgresDbContext.Users.Update(user);
            _postgresDbContext.PasswordRestoreRequests.Remove(restorePasswordRequest);
            await _postgresDbContext.SaveChangesAsync(cancellationToken);

            return ResponseBase.SuccessResponse;
        }
    }
}