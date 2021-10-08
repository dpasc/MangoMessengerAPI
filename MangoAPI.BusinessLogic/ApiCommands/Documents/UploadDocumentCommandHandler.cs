﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MangoAPI.DataAccess.Database;
using MangoAPI.Domain.Constants;
using MangoAPI.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Hosting;

namespace MangoAPI.BusinessLogic.ApiCommands.Documents
{
    public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, UploadDocumentResponse>
    {
        private readonly MangoPostgresDbContext _postgresDbContext;
        private readonly IHostingEnvironment _environment;

        public UploadDocumentCommandHandler(MangoPostgresDbContext postgresDbContext, IHostingEnvironment environment)
        {
            _postgresDbContext = postgresDbContext;
            _environment = environment;
        }

        public async Task<UploadDocumentResponse> Handle(UploadDocumentCommand request,
            CancellationToken cancellationToken)
        {
            var uniqueFileName = GetUniqueFileName(request.FormFile.FileName);
            var uploads = Path.Combine(_environment.WebRootPath, "Uploads");
            var filePath = Path.Combine(uploads, uniqueFileName);

            await request.FormFile.CopyToAsync(new FileStream(filePath, FileMode.Create), cancellationToken);

            var documentEntity = new DocumentEntity
            {
                FileName = uniqueFileName,
                FilePath = filePath
            };

            await _postgresDbContext.Documents.AddAsync(documentEntity, cancellationToken);
            await _postgresDbContext.SaveChangesAsync(cancellationToken);

            var fileUrl = $"{EnvironmentConstants.BackendAddress}Uploads/{documentEntity.FileName}";
            return UploadDocumentResponse.FromSuccess(documentEntity.FileName, fileUrl);
        }

        private static string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                   + "_"
                   + Guid.NewGuid()
                   + Path.GetExtension(fileName);
        }
    }
}