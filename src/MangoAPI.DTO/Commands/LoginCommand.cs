﻿using MangoAPI.DTO.Models;
using MediatR;

namespace MangoAPI.DTO.Commands
{
    public class LoginCommand : IRequest<User>
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}