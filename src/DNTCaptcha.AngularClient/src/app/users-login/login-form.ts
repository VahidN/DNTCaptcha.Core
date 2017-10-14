import { DNTCaptchaBase } from "../dnt-captcha/dnt-captcha-base";

export class LoginForm extends DNTCaptchaBase {
  constructor(
    public username: string,
    public password: string) { super(); }
}

