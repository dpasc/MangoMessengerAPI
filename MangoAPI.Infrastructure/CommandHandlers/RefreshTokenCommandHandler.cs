﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MangoAPI.DTO.Commands.Auth;
using MangoAPI.DTO.Responses.Auth;
using MangoAPI.Infrastructure.Database;
using MangoAPI.Infrastructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MangoAPI.Infrastructure.CommandHandlers
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
    {
        private readonly MangoPostgresDbContext _postgresDbContext;
        private readonly IJwtGenerator _jwtGenerator;

        public RefreshTokenCommandHandler(MangoPostgresDbContext postgresDbContext, IJwtGenerator jwtGenerator)
        {
            _postgresDbContext = postgresDbContext;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var token = await _postgresDbContext.RefreshTokens
                .FirstOrDefaultAsync(x => 
                    x.Token == request.RefreshToken, cancellationToken);

            if (token is not {IsActive: true})
            {
                return await Task.FromResult(new RefreshTokenResponse
                {
                    Message = "Invalid Refresh Token provided.",
                    Success = false,
                    RefreshToken = "N/A",
                    AccessToken = "N/A"
                });
            }

            var user = await _postgresDbContext.Users
                .FirstOrDefaultAsync(x => x.Id == token.UserId, cancellationToken);
            
            if (user == null)
            {
                return await Task.FromResult(new RefreshTokenResponse
                {
                    Message = "Internal error in user fetching. Try again later.",
                    Success = false,
                    RefreshToken = "N/A",
                    AccessToken = "N/A"
                });
            }

            var newRefreshToken = _jwtGenerator.GenerateRefreshToken();
            var newJwtToken = _jwtGenerator.GenerateJwtToken(user);
            
            newRefreshToken.UserId = user.Id;
            
            token.Revoked = DateTime.Now;

            _postgresDbContext.RefreshTokens.Update(token);
            await _postgresDbContext.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
            await _postgresDbContext.SaveChangesAsync(cancellationToken);
            
            return await Task.FromResult(new RefreshTokenResponse
            {
                Message = "Token refreshed successfully.",
                Success = true,
                RefreshToken = newRefreshToken.Token,
                AccessToken = newJwtToken
            });
        }
    }
}