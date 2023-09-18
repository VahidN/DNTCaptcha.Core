import { Component, inject, Input, OnInit } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { DntCaptchaService } from '../service/dnt-captcha.service';
import { DntCaptchaParams } from '../interfaces/dnt-captcha-params.interface';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { DntCaptchaForm } from '../interfaces/dnt-captcha-form.interface';

@Component({
  selector: 'app-dnt-captcha',
  standalone: true,
  imports: [CommonModule, NgOptimizedImage, ReactiveFormsModule],
  templateUrl: './dnt-captcha.component.html',
  styles: ['i { font-size: 1.125rem } form { width: 65%;} button { height: 38px; width: 38px;}']
})
export class DntCaptchaComponent implements OnInit {
  @Input({ required: true }) captchaForm!: FormGroup<DntCaptchaForm>;

  captchaImageUrl = '';

  dntCaptchaService = inject(DntCaptchaService);
  ngOnInit(): void {
    this.loadCaptcha();
  }

  loadCaptcha(): void {
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
