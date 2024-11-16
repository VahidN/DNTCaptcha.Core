using System;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace DNTCaptcha.Core;

/// <summary>
///     ASP.NET Core encodes attribute values in a TagBuilder.
///     This is a solution to disable this behaviour.
/// </summary>
/// <remarks>
///     ASP.NET Core encodes attribute values in a TagBuilder.
///     This is a solution to disable this behaviour.
/// </remarks>
public class DNTScriptTag(string innerText, string nonce) : IHtmlContent
{
    /// <inheritdoc />
    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        writer.Write($"""<script nonce="{nonce}" type="text/javascript">""");
        writer.Write(innerText);
        writer.Write(value: "</script>");
    }
}