#if NET7_0
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
    public DNTCaptchaRateLimiterPolicy(IOptions<DNTCaptchaOptions> options) =>
        _options = options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;

    /// <inheritdoc />
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; } = (context, cancellationToken) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter =
                    ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
            }

            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return default;
        };

    /// <inheritdoc />
    public RateLimitPartition<string> GetPartition(HttpContext httpContext) =>
        RateLimitPartition.GetFixedWindowLimiter(httpContext.GetClientIP(),
                                                 partition => new FixedWindowRateLimiterOptions
                                                              {
                                                                  AutoReplenishment = true,
                                                                  PermitLimit = _options.PermitLimit,
                                                                  QueueLimit = 0,
                                                                  Window = TimeSpan.FromMinutes(1),
                                                              });
}
#endif