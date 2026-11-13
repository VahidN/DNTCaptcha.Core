import { inject, Service } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DntCaptchaParams } from '../interfaces/dnt-captcha-params.interface';

@Service()
export class DntCaptchaService {
  private readonly http = inject(HttpClient);

  getDntCaptchaParams(): Observable<DntCaptchaParams> {
    return this.http.get<DntCaptchaParams>('api/account/CreateDNTCaptchaParams');
  }
}
