#if NET7_0 || NET8_0 || NET9_0
using System;
using System.Globalization;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core;

/// <summary>
///     limiting the number of requests that can be made in a given period of time
/// </summary>
public class DNTCaptchaRateLimiterPolicy : IRateLimiterPolicy<string>
{
    /// <summary>
    ///     The name of this policy: DNTCaptcha-RateLimiter-Policy
    /// </summary>
    public const string Name = "DNTCaptcha-RateLimiter-Policy";

    private readonly DNTCaptchaOptions _options;

    /// <summary>
    ///     limiting the number of requests that can be made in a given period of time
    /// </summary>
    public DNTCaptchaRateLimiterPolicy(IOptions<DNTCaptchaOptions> options)
    {
        _options = options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;
        RejectedResponse = _options.RateLimiterRejectedResponse;

        if (!string.IsNullOrEmpty(RejectedResponse))
        {
            RejectedResponse = RejectedResponse
                .Replace(oldValue: "/TimeoutWindow/", newValue: "1", StringComparison.OrdinalIgnoreCase)
                .Replace(oldValue: "/PermitLimit/", _options.PermitLimit.ToString(CultureInfo.InvariantCulture),
                    StringComparison.OrdinalIgnoreCase);

            RejectedResponseType = _options.RateLimiterRejectedResponseType;
        }
    }

    /// <summary>
    ///     The response that is sent to user when rate limit has exceeded.
    /// </summary>
    private static string? RejectedResponse { get; set; } = string.Empty;

    private static bool? RejectedResponseType { get; set; } = false;

    /// <inheritdoc />
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; } =
        async (context, cancellationToken) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter =
                    ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
            }

            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

            if (!string.IsNullOrEmpty(RejectedResponse) && RejectedResponseType != null)
            {
                context.HttpContext.Response.Headers.ContentType =
                    (bool)RejectedResponseType ? "application/json" : "text/plain";

                await context.HttpContext.Response.WriteAsync(RejectedResponse);
            }
        };

    /// <inheritdoc />
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
        => RateLimitPartition.GetFixedWindowLimiter(httpContext.GetClientIP(), partition
            => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = _options.PermitLimit,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(value: 1)
            });
}
#endif