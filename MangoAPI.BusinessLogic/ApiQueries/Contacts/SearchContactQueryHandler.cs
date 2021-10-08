﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MangoAPI.Application.Services;
using MangoAPI.BusinessLogic.Models;
using MangoAPI.DataAccess.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MangoAPI.BusinessLogic.ApiQueries.Contacts
{
    public class
        SearchContactByDisplayNameQueryHandler : IRequestHandler<SearchContactQuery,
            SearchContactResponse>
    {
        private readonly MangoPostgresDbContext _postgresDbContext;

        public SearchContactByDisplayNameQueryHandler(MangoPostgresDbContext postgresDbContext)
        {
            _postgresDbContext = postgresDbContext;
        }

        public async Task<SearchContactResponse> Handle(SearchContactQuery request,
            CancellationToken cancellationToken)
        {
            var query = _postgresDbContext.Users
                .AsNoTracking()
                .Include(x => x.UserInformation)
                .Where(x => x.Id != request.UserId)
                .Select(x => new Contact
                {
                    UserId = x.Id,
                    DisplayName = x.DisplayName,
                    Address = x.UserInformation.Address,
                    Bio = x.Bio,
                    PictureUrl = StringService.GetDocumentUrl(x.Image),
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber
                });

            if (!string.IsNullOrEmpty(request.SearchQuery) || !string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                query = query.Where(x => x.DisplayName.Contains(request.SearchQuery) ||
                                         x.PhoneNumber.Contains(request.SearchQuery) ||
                                         x.Email.Contains(request.SearchQuery));
            }

            var searchResult = await query.ToListAsync(cancellationToken);

            var contactsUserIds = searchResult.Select(x => x.UserId);

            var commonContacts = await _postgresDbContext.UserContacts.AsNoTracking()
                .Where(x => x.UserId == request.UserId && contactsUserIds.Contains(x.UserId))
                .Select(x => x.UserId)
                .ToListAsync(cancellationToken);

            foreach (var contact in searchResult)
            {
                contact.IsContact = commonContacts.Contains(contact.UserId);
            }

            return SearchContactResponse.FromSuccess(searchResult);
        }
    }
}