# Razor Pages audio sample

From the repository root, run:

```sh
dotnet run --project src/DNTCaptcha.TestRazorPages
```

Open https://localhost:5001. The home page uses `AudioDigits` with the optional
English audio package and Female recordings. Press play, enter the four digits
you hear, fill in any user name and password, and submit. A correct answer
redirects to the Privacy page. An incorrect answer displays a validation error.

Use the refresh icon to generate a new challenge. Playback can be repeated
without consuming the answer; refresh invalidates the previous audio URL.
The sample permits ten CAPTCHA endpoint requests per minute, so wait a minute
if repeated playback and refresh hit the rate limit.

To test the other English voice, change `asp-captcha-audio-voice` to `Male` in
`Pages/Index.cshtml`. To return to the visual sample, change
`asp-captcha-generator-display-mode` to `NumberToWord`.
