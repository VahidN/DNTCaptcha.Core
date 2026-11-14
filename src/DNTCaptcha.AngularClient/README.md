# DntCaptchaAngularClient

Simple frontend for DNTCaptcha.TestApiApp backend.

Uses Angular 22 with `@openng/ngx-toastr` for notifications and Vitest with jsdom
for unit tests. Build and test targets use Angular's `@angular/build` package.

Requires Node.js 24.16 or later in the Node 24 LTS line and npm. If you use nvm,
run `nvm use` in this directory before installing dependencies.

### How to run it:

1. run DNTCaptcha.TestApiApp project from IDE(Visual Studio/Rider) or just by `dotnet watch run`
2. run `npm ci` inside DNTCaptcha.AngularClient/
3. run `ng serve` or `npm run start` inside DNTCaptcha.AngularClient/
4. navigate to `localhost:4200`

### Validation

Run these commands from this directory:

```sh
npm run build
npm test -- --watch=false
npm run lint
npm run format:check
```

Use `npm test` for interactive watch mode.

The `Angular CI` GitHub Actions workflow runs on pushes and pull requests and can
also be started manually. It uses the Node.js version in `.nvmrc`, caches npm's
downloads using `package-lock.json`, installs with `npm ci`, then checks formatting,
lint, unit tests without watch mode, and the production build. The job runs from
this directory and does not require the API backend. Its check is named
`Angular checks`.

Run `npm run format` to apply formatting. Generated files are excluded by
`.prettierignore`; ESLint also checks formatting in TypeScript, HTML, and its own
configuration file.

The boundaries rules permit app composition files to import login code, and login
code to import the CAPTCHA module. Imports within each module are allowed. Reverse
dependencies and unclassified folders under `src/app` are rejected. App composition
and both features can import shared HTTP utilities from `core`; `core` cannot
import either feature.

The login page loads lazily and uses a single native form, so Enter submits it.
Pending logins block repeat submissions and CAPTCHA refreshes. CAPTCHA refreshes
cancel older requests; failed requests display an inline message and can be retried
with the refresh button. Credentials are preserved when login fails.

Forms use Angular Signal Forms (`form`, `FormField`, and `FormRoot`) with a shared
signal model and schema validation. Signal Forms manage touched state, validation,
read-only controls, and concurrent submission protection. The CAPTCHA loads through
a component effect whose cleanup cancels requests on refresh, form replacement, or
component destruction. Render hooks are reserved for work that needs rendered DOM.

Bootstrap is compiled from `src/bootstrap.scss`, which includes only the components
and utility families used by this sample. Extend that entry point when adding new
Bootstrap features. Bootstrap 5 still uses legacy Sass imports; only that import
deprecation is silenced, while Angular's template diagnostics remain enabled.

HTTP uses Angular's default Fetch backend. CAPTCHA input and status IDs come from
their Signal Forms field name; pass an explicit `inputId` when a stable custom ID
is needed or when displaying the same field in multiple places.

The app and tests use Angular's default zoneless change detection; Zone.js is not
installed or loaded. Signal Forms, component signals, and the toast library notify
Angular when asynchronous state changes need to render.

HTTP requests have a 15-second backend timeout through `requestTimeoutInterceptor`.
Override `HTTP_REQUEST_TIMEOUT_MS` in application providers to change the default,
or pass a request's `timeout` option to override it individually. Values must be
positive integers in milliseconds. Timed-out requests
are aborted and show an inline recovery message. Login attempts are not retried
automatically; credentials are preserved and a fresh CAPTCHA is loaded for retry.

HTTP 429 responses with a valid `Retry-After` header start a cooldown for the
affected action. Both seconds and HTTP-date values are supported. CAPTCHA refresh
and login submission are guarded until their cooldown expires, including direct
method calls and native form submission. The UI shows the remaining wait; it does
not automatically repeat the rejected request. Missing, invalid, or expired
headers allow a manual retry. Timers are cleaned up when the component is destroyed.

API services receive JSON as `unknown` and validate it before exposing typed values.
Login requires a non-empty name; CAPTCHA requires a non-empty ID, encrypted text,
token, and a relative or HTTP(S) image URL. Extra response fields are ignored and
valid strings are preserved without trimming. Invalid successful responses produce
an inline recovery message rather than login success or a usable CAPTCHA. A failed
login preserves credentials and loads a new challenge for manual retry.
