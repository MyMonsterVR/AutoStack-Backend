using System.Text.Json;
using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Utilities;
using Microsoft.Extensions.Logging;

namespace AutoStack.Infrastructure.Services;

/// <summary>
/// Service for logging audit entries to the database with GDPR-compliant sanitization
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IAuditLogRepository repository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<AuditLogService> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task LogAsync(AuditLogRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = request.UserIdOverride ?? _currentUserService.UserId;
            var username = request.UsernameOverride ?? _currentUserService.Username;

            var sanitizedException = request.Exception != null
                ? SanitizationHelper.SanitizeException(request.Exception)
                : null;

            var sanitizedBody = request.SanitizedRequestBody != null
                ? SanitizationHelper.SanitizeJson(request.SanitizedRequestBody)
                : null;

            var additionalDataJson = request.AdditionalData != null
                ? JsonSerializer.Serialize(request.AdditionalData)
                : null;

            var auditLog = AuditLog.CreateLog(
                level: request.Level,
                category: request.Category,
                message: request.Message,
                exception: sanitizedException,
                userId: userId,
                username: username,
                ipAddress: _currentUserService.IpAddress,
                userAgent: _currentUserService.UserAgent,
                httpMethod: _currentUserService.HttpMethod,
                endpoint: _currentUserService.Endpoint,
                statusCode: request.StatusCode,
                sanitizedRequestBody: sanitizedBody,
                durationMs: request.DurationMs,
                additionalData: additionalDataJson
            );

            await _repository.AddAsync(auditLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit entry to database: {Message}", ex.Message);
        }
    }

    public async Task LogInformationAsync(
        Domain.Enums.LogCategory category,
        string message,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(new AuditLogRequest
        {
            Level = Domain.Enums.LogLevel.Information,
            Category = category,
            Message = message
        }, cancellationToken);
    }

    public async Task LogWarningAsync(
        Domain.Enums.LogCategory category,
        string message,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(new AuditLogRequest
        {
            Level = Domain.Enums.LogLevel.Warning,
            Category = category,
            Message = message
        }, cancellationToken);
    }

    public async Task LogErrorAsync(
        Domain.Enums.LogCategory category,
        string message,
        Exception? exception = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(new AuditLogRequest
        {
            Level = Domain.Enums.LogLevel.Error,
            Category = category,
            Message = message,
            Exception = exception
        }, cancellationToken);
    }

    public async Task LogCriticalAsync(
        Domain.Enums.LogCategory category,
        string message,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(new AuditLogRequest
        {
            Level = Domain.Enums.LogLevel.Critical,
            Category = category,
            Message = message,
            Exception = exception
        }, cancellationToken);
    }
}
