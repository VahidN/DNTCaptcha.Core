import { Component, inject, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DNTCaptchaService } from '../service/dnt-captcha.service';
import { DNTCaptcha } from '../interfaces/dnt-captcha.interface';
import { FormGroup } from '@angular/forms';
import { DntCaptchaFormGroup } from '../interfaces/dnt-captcha.form-group';

@Component({
  selector: 'app-dnt-captcha',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dnt-captcha.component.html',
  styleUrls: ['./dnt-captcha.component.scss']
})
export class DntCaptchaComponent implements OnInit {
  @Input({ required: true }) captchaForm!: FormGroup<DntCaptchaFormGroup>;

  dntCaptchaService = inject(DNTCaptchaService);
  ngOnInit() {
    this.dntCaptchaService.getDntCaptcha().subscribe((data: DNTCaptcha) => {});
  }
}
