import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DNTCaptcha } from '../interfaces/dnt-captcha.interface';

@Injectable({
  providedIn: 'root'
})
export class DNTCaptchaService {
  // default endpoint from DNTCaptcha.TestApiApp
  captchaApiUrl = 'https://localhost:5001/api/account/CreateDNTCaptchaParams';
  http = inject(HttpClient);
  getDntCaptcha(): Observable<DNTCaptcha> {
    return this.http.get<DNTCaptcha>(this.captchaApiUrl);
  }
}
