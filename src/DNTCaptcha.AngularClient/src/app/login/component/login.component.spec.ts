import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse, HttpHeaders, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ToastrService } from '@openng/ngx-toastr';
import { of, Subject, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { LoginService } from '../service/login.service';

const captchaEndpoint = 'api/account/CreateDNTCaptchaParams';

describe('LoginComponent', () => {
  let fixture: ComponentFixture<LoginComponent>;
  let component: LoginComponent;
  let http: HttpTestingController;
  let login: ReturnType<typeof vi.fn<LoginService['login']>>;
  let success: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    login = vi.fn<LoginService['login']>().mockReturnValue(of({ name: 'testUsername' }));
    success = vi.fn();
    TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ToastrService, useValue: { success } },
        { provide: LoginService, useValue: { login } }
      ]
    });
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.autoDetectChanges();
    respondToCaptcha();
  });

  afterEach(() => http.verify());

  function respondToCaptcha(): void {
    fixture.detectChanges();
    http.expectOne(captchaEndpoint).flush({
      dntCaptchaImgUrl: new URL('/captcha.png', document.baseURI).href,
      dntCaptchaTextValue: 'text',
      dntCaptchaTokenValue: 'token',
      dntCaptchaId: 'id'
    });
  }

  function fillForm(): void {
    component.form.username().value.set('testUsername');
    component.form.password().value.set('testPassword');
    component.form.captcha.dntCaptchaInputText().value.set('42');
  }

  it('rejects invalid submission and focuses the first invalid input', async () => {
    await fixture.whenStable();
    const form: HTMLFormElement = fixture.nativeElement.querySelector('form');
    form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    await fixture.whenStable();
    expect(login).not.toHaveBeenCalled();
    expect(document.activeElement?.id).toBe('username');
    expect(fixture.nativeElement.querySelector('#username-error').textContent).toContain('Enter your username');
    expect(fixture.nativeElement.querySelector('#password-error').textContent).toContain('Enter your password');
    expect(fixture.nativeElement.querySelector('[role="status"]').textContent).toContain('Enter the CAPTCHA result');
  });

  it('uses one native form and submits the flattened payload through FormRoot', async () => {
    fillForm();
    await fixture.whenStable();
    const form: HTMLFormElement = fixture.nativeElement.querySelector('form');
    const button: HTMLButtonElement = fixture.nativeElement.querySelector('button[type="submit"]');
    expect(fixture.nativeElement.querySelectorAll('form')).toHaveLength(1);
    expect(button.form).toBe(form);
    const captcha: HTMLInputElement = fixture.nativeElement.querySelector('input[autocomplete="off"]');
    expect(captcha.form).toBe(form);
    expect(captcha.labels?.[0]?.textContent).toContain('Captcha');
    form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    expect(login).toHaveBeenCalledExactlyOnceWith({
      username: 'testUsername',
      password: 'testPassword',
      dntCaptchaInputText: '42',
      dntCaptchaText: 'text',
      dntCaptchaToken: 'token'
    });
    await Promise.resolve();
    expect(success).toHaveBeenCalledWith('You are successfully logged in as testUsername', 'Success');
    expect(component.form.captcha.dntCaptchaInputText().value()).toBe('');
    respondToCaptcha();
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('[role="status"]').textContent).toContain('Enter the result');
  });

  it('prevents duplicate login requests and disables controls while submitting', async () => {
    const response = new Subject<{ name: string }>();
    login.mockReturnValue(response);
    fillForm();
    const submission = component.submitForm();
    expect(await component.submitForm()).toBe(false);
    await fixture.whenStable();
    expect(login).toHaveBeenCalledTimes(1);
    expect(component.submitting()).toBe(true);
    expect(fixture.nativeElement.querySelector('button[type="submit"]').disabled).toBe(true);
    expect(fixture.nativeElement.querySelector('button[type="button"]').disabled).toBe(true);
    expect(fixture.nativeElement.querySelector('#password').readOnly).toBe(true);
    response.next({ name: 'testUsername' });
    response.complete();
    await submission;
    respondToCaptcha();
    await fixture.whenStable();
    expect(component.submitting()).toBe(false);
    expect(fixture.nativeElement.querySelector('button[type="submit"]').disabled).toBe(false);
  });

  it.each([
    [0, 'Cannot reach the server'],
    [400, 'captcha/credentials are not valid'],
    [401, 'captcha/credentials are not valid'],
    [403, 'Login was denied'],
    [429, 'Too many login attempts'],
    [500, 'server could not complete the login']
  ])('reports HTTP %s accurately and preserves the credentials', async (status, message) => {
    login.mockReturnValue(throwError(() => new HttpErrorResponse({ status })));
    fillForm();
    await component.submitForm();
    expect(component.submitting()).toBe(false);
    expect(component.form.username().value()).toBe('testUsername');
    expect(component.form.password().value()).toBe('testPassword');
    expect(component.form.captcha.dntCaptchaInputText().value()).toBe('');
    respondToCaptcha();
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain(message);
    expect(success).not.toHaveBeenCalled();
  });

  it('does not submit while the CAPTCHA is loading or failed', async () => {
    fillForm();
    component.dntCaptchaComponent()!.loadNewCaptcha();
    component.submitForm();
    fixture.detectChanges();
    http.expectOne(captchaEndpoint).flush({}, { status: 500, statusText: 'Failed' });
    component.submitForm();
    await fixture.whenStable();
    expect(login).not.toHaveBeenCalled();
  });

  it('blocks native and programmatic login retries until Retry-After expires', async () => {
    vi.useFakeTimers();
    try {
      login.mockReturnValue(
        throwError(
          () =>
            new HttpErrorResponse({
              status: 429,
              headers: new HttpHeaders({ 'Retry-After': '3' })
            })
        )
      );
      fillForm();
      await component.submitForm();
      respondToCaptcha();
      fixture.detectChanges();
      const button: HTMLButtonElement = fixture.nativeElement.querySelector('button[type="submit"]');
      const form: HTMLFormElement = fixture.nativeElement.querySelector('form');
      expect(button.disabled).toBe(true);
      expect(button.textContent).toContain('Try again in 3s');
      expect(component.form.username().value()).toBe('testUsername');
      expect(component.form.password().value()).toBe('testPassword');
      component.form.captcha.dntCaptchaInputText().value.set('43');
      expect(await component.submitForm()).toBe(false);
      form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
      await vi.advanceTimersByTimeAsync(0);
      fixture.detectChanges();
      expect(login).toHaveBeenCalledTimes(1);
      await vi.advanceTimersByTimeAsync(2_000);
      fixture.detectChanges();
      expect(button.disabled).toBe(true);
      await vi.advanceTimersByTimeAsync(1_000);
      fixture.detectChanges();
      expect(button.disabled).toBe(false);
      expect(login).toHaveBeenCalledTimes(1);
      login.mockReturnValue(of({ name: 'testUsername' }));
      await component.submitForm();
      respondToCaptcha();
      fixture.detectChanges();
      expect(login).toHaveBeenCalledTimes(2);
      expect(success).toHaveBeenCalledTimes(1);
    } finally {
      fixture.destroy();
      vi.useRealTimers();
    }
  });

  it('recovers from a timeout without automatically repeating the login', async () => {
    const response = new Subject<{ name: string }>();
    login.mockReturnValue(response);
    fillForm();
    const submission = component.submitForm();
    response.error(new HttpErrorResponse({ status: 0, error: new DOMException('Timed out', 'TimeoutError') }));
    await submission;
    expect(component.submitting()).toBe(false);
    expect(component.form.username().value()).toBe('testUsername');
    expect(component.form.password().value()).toBe('testPassword');
    expect(component.form.captcha.dntCaptchaToken().value()).toBe('');
    respondToCaptcha();
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain('login request timed out');
    expect(fixture.nativeElement.querySelector('#password').readOnly).toBe(false);
    expect(fixture.nativeElement.querySelector('button[type="submit"]').disabled).toBe(false);
    expect(login).toHaveBeenCalledTimes(1);
    expect(success).not.toHaveBeenCalled();
    // Retrying requires a new answer and an explicit submission.
    login.mockReturnValue(of({ name: 'testUsername' }));
    component.form.captcha.dntCaptchaInputText().value.set('43');
    await component.submitForm();
    respondToCaptcha();
    await fixture.whenStable();
    expect(login).toHaveBeenCalledTimes(2);
    expect(success).toHaveBeenCalledTimes(1);
  });

  it('cancels a pending login subscription without showing feedback after destruction', async () => {
    const response = new Subject<{ name: string }>();
    login.mockReturnValue(response);
    fillForm();
    const submission = component.submitForm();
    expect(response.observed).toBe(true);
    fixture.destroy();
    expect(response.observed).toBe(false);
    await submission;
    expect(success).not.toHaveBeenCalled();
    expect(component.state()).toEqual({ status: 'idle' });
  });

  it('synchronizes browser input with the signal model and schema validation', async () => {
    await fixture.whenStable();
    const username: HTMLInputElement = fixture.nativeElement.querySelector('#username');
    username.value = 'typed-user';
    username.dispatchEvent(new Event('input', { bubbles: true }));
    username.dispatchEvent(new Event('blur'));
    await fixture.whenStable();
    expect(component.model().username).toBe('typed-user');
    expect(component.form.username().touched()).toBe(true);
    expect(component.form.username().valid()).toBe(true);

    component.form.username().value.set('');
    await fixture.whenStable();
    expect(username.value).toBe('');
    expect(username.getAttribute('aria-invalid')).toBe('true');
    expect(fixture.nativeElement.querySelector('#username-error').textContent).toContain('Enter your username');
  });
});
