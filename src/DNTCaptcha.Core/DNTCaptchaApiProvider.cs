using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DNTCaptcha.Core;

/// <summary>
///     DNTCaptcha Api
/// </summary>
/// <remarks>
///     DNTCaptcha Api
/// </remarks>
public class DNTCaptchaApiProvider(
    ICaptchaCryptoProvider captchaProtectionProvider,
    IRandomNumberProvider randomNumberProvider,
    Func<DisplayMode, ICaptchaTextProvider> captchaTextProvider,
    ICaptchaStorageProvider captchaStorageProvider,
    ISerializationProvider serializationProvider,
    IHttpContextAccessor httpContextAccessor,
    IUrlHelper urlHelper,
    IOptions<DNTCaptchaOptions> options) : IDNTCaptchaApiProvider
{
    private readonly DNTCaptchaOptions _captchaOptions =
        options is null ? throw new ArgumentNullException(nameof(options)) : options.Value;

    private readonly ICaptchaCryptoProvider _captchaProtectionProvider = captchaProtectionProvider ??
                                                                         throw new ArgumentNullException(
                                                                             nameof(captchaProtectionProvider));

    private readonly ICaptchaStorageProvider _captchaStorageProvider =
        captchaStorageProvider ?? throw new ArgumentNullException(nameof(captchaStorageProvider));

    private readonly Func<DisplayMode, ICaptchaTextProvider> _captchaTextProvider =
        captchaTextProvider ?? throw new ArgumentNullException(nameof(captchaTextProvider));

    private readonly IHttpContextAccessor _httpContextAccessor =
        httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    private readonly IRandomNumberProvider _randomNumberProvider =
        randomNumberProvider ?? throw new ArgumentNullException(nameof(randomNumberProvider));

    private readonly ISerializationProvider _serializationProvider =
        serializationProvider ?? throw new ArgumentNullException(nameof(serializationProvider));

    private readonly IUrlHelper _urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));

    private readonly CaptchaAudioService? _captchaAudioService;

    /// <summary>
    ///     Creates the API provider with optional audio integration, preserving the original constructor for visual modes.
    /// </summary>
    public DNTCaptchaApiProvider(ICaptchaCryptoProvider captchaProtectionProvider,
        IRandomNumberProvider randomNumberProvider, Func<DisplayMode, ICaptchaTextProvider> captchaTextProvider,
        ICaptchaStorageProvider captchaStorageProvider, ISerializationProvider serializationProvider,
        IHttpContextAccessor httpContextAccessor, IUrlHelper urlHelper, IOptions<DNTCaptchaOptions> options,
        CaptchaAudioService captchaAudioService)
        : this(captchaProtectionProvider, randomNumberProvider, captchaTextProvider, captchaStorageProvider,
            serializationProvider, httpContextAccessor, urlHelper, options)
        => _captchaAudioService = captchaAudioService ?? throw new ArgumentNullException(nameof(captchaAudioService));

    /// <summary>
    ///     Creates DNTCaptcha
    /// </summary>
    /// <param name="captchaAttributes">captcha attributes</param>
    public DNTCaptchaApiResponse CreateDNTCaptcha(DNTCaptchaTagHelperHtmlAttributes captchaAttributes)
    {
        ArgumentNullException.ThrowIfNull(captchaAttributes);

        if (_httpContextAccessor.HttpContext is null)
        {
            throw new InvalidOperationException(message: "`_httpContextAccessor.HttpContext` is null.");
        }

        if (captchaAttributes.DisplayMode == DisplayMode.AudioDigits &&
            (captchaAttributes.Min < 0 || captchaAttributes.Max < captchaAttributes.Min ||
             (long)captchaAttributes.Max - captchaAttributes.Min >= int.MaxValue))
        {
            throw new ArgumentOutOfRangeException(nameof(captchaAttributes),
                "AudioDigits requires a non-negative range smaller than Int32.MaxValue.");
        }

        var number = _randomNumberProvider.NextNumber(captchaAttributes.Min, captchaAttributes.Max);

        var randomText = _captchaTextProvider(captchaAttributes.DisplayMode)
            .GetText(number, captchaAttributes.Language);

        var captchaDivId = string.Create(CultureInfo.InvariantCulture,
            $"{_captchaOptions.CaptchaClass}{Guid.NewGuid():N}{_randomNumberProvider.NextNumber(captchaAttributes.Min, captchaAttributes.Max)}");

        var cookieToken = $".{captchaDivId}";
        var hiddenInputToken = _captchaProtectionProvider.Encrypt(cookieToken);

        var value = number.ToString(CultureInfo.InvariantCulture);
        var encryptedValue = _captchaProtectionProvider.Encrypt(value);

        var audioUrl = captchaAttributes.DisplayMode == DisplayMode.AudioDigits
            ? GetCaptchaAudioUrl(captchaAttributes, randomText, cookieToken)
            : null;

        var captchaImageUrl = captchaAttributes.DisplayMode == DisplayMode.AudioDigits
            ? string.Empty
            : GetCaptchaImageUrl(captchaAttributes, _captchaProtectionProvider.Encrypt(randomText));

        _captchaStorageProvider.Add(_httpContextAccessor.HttpContext, cookieToken, value);

        return new DNTCaptchaApiResponse
        {
            DntCaptchaImgUrl = captchaImageUrl,
            DntCaptchaAudioUrl = audioUrl,
            DntCaptchaId = captchaDivId,
            DntCaptchaTextValue = encryptedValue,
            DntCaptchaTokenValue = hiddenInputToken
        };
    }

    private string GetCaptchaAudioUrl(DNTCaptchaTagHelperHtmlAttributes attributes, string digits, string token)
    {
        var context = _httpContextAccessor.HttpContext ??
                      throw new InvalidOperationException("HttpContext is null.");

        var audioService = _captchaAudioService ??
                           throw new InvalidOperationException("Use the audio-enabled constructor or register services.AddDNTCaptcha() for AudioDigits.");
        var data = audioService.Protect(context, digits, token, attributes.Language, attributes.AudioVoice);
        var controller = string.IsNullOrEmpty(_captchaOptions.CaptchaImageControllerNameTemplate)
            ? nameof(DNTCaptchaImageController).Replace("Controller", string.Empty, StringComparison.Ordinal)
            : _captchaOptions.CaptchaImageControllerNameTemplate;

        var routeValues = new { data, area = "" };
        var url = attributes.UseRelativeUrls
            ? _urlHelper.Action(nameof(DNTCaptchaImageController.Audio), controller, routeValues)
            : _urlHelper.Action(nameof(DNTCaptchaImageController.Audio), controller, routeValues, context.Request.Scheme);

        return string.IsNullOrWhiteSpace(url)
            ? throw new InvalidOperationException(
                "Cannot determine the URL of DNTCaptchaImageController.Audio. Register controllers and map controller routes.")
            : url;
    }

    private string GetCaptchaImageUrl(DNTCaptchaTagHelperHtmlAttributes captchaAttributes, string encryptedText)
    {
        if (_httpContextAccessor.HttpContext is null)
        {
            throw new InvalidOperationException(message: "`_httpContextAccessor.HttpContext` is null.");
        }

        var values = new CaptchaImageParams
        {
            Text = encryptedText,
            RndDate = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture),
            ForeColor = captchaAttributes.ForeColor,
            BackColor = captchaAttributes.BackColor,
            FontSize = captchaAttributes.FontSize,
            FontName = captchaAttributes.FontName
        };

        var encryptSerializedValues = _captchaProtectionProvider.Encrypt(_serializationProvider.Serialize(values));

        var controllerName = nameof(DNTCaptchaImageController)
            .Replace(oldValue: "Controller", string.Empty, StringComparison.Ordinal);

        if (!string.IsNullOrEmpty(_captchaOptions.CaptchaImageControllerNameTemplate))
        {
            controllerName = _captchaOptions.CaptchaImageControllerNameTemplate;
        }

        var actionUrl = captchaAttributes.UseRelativeUrls
            ? _urlHelper.Action(nameof(DNTCaptchaImageController.Show), controllerName, new
            {
                data = encryptSerializedValues,
                area = ""
            })
            : _urlHelper.Action(nameof(DNTCaptchaImageController.Show), controllerName, new
            {
                data = encryptSerializedValues,
                area = ""
            }, _httpContextAccessor.HttpContext.Request.Scheme);

        if (string.IsNullOrWhiteSpace(actionUrl))
        {
            throw new InvalidOperationException(
                message:
                "It's not possible to determine the URL of the `DNTCaptchaImageController.Show` method. Please register the `services.AddControllers()` and `endpoints.MapControllerRoute(...)`.");
        }

        return actionUrl;
    }
}
