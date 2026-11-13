import { inject, Service } from '@angular/core';
import { map, Observable } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { LoginDto } from '../interface/login.dto';
import type { LoginResponse } from '../interface/login-response.interface';
import { parseLoginResponse } from '../validation/parse-login-response';

@Service()
export class LoginService {
  private readonly http = inject(HttpClient);

  login(body: LoginDto): Observable<LoginResponse> {
    return this.http
      .post<unknown>('api/account/login', new HttpParams({ fromObject: { ...body } }))
      .pipe(map(parseLoginResponse));
  }
}
