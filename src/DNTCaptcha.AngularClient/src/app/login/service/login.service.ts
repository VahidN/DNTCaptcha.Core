import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { LoginDto } from '../interface/login.dto';
import type { LoginResponse } from '../interface/login-response.interface';

@Service()
export class LoginService {
  private readonly http = inject(HttpClient);

  login(body: LoginDto): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('api/account/login', new HttpParams({ fromObject: { ...body } }));
  }
}
