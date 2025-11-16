using Microsoft.AspNetCore.Builder;

namespace DNTCaptcha.TestWebApp.Common;

public static class SecurityHeadersBuilder
{
    public static HeaderPolicyCollection AddMyCustomCsp(this HeaderPolicyCollection hpc, bool isDevelopment)
    {
        hpc.AddFrameOptionsDeny()
            .AddXssProtectionBlock()
            .AddContentTypeOptionsNoSniff()
            .AddReferrerPolicyStrictOriginWhenCrossOrigin()
            .AddCrossOriginOpenerPolicy(builder => builder.SameOrigin())
            .AddCrossOriginResourcePolicy(builder => builder.SameOrigin())
            .AddCrossOriginEmbedderPolicy(builder => builder.RequireCorp())
            .AddContentSecurityPolicy(builder =>
            {
                builder.AddBaseUri().Self();
                builder.AddDefaultSrc().Self().From(uri: "blob:");
                builder.AddObjectSrc().Self().From(uri: "blob:");
                builder.AddBlockAllMixedContent();
                builder.AddImgSrc().Self().From(uri: "data:").From(uri: "blob:").From(uri: "https:");
                builder.AddFontSrc().Self();

                builder.AddStyleSrc()
                    .Self()

                    // Specify any additional hashes to permit your required inline styles to load.
                    // this is a sample ...
                    .WithHash256(hash: "bnIC0/ztx6X71evb78hypKsIopl034E0/ThEB7hd7Kg=")

                    //.WithHash256("....sha256 value.....")
                    ;

                builder.AddFrameAncestors().None();
                builder.AddConnectSrc().Self();
                builder.AddMediaSrc().Self();

                builder.AddScriptSrc()
                    .Self()

                    // Specify any additional hashes to permit your required inline scripts to load.
                    // this is a sample ...
                    .WithHash256(hash: "aMaNJuD4ukqKzy4/Ho066vyaf6Ua7INNrDS7Oi8iA34=")

                    // Specify unsafe-eval to permit the `Blazor WebAssembly Mono runtime` to function.
                    //.UnsafeEval()
                    .WithNonce() // It will generate the HttpContext.Items["NETESCAPADES_NONCE"] automatically
                    ;

                //TODO: Add api/CspReport/Log action method ...
                // https://www.dntips.ir/post/2706
                // builder.AddReportUri().To("/api/CspReport/Log");

                builder.AddUpgradeInsecureRequests();
            })
            .RemoveServerHeader()
            .AddPermissionsPolicy(builder =>
            {
                builder.AddAccelerometer().None();
                builder.AddAutoplay().None();
                builder.AddCamera().None();
                builder.AddEncryptedMedia().None();
                builder.AddFullscreen().All();
                builder.AddGeolocation().None();
                builder.AddGyroscope().None();
                builder.AddMagnetometer().None();
                builder.AddMicrophone().None();
                builder.AddMidi().None();
                builder.AddPayment().None();
                builder.AddPictureInPicture().None();
                builder.AddSyncXHR().None();
                builder.AddUsb().None();
            });

        if (!isDevelopment)
        {
            // maxAge = one year in seconds
            hpc.AddStrictTransportSecurityMaxAgeIncludeSubDomains();
        }

        return hpc;
    }
}
