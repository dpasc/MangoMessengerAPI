﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MangoAPI.Application.Interfaces;
using MangoAPI.Domain.Constants;
using MangoAPI.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace MangoAPI.Application.Services
{
    public class JwtGenerator : IJwtGenerator
    {
        private readonly SymmetricSecurityKey _key;

        public JwtGenerator()
        {
            var tokenKey = EnvironmentConstants.MangoTokenKey;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey!));
        }

        public string GenerateJwtToken(UserEntity userEntity, string role)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, userEntity.Id),
                new(ClaimsIdentity.DefaultRoleClaimType, role),
            };

            var jwtLifetime = EnvironmentConstants.JwtLifeTime;

            if (jwtLifetime == null || !int.TryParse(jwtLifetime, out var jwtLifetimeParsed))
            {
                throw new InvalidOperationException("Jwt lifetime environmental variable error.");
            }

            var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(jwtLifetimeParsed),
                SigningCredentials = credentials,
                Issuer = EnvironmentConstants.MangoIssuer,
                Audience = EnvironmentConstants.MangoAudience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}