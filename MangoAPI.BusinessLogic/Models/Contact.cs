﻿namespace MangoAPI.BusinessLogic.Models
{
    public record Contact
    {
        public string UserId { get; init; }

        public string DisplayName { get; init; }

        public string Address { get; init; }

        public string Bio { get; init; }

        public bool IsContact { get; init; }
    }
}
