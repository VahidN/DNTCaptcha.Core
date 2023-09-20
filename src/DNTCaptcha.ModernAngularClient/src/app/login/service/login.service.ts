import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { LoginDto } from '../interface/login.dto';

@Injectable({
  providedIn: 'root'
})
export class LoginService {
  http: HttpClient = inject(HttpClient);

  // body should contain dntCaptchaText, dntCaptchaToken, dntCaptchaInputText and actual form fields
  login(body: LoginDto): Observable<{ name: string }> {
    const data: string = new HttpParams({ fromObject: { ...body } }).toString();

    return this.http.post<{ name: string }>('api/account/login', data, {
      headers: new HttpHeaders().set('Content-Type', 'application/x-www-form-urlencoded')
    });
  }
}
