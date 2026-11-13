import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { DntCaptchaService } from './dnt-captcha.service';
import { DntCaptchaParams } from '../interfaces/dnt-captcha-params.interface';
import { InvalidApiResponseError } from '../../core/http/api-response-validation';

const captchaData: DntCaptchaParams = {
  dntCaptchaImgUrl: '/captcha.png',
  dntCaptchaTextValue: 'text',
  dntCaptchaTokenValue: 'token',
  dntCaptchaId: 'id'
};
const requiredFields = ['dntCaptchaImgUrl', 'dntCaptchaId', 'dntCaptchaTextValue', 'dntCaptchaTokenValue'] as const;

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

  it.each([null, [], 'unexpected', 42].map((body) => ({ body })))(
    'rejects malformed CAPTCHA response body %#',
    ({ body }) => {
      const next = vi.fn();
      const error = vi.fn();
      service.getDntCaptchaParams().subscribe({ next, error });
      http.expectOne('api/account/CreateDNTCaptchaParams').flush(body);
      expect(next).not.toHaveBeenCalled();
      expect(error).toHaveBeenCalledExactlyOnceWith(expect.any(InvalidApiResponseError));
    }
  );

  it.each(requiredFields)('rejects a response missing %s', (field) => {
    const body: Partial<DntCaptchaParams> = { ...captchaData };
    delete body[field];
    const error = vi.fn();
    service.getDntCaptchaParams().subscribe({ error });
    http.expectOne('api/account/CreateDNTCaptchaParams').flush(body);
    expect(error).toHaveBeenCalledExactlyOnceWith(expect.any(InvalidApiResponseError));
  });

  it.each(requiredFields.flatMap((field) => [null, 42, '', ' \t '].map((value) => ({ field, value }))))(
    'rejects invalid $field: $value',
    ({ field, value }) => {
      const error = vi.fn();
      service.getDntCaptchaParams().subscribe({ error });
      http.expectOne('api/account/CreateDNTCaptchaParams').flush({ ...captchaData, [field]: value });
      expect(error).toHaveBeenCalledExactlyOnceWith(expect.any(InvalidApiResponseError));
    }
  );

  it.each(['javascript:alert(1)', 'data:image/png;base64,AAAA', 'ftp://example.test/captcha', 'http://['])(
    'rejects an unsupported or invalid image URL: %s',
    (url) => {
      const error = vi.fn();
      service.getDntCaptchaParams().subscribe({ error });
      http.expectOne('api/account/CreateDNTCaptchaParams').flush({ ...captchaData, dntCaptchaImgUrl: url });
      expect(error).toHaveBeenCalledExactlyOnceWith(expect.any(InvalidApiResponseError));
    }
  );

  it.each(['/captcha.png', 'captcha.png?token=abc', 'http://localhost:5000/captcha', 'https://example.test/captcha'])(
    'accepts an HTTP image URL without modifying tokens: %s',
    (url) => {
      const next = vi.fn();
      service.getDntCaptchaParams().subscribe(next);
      const data = {
        ...captchaData,
        dntCaptchaImgUrl: url,
        dntCaptchaTokenValue: ' token+/%= ',
        dntCaptchaTextValue: ' text+/%= '
      };
      http.expectOne('api/account/CreateDNTCaptchaParams').flush({ ...data, additionalField: 'ignored' });
      expect(next).toHaveBeenCalledExactlyOnceWith(data);
    }
  );
});
