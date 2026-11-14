import type { DntCaptchaForm } from '../../dnt-captcha/interfaces/dnt-captcha-form.interface';

export interface LoginFormModel {
  username: string;
  password: string;
  captcha: DntCaptchaForm;
}
