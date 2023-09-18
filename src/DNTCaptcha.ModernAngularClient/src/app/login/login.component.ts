import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DntCaptchaComponent } from '../dnt-captcha/component/dnt-captcha.component';
import { DntCaptchaService } from '../dnt-captcha/service/dnt-captcha.service';
import { DntCaptchaForm } from '../dnt-captcha/interfaces/dnt-captcha-form.interface';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, DntCaptchaComponent, ReactiveFormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  captchaService = inject(DntCaptchaService);

  form: FormGroup = new FormGroup({
    username: new FormControl<string>('', { nonNullable: true, validators: Validators.required }),
    password: new FormControl<string>('', { nonNullable: true, validators: Validators.required })
  });

  captchaForm: FormGroup<DntCaptchaForm> = new FormGroup<DntCaptchaForm>({
    dntCaptchaInputText: new FormControl('', { nonNullable: true, validators: Validators.required }),
    dntCaptchaText: new FormControl('', { nonNullable: true, validators: Validators.required }),
    dntCaptchaToken: new FormControl('', { nonNullable: true, validators: Validators.required })
  });

  login(): void {
    if (!this.form.valid || !this.captchaForm.valid) {
      return;
    }

    this.captchaService
      .validateDntCaptchaAndLogin({ ...this.form.getRawValue(), ...this.captchaForm.getRawValue() })
      .subscribe((response: boolean): void => {
        console.log(response);
      });
  }
}
