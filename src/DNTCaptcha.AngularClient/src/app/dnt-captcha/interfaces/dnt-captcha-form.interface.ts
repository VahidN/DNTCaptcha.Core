import { AbstractControl } from '@angular/forms';

export interface DntCaptchaForm {
  dntCaptchaInputText: AbstractControl<string>;
  dntCaptchaText: AbstractControl<string>;
  dntCaptchaToken: AbstractControl<string>;
}
