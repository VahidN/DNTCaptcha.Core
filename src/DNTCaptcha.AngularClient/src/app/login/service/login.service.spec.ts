import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { LoginService } from './login.service';
import { LoginDto } from '../interface/login.dto';

const loginData: LoginDto = {
  dntCaptchaText: 'text',
  dntCaptchaToken: 'token',
  dntCaptchaInputText: '42',
  username: 'user+name@example.test',
  password: 'p&ss=word+value'
};

describe('LoginService', () => {
  let service: LoginService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(LoginService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('posts credentials and CAPTCHA fields as form-encoded data', () => {
    const next = vi.fn();
    service.login(loginData).subscribe(next);
    const request = http.expectOne('api/account/login');
    expect(request.request.method).toBe('POST');
    expect(request.request.detectContentTypeHeader()).toBe('application/x-www-form-urlencoded;charset=UTF-8');
    const body = request.request.serializeBody();
    expect(typeof body).toBe('string');
    expect(Object.fromEntries(new URLSearchParams(String(body)))).toEqual(loginData);
    request.flush({ name: 'test' });
    expect(next).toHaveBeenCalledExactlyOnceWith({ name: 'test' });
  });
});
