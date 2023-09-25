import { TestBed } from '@angular/core/testing';
import { DntCaptchaService } from './dnt-captcha.service';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { first } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { DntCaptchaParams } from '../interfaces/dnt-captcha-params.interface';

const captchaDataMock: DntCaptchaParams = {
  dntCaptchaImgUrl: 'dntCaptchaImgUrl',
  dntCaptchaTextValue: 'dntCaptchaTextValue',
  dntCaptchaTokenValue: 'dntCaptchaTokenValue',
  dntCaptchaId: 'dntCaptchaId'
};

describe('DntCaptchaService', () => {
  let service: DntCaptchaService;
  let httpMock: HttpTestingController;

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
    httpMock = TestBed.inject(HttpTestingController);
    service = TestBed.inject(DntCaptchaService);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should perform GET operation and return captchaData', () => {
    let captchaData: DntCaptchaParams | null = null;

    service
      .getDntCaptchaParams()
      .pipe(first())
      .subscribe((data: DntCaptchaParams) => {
        captchaData = data;
      });

    const req = httpMock.expectOne('api/account/CreateDNTCaptchaParams');

    req.flush(captchaDataMock);

    expect(req.request.method).toEqual('GET');
    expect(captchaData).toEqual({
      dntCaptchaImgUrl: 'dntCaptchaImgUrl',
      dntCaptchaTextValue: 'dntCaptchaTextValue',
      dntCaptchaTokenValue: 'dntCaptchaTokenValue',
      dntCaptchaId: 'dntCaptchaId'
    });
  });
});
