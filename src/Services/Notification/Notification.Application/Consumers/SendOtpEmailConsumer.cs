using BuildingBlocks.Messaging.Contracts;
using Hangfire;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Notification.Application.Consumers;

public sealed class SendOtpEmailConsumer : IConsumer<SendOtpEmailMessage>
{
    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly ILogger<SendOtpEmailConsumer> _logger;

    public SendOtpEmailConsumer(IBackgroundJobClient backgroundJobs, ILogger<SendOtpEmailConsumer> logger)
    {
        _backgroundJobs = backgroundJobs;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SendOtpEmailMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Received OTP email request for {Email}", msg.Email);
        _backgroundJobs.Enqueue<Jobs.SendOtpEmailJob>(
            job => job.ExecuteAsync(msg.Email, msg.OtpCode, msg.ExpiresAt, CancellationToken.None));
        return Task.CompletedTask;
    }
}
