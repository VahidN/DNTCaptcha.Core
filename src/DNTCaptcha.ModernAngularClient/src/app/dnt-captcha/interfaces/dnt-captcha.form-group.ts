import { AbstractControl } from '@angular/forms';

export interface DntCaptchaFormGroup {
  captchaInputText: AbstractControl;
  captchaText: AbstractControl;
  captchaToken: AbstractControl;
}
