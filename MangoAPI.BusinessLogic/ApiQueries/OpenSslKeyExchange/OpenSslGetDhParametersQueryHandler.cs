﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MangoAPI.BusinessLogic.Responses;
using MangoAPI.DataAccess.Database;
using MangoAPI.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MangoAPI.BusinessLogic.ApiQueries.OpenSslKeyExchange;

public class OpenSslGetDhParametersQueryHandler : IRequestHandler<OpenSslGetDhParametersQuery, Result<OpenSslGetDhParametersResponse>>
{
    private readonly MangoPostgresDbContext _mangoPostgresDbContext;
    private readonly ResponseFactory<OpenSslGetDhParametersResponse> _responseFactory;

    public OpenSslGetDhParametersQueryHandler(MangoPostgresDbContext mangoPostgresDbContext,
        ResponseFactory<OpenSslGetDhParametersResponse> responseFactory)
    {
        _mangoPostgresDbContext = mangoPostgresDbContext;
        _responseFactory = responseFactory;
    }

    public async Task<Result<OpenSslGetDhParametersResponse>> Handle(OpenSslGetDhParametersQuery request,
        CancellationToken cancellationToken)
    {
        var dhParameter = await _mangoPostgresDbContext.OpenSslDhParameters
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (dhParameter == null)
        {
            const string errorMessage = ResponseMessageCodes.DhParameterNotFound;
            var errorDetails = ResponseMessageCodes.ErrorDictionary[errorMessage];

            return _responseFactory.ConflictResponse(errorMessage, errorDetails);
        }

        var bytes = dhParameter.OpenSslDhParameter;

        var response = new OpenSslGetDhParametersResponse
        {
            FileContent = bytes,
            Message = ResponseMessageCodes.Success,
            Success = true
        };

        return _responseFactory.SuccessResponse(response);
    }
}