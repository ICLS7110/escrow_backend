using System;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.ContactUsPanel;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.ContactUsPanel.Commands;

public record SendContactMessageCommand : IRequest<Result<int>>
{
    public string Name { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public class SendContactMessageHandler : IRequestHandler<SendContactMessageCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public SendContactMessageHandler(IApplicationDbContext context, IEmailService emailService) // 
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<Result<int>> Handle(SendContactMessageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 🔹 Temporarily commenting out validation for debugging
            /*
            var validator = new SendContactMessageValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                string errorMessages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return Result<int>.Failure(StatusCodes.Status400BadRequest, errorMessages);
            }
            */


           
            await _emailService.SendEmailAsync(request.Email, request.Title, request.Name, request.Message);

            // 🔹 Step 1: Store Message in Database
            var contactMessage = new ContactUs
            {
                Name = request.Name,
                Number = request.Number,
                Email = request.Email,
                Title = request.Title,
                Message = request.Message,
                Created = DateTime.UtcNow
            };

            await _context.ContactUs.AddAsync(contactMessage, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(StatusCodes.Status200OK, "Message sent successfully.", contactMessage.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
        }
    }
}

