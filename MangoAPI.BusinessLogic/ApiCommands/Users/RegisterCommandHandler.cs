﻿using MangoAPI.Application.Interfaces;
using MangoAPI.BusinessLogic.Responses;
using MangoAPI.DataAccess.Database;
using MangoAPI.Domain.Constants;
using MangoAPI.Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MangoAPI.BusinessLogic.ApiCommands.Users;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<ResponseBase>>
{
    private readonly IEmailSenderService _emailSenderService;
    private readonly MangoPostgresDbContext _postgresDbContext;
    private readonly IUserManagerService _userManager;
    private readonly ResponseFactory<ResponseBase> _responseFactory;
    private readonly IPasswordValidatorService _passwordValidator;

    public RegisterCommandHandler(
        IUserManagerService userManager,
        MangoPostgresDbContext postgresDbContext,
        IEmailSenderService emailSenderService,
        ResponseFactory<ResponseBase> responseFactory, 
        IPasswordValidatorService passwordValidator)
    {
        _userManager = userManager;
        _postgresDbContext = postgresDbContext;
        _emailSenderService = emailSenderService;
        _responseFactory = responseFactory;
        _passwordValidator = passwordValidator;
    }

    public async Task<Result<ResponseBase>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _postgresDbContext.Users
            .AnyAsync(entity => entity.Email == request.Email, cancellationToken);

        if (userExists)
        {
            const string errorMessage = ResponseMessageCodes.UserAlreadyExists;
            var details = ResponseMessageCodes.ErrorDictionary[errorMessage];

            return _responseFactory.ConflictResponse(errorMessage, details);
        }

        var newUser = new UserEntity
        {
            DisplayName = request.DisplayName,
            UserName = Guid.NewGuid().ToString(),
            Email = request.Email,
            EmailCode = Guid.NewGuid(),
            Image = "default_avatar.png"
        };

        if (!_passwordValidator.ValidatePassword(request.Password))
        {
            const string errorMessage = ResponseMessageCodes.WeakPassword;
            var details = ResponseMessageCodes.ErrorDictionary[errorMessage];

            return _responseFactory.ConflictResponse(errorMessage, details);
        }

        await _userManager.CreateAsync(newUser, request.Password);

        var userInfo = new UserInformationEntity
        {
            UserId = newUser.Id,
            CreatedAt = DateTime.UtcNow,
        };

        await _emailSenderService.SendVerificationEmailAsync(newUser, cancellationToken);

        _postgresDbContext.UserInformation.Add(userInfo);

        await _postgresDbContext.SaveChangesAsync(cancellationToken);

        return _responseFactory.SuccessResponse(ResponseBase.SuccessResponse);
    }
}