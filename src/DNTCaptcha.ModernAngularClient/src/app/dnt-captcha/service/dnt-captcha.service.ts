import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { DntCaptchaParams } from '../interfaces/dnt-captcha-params.interface';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})
export class DntCaptchaService {
  http: HttpClient = inject(HttpClient);
  toastr: ToastrService = inject(ToastrService);
  getDntCaptchaParams(): Observable<DntCaptchaParams> {
    return this.http.get<DntCaptchaParams>('api/account/CreateDNTCaptchaParams').pipe(
      catchError((err) => {
        if (err.status === 429) {
          this.toastr.error('Too many requests for captcha generation.', 'Error');
        }

        return throwError(err);
      })
    );
  }

  // body should contain dntCaptchaText, dntCaptchaToken, dntCaptchaInputText and actual form fields
  validateDntCaptchaAndLogin(body: { [key: string]: string }): Observable<boolean> {
    const data: string = new HttpParams({ fromObject: body }).toString();

    return this.http.post<boolean>('api/account/login', data, {
      headers: new HttpHeaders().set('Content-Type', 'application/x-www-form-urlencoded')
    });
  }
}
