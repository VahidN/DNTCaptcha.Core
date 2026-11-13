import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { DntCaptchaService } from './dnt-captcha.service';
import { DntCaptchaParams } from '../interfaces/dnt-captcha-params.interface';

const captchaData: DntCaptchaParams = {
  dntCaptchaImgUrl: '/captcha.png',
  dntCaptchaTextValue: 'text',
  dntCaptchaTokenValue: 'token',
  dntCaptchaId: 'id'
};

describe('DntCaptchaService', () => {
  let service: DntCaptchaService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    http = TestBed.inject(HttpTestingController);
    service = TestBed.inject(DntCaptchaService);
  });

  afterEach(() => http.verify());

  it('fetches the CAPTCHA parameters', () => {
    const next = vi.fn();
    service.getDntCaptchaParams().subscribe(next);
    const request = http.expectOne('api/account/CreateDNTCaptchaParams');
    expect(request.request.method).toBe('GET');
    request.flush(captchaData);
    expect(next).toHaveBeenCalledExactlyOnceWith(captchaData);
  });

  it('preserves HTTP failures for the component to handle', () => {
    const error = vi.fn();
    service.getDntCaptchaParams().subscribe({ error });
    http.expectOne('api/account/CreateDNTCaptchaParams').flush({}, { status: 429, statusText: 'Too Many Requests' });
    expect(error).toHaveBeenCalledWith(expect.objectContaining({ status: 429 }));
  });
});
