import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ToastrService } from '@openng/ngx-toastr';
import { LoginComponent } from './login.component';

describe('Login API response validation', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ToastrService, useValue: { success: vi.fn() } }
      ]
    });
  });

  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it.each([null, {}, { name: ' ' }])(
    'recovers from malformed login response %# without false success',
    async (body) => {
      // Use both real services to exercise validation before component state changes.
      const fixture = TestBed.createComponent(LoginComponent);
      const component = fixture.componentInstance;
      const http = TestBed.inject(HttpTestingController);
      const toastr = TestBed.inject(ToastrService);
      fixture.autoDetectChanges();

      function respondToCaptcha(): void {
        fixture.detectChanges();
        http.expectOne('api/account/CreateDNTCaptchaParams').flush({
          dntCaptchaImgUrl: new URL('/captcha.png', document.baseURI).href,
          dntCaptchaId: 'id',
          dntCaptchaTextValue: 'text',
          dntCaptchaTokenValue: 'token'
        });
      }

      respondToCaptcha();
      component.form.username().value.set('user');
      component.form.password().value.set('password');
      component.form.captcha.dntCaptchaInputText().value.set('42');
      const submission = component.submitForm();
      http.expectOne('api/account/login').flush(body);
      await submission;
      expect(component.state()).toMatchObject({ status: 'error' });
      expect(component.submitting()).toBe(false);
      expect(component.model().username).toBe('user');
      expect(component.model().password).toBe('password');
      expect(component.model().captcha.dntCaptchaToken).toBe('');
      expect(toastr.success).not.toHaveBeenCalled();
      respondToCaptcha();
      await fixture.whenStable();
      expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain('invalid login response');
      expect(fixture.nativeElement.querySelector('button[type="submit"]').disabled).toBe(false);
      component.form.captcha.dntCaptchaInputText().value.set('43');
      const retry = component.submitForm();
      http.expectOne('api/account/login').flush({ name: 'user' });
      await retry;
      respondToCaptcha();
      await fixture.whenStable();
      expect(component.state()).toMatchObject({ status: 'success' });
      expect(toastr.success).toHaveBeenCalledExactlyOnceWith('You are successfully logged in as user', 'Success');
    }
  );
});
