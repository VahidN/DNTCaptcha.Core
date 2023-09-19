import { Component, inject, OnInit } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { DntCaptchaService } from '../service/dnt-captcha.service';
import { DntCaptchaParams } from '../interfaces/dnt-captcha-params.interface';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DntCaptchaForm } from '../interfaces/dnt-captcha-form.interface';

@Component({
  selector: 'app-dnt-captcha',
  standalone: true,
  imports: [CommonModule, NgOptimizedImage, ReactiveFormsModule],
  templateUrl: './dnt-captcha.component.html',
  styles: ['i { font-size: 1.125rem } form { width: 65%;} button { height: 38px; width: 38px;}']
})
export class DntCaptchaComponent implements OnInit {
  dntCaptchaService = inject(DntCaptchaService);

  captchaImageUrl = '';

  captchaForm: FormGroup<DntCaptchaForm> = new FormGroup<DntCaptchaForm>({
    dntCaptchaInputText: new FormControl('', { nonNullable: true, validators: Validators.required }),
    dntCaptchaText: new FormControl('', { nonNullable: true, validators: Validators.required }),
    dntCaptchaToken: new FormControl('', { nonNullable: true, validators: Validators.required })
  });
  ngOnInit(): void {
    this.loadNewCaptcha();
  }

  loadNewCaptcha(): void {
    this.dntCaptchaService.getDntCaptchaParams().subscribe((data: DntCaptchaParams) => {
      this.captchaImageUrl = data.dntCaptchaImgUrl;
      this.captchaForm.reset({
        dntCaptchaText: data.dntCaptchaTextValue,
        dntCaptchaToken: data.dntCaptchaTokenValue,
        dntCaptchaInputText: ''
      });
    });
  }
}
