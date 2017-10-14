import "rxjs/add/observable/throw";
import "rxjs/add/operator/catch";
import "rxjs/add/operator/map";

import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs/Observable";

import { LoginForm } from "./login-form";

@Injectable()
export class FormPosterService {
  private baseUrl = "http://localhost:5000/api/Ngx";

  constructor(private http: HttpClient) { }

  postUserForm(form: LoginForm): Observable<LoginForm> {
    const headers = new HttpHeaders({ "Content-Type": "application/json" });
    return this.http
      .post<LoginForm>(`${this.baseUrl}/login`, form, { headers: headers, withCredentials: true /* For CORS */ });
  }
}
