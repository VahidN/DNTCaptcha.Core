import { Component, computed, effect, inject, input, signal, untracked } from '@angular/core';
import { NgOptimizedImage } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FieldTree, FormField } from '@angular/forms/signals';
import { DntCaptchaService } from '../service/dnt-captcha.service';
import type { DntCaptchaForm } from '../interfaces/dnt-captcha-form.interface';
import type { CaptchaState } from '../interfaces/dnt-captcha-state.type';
import { createRetryAfterCooldown } from '../../core/http/create-retry-after-cooldown';
import { InvalidApiResponseError } from '../../core/http/api-response-validation';

@Component({
  selector: 'app-dnt-captcha',
  imports: [NgOptimizedImage, FormField],
  templateUrl: './dnt-captcha.component.html',
  styles: ['.captcha-input { width: 65%; } .refresh { min-height: 44px; min-width: 44px; }']
})
export class DntCaptchaComponent {
  private readonly captchaService = inject(DntCaptchaService);
  private readonly refreshVersion = signal(0);
  readonly cooldown = createRetryAfterCooldown();

  readonly captchaForm = input.required<FieldTree<DntCaptchaForm>>();
  readonly inputId = input<string>();
  readonly captchaInputId = computed(
    () => this.inputId() ?? this.captchaForm().dntCaptchaInputText().name() + '-input'
  );
  readonly statusId = computed(() => this.captchaInputId() + '-status');
  readonly busy = input(false);
  readonly captchaImageUrl = signal('');
  readonly state = signal<CaptchaState>({ status: 'loading' });
  readonly statusMessage = computed(() => {
    const state = this.state();
    if (state.status === 'loading') return 'Loading CAPTCHA…';
    if (state.status === 'error') {
      return this.cooldown.active() ? 'Too many CAPTCHA requests. Wait for the cooldown to finish.' : state.message;
    }
    const answer = this.captchaForm().dntCaptchaInputText();
    if (answer.touched() && answer.invalid()) {
      return (
        answer
          .errors()
          .map((error) => error.message)
          .filter(Boolean)
          .join(' ') || 'The CAPTCHA result is invalid.'
      );
    }
    return 'Enter the result shown in the image.';
  });

  constructor() {
    effect((onCleanup) => {
      const field = this.captchaForm();
      const version = this.refreshVersion();
      // Editing the form must not become a dependency that triggers another HTTP request.
      untracked(() => {
        if (this.cooldown.active()) {
          // Replacing the form must not bypass the server's cooldown.
          field().reset({ dntCaptchaInputText: '', dntCaptchaText: '', dntCaptchaToken: '' });
          return;
        }
        this.clearChallenge(field);
        const request = this.captchaService.getDntCaptchaParams().subscribe({
          next: (data) => {
            if (this.refreshVersion() !== version || this.captchaForm() !== field) return;
            field().reset({
              dntCaptchaText: data.dntCaptchaTextValue,
              dntCaptchaToken: data.dntCaptchaTokenValue,
              dntCaptchaInputText: ''
            });
            this.captchaImageUrl.set(data.dntCaptchaImgUrl);
            this.state.set({ status: 'ready' });
          },
          error: (error: unknown) => {
            if (this.refreshVersion() !== version || this.captchaForm() !== field) return;
            let message = 'Could not load the CAPTCHA. Check your connection, then refresh it.';
            if (error instanceof InvalidApiResponseError) {
              message = 'The server returned an invalid CAPTCHA response. Refresh it to try again.';
            } else if (error instanceof HttpErrorResponse) {
              if (error.error instanceof DOMException && error.error.name === 'TimeoutError')
                message = 'The CAPTCHA request timed out. Refresh it to try again.';
              else if (error.status === 429) {
                this.cooldown.start(error.headers.get('Retry-After'));
                message = 'Too many CAPTCHA requests. Refresh it to try again when you are ready.';
              }
            }
            this.state.set({
              status: 'error',
              message
            });
          }
        });
        onCleanup(() => request.unsubscribe());
      });
    });
  }

  loadNewCaptcha(): void {
    if (this.cooldown.active()) return;
    // Invalidate the old challenge immediately, before the effect starts the next request.
    this.clearChallenge(this.captchaForm());
    this.refreshVersion.update((version) => version + 1);
  }

  onImageError(): void {
    this.clearChallenge(this.captchaForm());
    this.state.set({ status: 'error', message: 'The CAPTCHA image could not load. Refresh it to try again.' });
  }

  private clearChallenge(field: FieldTree<DntCaptchaForm>): void {
    field().reset({ dntCaptchaInputText: '', dntCaptchaText: '', dntCaptchaToken: '' });
    this.captchaImageUrl.set('');
    this.state.set({ status: 'loading' });
  }
}
