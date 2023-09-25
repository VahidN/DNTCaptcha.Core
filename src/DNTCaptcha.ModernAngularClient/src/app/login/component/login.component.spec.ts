import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoginComponent } from './login.component';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ToastrService } from 'ngx-toastr';
import { LoginService } from '../service/login.service';
import { provideHttpClient } from '@angular/common/http';
import { of } from 'rxjs';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: ToastrService,
          useValue: {
            success: jest.fn(),
            error: jest.fn()
          }
        },
        {
          provide: LoginService,
          useValue: {
            login: jest.fn()
          }
        }
      ]
    });
  });

  it('should create', () => {
    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    expect(component).toBeTruthy();
  });

  describe('upon successful login', () => {
    let loginService: LoginService;

    beforeEach(() => {
      fixture = TestBed.createComponent(LoginComponent);
      component = fixture.componentInstance;
      fixture.detectChanges();

      component.dntCaptchaComponent.loadNewCaptcha = jest.fn();
      loginService = TestBed.inject(LoginService);
      loginService.login = jest.fn().mockReturnValue(of({ name: 'testUsername' }));

      component.captchaForm.setValue({
        dntCaptchaInputText: 'test',
        dntCaptchaText: 'test',
        dntCaptchaToken: 'test'
      });
      component.form.setValue({
        username: 'testUsername',
        password: 'testPassword'
      });
    });

    it('should pass merged forms as login parameters', () => {
      component.submitForm();
      expect(loginService.login).toHaveBeenCalledWith({
        username: 'testUsername',
        password: 'testPassword',
        dntCaptchaInputText: 'test',
        dntCaptchaText: 'test',
        dntCaptchaToken: 'test'
      });
    });

    it('should reload captcha', () => {
      component.submitForm();
      expect(component.dntCaptchaComponent.loadNewCaptcha).toHaveBeenCalledTimes(1);
    });

    it('should show success toast', () => {
      const toastr: ToastrService = TestBed.inject(ToastrService);
      toastr.success = jest.fn();

      component.submitForm();

      expect(toastr.success).toHaveBeenCalledWith('You are successfully logged in as testUsername', 'Success');
    });
  });
});
