import { Component, inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DntCaptchaComponent } from '../dnt-captcha/component/dnt-captcha.component';
import { DntCaptchaService } from '../dnt-captcha/service/dnt-captcha.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, DntCaptchaComponent, ReactiveFormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  dntCaptchaService: DntCaptchaService = inject(DntCaptchaService);
  toastr: ToastrService = inject(ToastrService);
  @ViewChild(DntCaptchaComponent) dntCaptchaComponent!: DntCaptchaComponent;

  form: FormGroup = new FormGroup({
    username: new FormControl<string>('', { nonNullable: true, validators: Validators.required }),
    password: new FormControl<string>('', { nonNullable: true, validators: Validators.required })
  });

  login(): void {
    if (!this.form.valid || !this.dntCaptchaComponent.captchaForm.valid) {
      return;
    }

    this.dntCaptchaService
      .validateDntCaptchaAndLogin({ ...this.form.getRawValue(), ...this.dntCaptchaComponent.captchaForm.getRawValue() })
      .subscribe({
        next: () => {
          this.toastr.success('You are logged in successfully.', 'Success');
          this.dntCaptchaComponent.loadNewCaptcha();
        },
        error: () => {
          this.toastr.error('The entered captcha/credentials are not valid.', 'Error');

          this.dntCaptchaComponent.loadNewCaptcha();
        }
      });
  }
}
