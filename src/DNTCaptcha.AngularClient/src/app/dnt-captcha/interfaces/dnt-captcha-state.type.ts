export type CaptchaState = { status: 'loading' } | { status: 'ready' } | { status: 'error'; message: string };
