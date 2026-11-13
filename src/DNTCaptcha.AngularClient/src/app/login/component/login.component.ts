import { Component, computed, DestroyRef, inject, signal, viewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FieldTree, form, FormField, FormRoot, readonly as readOnly, required, submit } from '@angular/forms/signals';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { firstValueFrom } from 'rxjs';
import { ToastrService } from '@openng/ngx-toastr';
import { LoginService } from '../service/login.service';
import { DntCaptchaComponent } from '../../dnt-captcha/component/dnt-captcha.component';
import type { LoginFormModel } from '../interface/login-form.interface';
import type { LoginState } from '../interface/login-state.type';

@Component({
  selector: 'app-login',
  imports: [DntCaptchaComponent, FormField, FormRoot],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private readonly loginService = inject(LoginService);
  private readonly toastr = inject(ToastrService);
  private readonly destroyRef = inject(DestroyRef);
  readonly dntCaptchaComponent = viewChild(DntCaptchaComponent);

  readonly state = signal<LoginState>({ status: 'idle' });
  readonly model = signal<LoginFormModel>({
    username: '',
    password: '',
    captcha: { dntCaptchaInputText: '', dntCaptchaText: '', dntCaptchaToken: '' }
  });
  readonly form: FieldTree<LoginFormModel> = form(
    this.model,
    (path) => {
      required(path.username, { message: 'Enter your username.' });
      required(path.password, { message: 'Enter your password.' });
      required(path.captcha.dntCaptchaInputText, { message: 'Enter the CAPTCHA result.' });
      required(path.captcha.dntCaptchaText);
      required(path.captcha.dntCaptchaToken);
      readOnly(path.username, { when: () => this.submitting() });
      readOnly(path.password, { when: () => this.submitting() });
      readOnly(path.captcha.dntCaptchaInputText, {
        when: () => this.submitting() || this.dntCaptchaComponent()?.state().status !== 'ready'
      });
    },
    {
      submission: {
        action: (field) => this.login(field),
        onInvalid: (field) => {
          const firstInvalid = [field.username, field.password, field.captcha.dntCaptchaInputText].find(
            (control) => control().invalid() && !control().readonly()
          );
          firstInvalid?.().focusBoundControl();
        },
        ignoreValidators: 'none'
      }
    }
  );
  readonly submitting = computed(() => this.form().submitting());

  submitForm(): Promise<boolean> {
    return submit(this.form);
  }

  private async login(field: FieldTree<LoginFormModel>): Promise<{ kind: string; message: string } | undefined> {
    this.state.set({ status: 'idle' });
    const { captcha, ...credentials } = field().value();
    try {
      const { name } = await firstValueFrom(
        this.loginService.login({ ...credentials, ...captcha }).pipe(takeUntilDestroyed(this.destroyRef))
      );
      if (this.destroyRef.destroyed) return undefined;
      const message = `You are successfully logged in as ${name}`;
      this.state.set({ status: 'success', message });
      this.toastr.success(message, 'Success');
      this.dntCaptchaComponent()?.loadNewCaptcha();
      return undefined;
    } catch (error: unknown) {
      // Destroying the component cancels the request and completes firstValueFrom without a value.
      if (this.destroyRef.destroyed) return undefined;
      let message = 'The server could not complete the login. Try again later.';
      if (error instanceof HttpErrorResponse) {
        if (error.error instanceof DOMException && error.error.name === 'TimeoutError')
          message = 'The login request timed out. Try again with the new CAPTCHA.';
        else if (error.status === 0) message = 'Cannot reach the server. Check your connection and try again.';
        else if (error.status === 429) message = 'Too many login attempts. Wait a moment and try again.';
        else if (error.status === 400 || error.status === 401)
          message = 'The entered captcha/credentials are not valid.';
        else if (error.status === 403) message = 'Login was denied. Check your access and try again.';
      }
      this.state.set({ status: 'error', message });
      this.dntCaptchaComponent()?.loadNewCaptcha();
      return { kind: 'server', message };
    }
  }
}
