using System;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace DNTCaptcha.Core;

/// <summary>
///     ASP.NET Core encodes attribute values in a TagBuilder.
///     This is a solution to disable this behaviour.
/// </summary>
public class DNTScriptTag : IHtmlContent
{
    private readonly string _innerText;
    private readonly string _nonce;

    /// <summary>
    ///     ASP.NET Core encodes attribute values in a TagBuilder.
    ///     This is a solution to disable this behaviour.
    /// </summary>
    public DNTScriptTag(string innerText, string nonce)
    {
        _innerText = innerText;
        _nonce = nonce;
    }

    /// <inheritdoc />
    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        writer.Write($"""<script nonce="{_nonce}" type="text/javascript">""");
        writer.Write(_innerText);
        writer.Write("</script>");
    }
}