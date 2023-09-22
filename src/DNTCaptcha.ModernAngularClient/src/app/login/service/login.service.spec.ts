import { TestBed } from '@angular/core/testing';
import { LoginService } from './login.service';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { LoginDto } from '../interface/login.dto';

const loginDataMock: LoginDto = {
  dntCaptchaText: 'dntCaptchaText',
  dntCaptchaToken: 'dntCaptchaToken',
  dntCaptchaInputText: 'dntCaptchaInputText',
  username: 'username',
  password: 'password'
};

describe('LoginService', () => {
  let service: LoginService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), LoginService]
    });
    service = TestBed.inject(LoginService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should perform POST operation and return name', () => {
    let captchaData;

    service.login(loginDataMock).subscribe((data) => {
      captchaData = data;
    });

    const req = httpMock.expectOne('api/account/login');
    req.flush({ name: 'test' });

    expect(req.request.method).toEqual('POST');
    expect(captchaData).toEqual({ name: 'test' });
  });
});
