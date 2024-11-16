using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace DNTCaptcha.Core;

/// <summary>
///     HttpContext Extensions
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    ///     Gets the current HttpContext.Request's IP.
    /// </summary>
    public static string GetClientIP(this HttpContext httpContext, bool tryUseXForwardHeader = true)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var ip = string.Empty;

        // X-Forwarded-For (csv list):  Using the First entry in the list seems to work
        // for 99% of cases however it has been suggested that a better (although tedious)
        // approach might be to read each IP from right to left and use the first public static IP.
        // http://stackoverflow.com/a/43554000/538763
        //
        if (tryUseXForwardHeader)
        {
            ip = SplitCsv(GetHeaderValue(httpContext, headerName: "X-Forwarded-For")).FirstOrDefault();
        }

        // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
        if (string.IsNullOrWhiteSpace(ip) && httpContext.Connection?.RemoteIpAddress != null)
        {
            ip = httpContext.Connection.RemoteIpAddress.ToString();
        }

        if (string.IsNullOrWhiteSpace(ip))
        {
            ip = GetHeaderValue(httpContext, headerName: "REMOTE_ADDR");
        }

        return ip ?? "unknown";
    }

    private static List<string> SplitCsv(string? csvList)
        => string.IsNullOrWhiteSpace(csvList)
            ? []
            : csvList.TrimEnd(trimChar: ',').Split(separator: ',').AsEnumerable().Select(s => s.Trim()).ToList();

    private static string? GetHeaderValue(HttpContext httpContext, string headerName)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        return httpContext.Request?.Headers?.TryGetValue(headerName, out var values) ?? false
            ? values.ToString()
            : string.Empty;
    }
}