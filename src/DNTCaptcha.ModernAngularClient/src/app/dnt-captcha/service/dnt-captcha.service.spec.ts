import { TestBed } from '@angular/core/testing';

import { DNTCaptchaService } from './dnt-captcha.service';

describe('DntCaptchaService', () => {
  let service: DNTCaptchaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DNTCaptchaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
