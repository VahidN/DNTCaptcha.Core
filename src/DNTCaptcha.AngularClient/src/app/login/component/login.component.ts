import { Component, inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { LoginService } from '../service/login.service';
import { DntCaptchaComponent } from '../../dnt-captcha/component/dnt-captcha.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, DntCaptchaComponent, ReactiveFormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  @ViewChild(DntCaptchaComponent) dntCaptchaComponent!: DntCaptchaComponent;

  loginService: LoginService = inject(LoginService);

  toastr: ToastrService = inject(ToastrService);

  form: FormGroup = new FormGroup({
    username: new FormControl<string>('', { nonNullable: true, validators: Validators.required }),
    password: new FormControl<string>('', { nonNullable: true, validators: Validators.required })
  });

  captchaForm: FormGroup = new FormGroup({
    dntCaptchaInputText: new FormControl('', { nonNullable: true, validators: Validators.required }),
    dntCaptchaText: new FormControl('', { nonNullable: true, validators: Validators.required }),
    dntCaptchaToken: new FormControl('', { nonNullable: true, validators: Validators.required })
  });

  submitForm(): void {
    if (!this.form.valid || !this.dntCaptchaComponent.captchaForm.valid) {
      return;
    }

    this.loginService.login({ ...this.form.getRawValue(), ...this.captchaForm.getRawValue() }).subscribe({
      next: (data: { name: string }): void => {
        this.toastr.success(`You are successfully logged in as ${data.name}`, 'Success');
        this.dntCaptchaComponent.loadNewCaptcha();
      },
      error: (): void => {
        this.toastr.error('The entered captcha/credentials are not valid.', 'Error');

        this.dntCaptchaComponent.loadNewCaptcha();
      }
    });
  }
}
