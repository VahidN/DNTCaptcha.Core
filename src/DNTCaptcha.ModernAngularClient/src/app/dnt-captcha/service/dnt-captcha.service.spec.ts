import { TestBed } from '@angular/core/testing';
import { DntCaptchaService } from './dnt-captcha.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ToastrService } from 'ngx-toastr';

describe('DntCaptchaService', () => {
  let service: DntCaptchaService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        DntCaptchaService,
        {
          provide: ToastrService,
          useValue: {
            error: jest.fn()
          }
        }
      ]
    });
    service = TestBed.inject(DntCaptchaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
