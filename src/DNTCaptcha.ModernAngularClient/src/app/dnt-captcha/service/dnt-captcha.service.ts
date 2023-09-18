import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DntCaptchaParams } from '../interfaces/dnt-captcha-params.interface';

@Injectable({
  providedIn: 'root'
})
export class DntCaptchaService {
  http: HttpClient = inject(HttpClient);
  getDntCaptchaParams(): Observable<DntCaptchaParams> {
    return this.http.get<DntCaptchaParams>('api/account/CreateDNTCaptchaParams');
  }

  // body should contain dntCaptchaText, dntCaptchaToken, dntCaptchaInputText and actual form fields
  validateDntCaptchaAndLogin(body: { [key: string]: string }): Observable<boolean> {
    const data: string = new HttpParams({ fromObject: body }).toString();

    return this.http.post<boolean>('api/account/login', data, {
      headers: new HttpHeaders().set('Content-Type', 'application/x-www-form-urlencoded')
    });
  }
}
