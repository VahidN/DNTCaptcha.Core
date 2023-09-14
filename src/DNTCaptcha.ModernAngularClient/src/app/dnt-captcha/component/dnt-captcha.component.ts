import { Component, inject, Input, OnInit } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { DNTCaptchaService } from '../service/dnt-captcha.service';
import { DNTCaptcha } from '../interfaces/dnt-captcha.interface';
import { FormGroup } from '@angular/forms';
import { DntCaptchaForm } from '../interfaces/dnt-captcha.form';

@Component({
  selector: 'app-dnt-captcha',
  standalone: true,
  imports: [CommonModule, NgOptimizedImage],
  templateUrl: './dnt-captcha.component.html',
  styleUrls: ['./dnt-captcha.component.scss']
})
export class DntCaptchaComponent implements OnInit {
  @Input({ required: true }) captchaForm!: FormGroup<DntCaptchaForm>;
  captchaImageUrl = '';

  dntCaptchaService = inject(DNTCaptchaService);
  ngOnInit() {
    this.dntCaptchaService.getDntCaptcha().subscribe((data: DNTCaptcha) => {
      this.captchaImageUrl = data.dntCaptchaImgUrl;
      console.log(this.captchaImageUrl);
      this.captchaForm.reset({
        captchaInputText: data.DNTCaptchaTextValue,
        captchaToken: data.DNTCaptchaTokenValue,
        captchaText: ''
      });
    });
  }
}
