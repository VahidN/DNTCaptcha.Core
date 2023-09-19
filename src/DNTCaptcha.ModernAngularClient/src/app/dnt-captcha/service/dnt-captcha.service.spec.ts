import { TestBed } from '@angular/core/testing';
import { DntCaptchaService } from './dnt-captcha.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('DntCaptchaService', () => {
  let service: DntCaptchaService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });
    service = TestBed.inject(DntCaptchaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
