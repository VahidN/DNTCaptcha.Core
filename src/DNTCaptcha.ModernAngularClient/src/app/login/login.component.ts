import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DntCaptchaForm } from '../dnt-captcha/interfaces/dnt-captcha.form';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { DntCaptchaComponent } from '../dnt-captcha/component/dnt-captcha.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, DntCaptchaComponent],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  loginForm: FormGroup = new FormGroup({
    username: new FormControl('', Validators.required),
    password: new FormControl('', Validators.required)
  });

  captchaForm: FormGroup<DntCaptchaForm> = new FormGroup<DntCaptchaForm>({
    captchaInputText: new FormControl('', Validators.required),
    captchaText: new FormControl('', Validators.required),
    captchaToken: new FormControl('', Validators.required)
  });
}
