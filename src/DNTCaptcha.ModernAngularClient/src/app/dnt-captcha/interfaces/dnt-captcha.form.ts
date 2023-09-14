import { AbstractControl } from '@angular/forms';

export interface DntCaptchaForm {
  captchaInputText: AbstractControl;
  captchaText: AbstractControl;
  captchaToken: AbstractControl;
}
