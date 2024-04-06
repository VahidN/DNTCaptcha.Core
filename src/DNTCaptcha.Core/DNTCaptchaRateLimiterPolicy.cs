#if NET7_0 || NET8_0
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

    /// <summary>
    ///     The response that is sent to user when rate limit has exceeded.
    /// </summary>
    private static string? _rejectedResponse { get; set; } = string.Empty;
    private static bool? _rejectedResponseType { get; set; } = false;

    private readonly DNTCaptchaOptions _options;

    /// <summary>
    ///     limiting the number of requests that can be made in a given period of time
    /// </summary>
    public DNTCaptchaRateLimiterPolicy(IOptions<DNTCaptchaOptions> options)
    {
        _options = options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;
        _rejectedResponse = _options.RateLimiterRejectedResponse;
        if (!string.IsNullOrEmpty(_rejectedResponse))
        {
            _rejectedResponse = _rejectedResponse.Replace("/TimeoutWindow/", "1", StringComparison.OrdinalIgnoreCase).Replace("/PermitLimit/", _options.PermitLimit.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
            _rejectedResponseType = _options.RateLimiterRejectedResponseType;
        }
    }

    /// <inheritdoc />
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; } = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (!string.IsNullOrEmpty(_rejectedResponse) && _rejectedResponseType != null)
        {
            context.HttpContext.Response.Headers.ContentType = (bool)_rejectedResponseType ? "application/json" : "text/plain";
            await context.HttpContext.Response.WriteAsync(_rejectedResponse);
        }
    };

    /// <inheritdoc />
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        return RateLimitPartition.GetFixedWindowLimiter(httpContext.GetClientIP(),
            partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = _options.PermitLimit,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    }
}
#endif