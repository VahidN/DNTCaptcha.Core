import { TestBed } from '@angular/core/testing';
import { DntCaptchaService } from './dnt-captcha.service';

describe('DntCaptchaService', () => {
  let service: DntCaptchaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DntCaptchaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
