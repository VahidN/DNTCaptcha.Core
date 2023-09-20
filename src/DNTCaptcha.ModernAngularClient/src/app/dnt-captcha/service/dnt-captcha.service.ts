import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { DntCaptchaParams } from '../interfaces/dnt-captcha-params.interface';
import { ToastrService } from 'ngx-toastr';

@Injectable()
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
}
