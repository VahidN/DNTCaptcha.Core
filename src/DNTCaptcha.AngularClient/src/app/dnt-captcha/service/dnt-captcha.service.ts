import { inject, Service } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { DntCaptchaParams } from '../interfaces/dnt-captcha-params.interface';
import { parseDntCaptchaParams } from '../validation/parse-dnt-captcha-params';

@Service()
export class DntCaptchaService {
  private readonly http = inject(HttpClient);

  getDntCaptchaParams(): Observable<DntCaptchaParams> {
    return this.http.get<unknown>('api/account/CreateDNTCaptchaParams').pipe(map(parseDntCaptchaParams));
  }
}
