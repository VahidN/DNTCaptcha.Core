import { Component, computed, effect, ElementRef, inject, input, signal, untracked, viewChild } from '@angular/core';
import { NgOptimizedImage } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FieldTree, FormField } from '@angular/forms/signals';
import { Subscription } from 'rxjs';
import { DntCaptchaService } from '../service/dnt-captcha.service';
import type { DntCaptchaForm } from '../interfaces/dnt-captcha-form.interface';
import type { CaptchaState } from '../interfaces/dnt-captcha-state.type';
import { createRetryAfterCooldown } from '../../core/http/create-retry-after-cooldown';
import { InvalidApiResponseError } from '../../core/http/api-response-validation';

@Component({
  selector: 'app-dnt-captcha',
  imports: [NgOptimizedImage, FormField],
  templateUrl: './dnt-captcha.component.html',
  styleUrl: './dnt-captcha.component.scss'
})
export class DntCaptchaComponent {
  private readonly captchaService = inject(DntCaptchaService);
  private readonly refreshVersion = signal(0);
  private readonly captchaAudio = viewChild<ElementRef<HTMLAudioElement>>('captchaAudio');
  private audioRequest?: Subscription;
  private objectUrl = '';
  private previousAudioUrl = '';
  private playbackVersion = 0;
  readonly cooldown = createRetryAfterCooldown();
  readonly audioCooldown = createRetryAfterCooldown();

  readonly captchaForm = input.required<FieldTree<DntCaptchaForm>>();
  readonly mode = input<'image' | 'audio'>('image');
  readonly inputId = input<string>();
  readonly captchaInputId = computed(
    () => this.inputId() ?? this.captchaForm().dntCaptchaInputText().name() + '-input'
  );
  readonly statusId = computed(() => this.captchaInputId() + '-status');
  readonly busy = input(false);
  readonly captchaImageUrl = signal('');
  readonly captchaAudioUrl = signal('');
  readonly isAudioLoading = signal(false);
  readonly isPlaying = signal(false);
  readonly audioError = signal('');
  readonly state = signal<CaptchaState>({ status: 'loading' });
  readonly statusMessage = computed(() => {
    const state = this.state();
    if (state.status === 'loading') return 'Loading CAPTCHA…';
    if (state.status === 'error') {
      return this.cooldown.active() ? 'Too many CAPTCHA requests. Wait for the cooldown to finish.' : state.message;
    }
    if (this.isAudioLoading()) return 'Loading audio…';
    if (this.audioError()) return this.audioError();
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
    return this.mode() === 'audio' ? 'Listen and type the digits in order.' : 'Enter the result shown in the image.';
  });

  constructor() {
    effect((onCleanup) => {
      const field = this.captchaForm();
      const version = this.refreshVersion();
      const mode = this.mode();
      // Form edits and playback state must not trigger a new CAPTCHA request.
      untracked(() => {
        this.clearChallenge(field);
        if (this.cooldown.active()) {
          this.state.set({ status: 'error', message: 'Too many CAPTCHA requests. Refresh it when you are ready.' });
          return;
        }
        const current = () => this.refreshVersion() === version && this.captchaForm() === field && this.mode() === mode;
        const request = this.captchaService.getDntCaptchaParams(mode, this.previousAudioUrl).subscribe({
          next: (data) => {
            if (!current()) return;
            field().reset({
              dntCaptchaText: data.dntCaptchaTextValue,
              dntCaptchaToken: data.dntCaptchaTokenValue,
              dntCaptchaInputText: ''
            });
            this.captchaImageUrl.set(data.dntCaptchaImgUrl);
            this.captchaAudioUrl.set(data.dntCaptchaAudioUrl ?? '');
            this.previousAudioUrl = '';
            this.state.set({ status: 'ready' });
          },
          error: (error: unknown) => {
            if (!current()) return;
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
            this.state.set({ status: 'error', message });
          }
        });
        onCleanup(() => {
          request.unsubscribe();
          this.stopPlayback(true);
        });
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

  playCaptcha(): void {
    if (this.busy()) return;
    if (this.isPlaying()) {
      this.stopPlayback();
      return;
    }
    if (
      !this.captchaAudioUrl() ||
      this.state().status !== 'ready' ||
      this.isAudioLoading() ||
      this.audioCooldown.active()
    )
      return;
    this.audioError.set('');
    const version = ++this.playbackVersion;
    if (this.objectUrl) {
      void this.startPlayback(version);
      return;
    }
    this.isAudioLoading.set(true);
    this.audioRequest = this.captchaService.getAudio(this.captchaAudioUrl()).subscribe({
      next: (audio) => {
        if (version !== this.playbackVersion) return;
        this.objectUrl = URL.createObjectURL(audio);
        void this.startPlayback(version);
      },
      error: (error: unknown) => {
        if (version !== this.playbackVersion) return;
        this.isAudioLoading.set(false);
        let message = 'Could not load the audio. Refresh the CAPTCHA and try again.';
        if (error instanceof HttpErrorResponse) {
          if (error.status === 429) {
            this.audioCooldown.start(error.headers.get('Retry-After'));
            message = 'Too many audio requests. Wait for the cooldown, then try again.';
          } else if (error.status === 400) {
            this.clearChallenge(this.captchaForm());
            this.state.set({
              status: 'error',
              message: 'The audio CAPTCHA expired or is unavailable. Refresh it to try again.'
            });
            return;
          } else if (error.error instanceof DOMException && error.error.name === 'TimeoutError') {
            message = 'The audio request timed out. Select Play audio to try again.';
          }
        }
        this.audioError.set(message);
      }
    });
  }

  playbackEnded(): void {
    this.isPlaying.set(false);
  }

  playbackFailed(): void {
    if (!this.captchaAudio()?.nativeElement.getAttribute('src')) return;
    this.stopPlayback(true);
    this.audioError.set('Could not play the audio. Refresh the CAPTCHA and try again.');
  }

  private async startPlayback(version: number): Promise<void> {
    const audio = this.captchaAudio()?.nativeElement;
    if (!audio || version !== this.playbackVersion) return;
    audio.src = this.objectUrl;
    audio.currentTime = 0;
    this.isAudioLoading.set(true);
    try {
      await audio.play();
      if (version === this.playbackVersion) {
        this.isAudioLoading.set(false);
        this.isPlaying.set(true);
      }
    } catch (error) {
      if (version !== this.playbackVersion) return;
      if (error instanceof DOMException && error.name === 'NotAllowedError') {
        this.isAudioLoading.set(false);
        this.audioError.set('Playback was blocked. Select Play audio again.');
      } else {
        this.playbackFailed();
      }
    }
  }

  private stopPlayback(releaseAudio = false): void {
    this.playbackVersion++;
    this.audioRequest?.unsubscribe();
    const audio = this.captchaAudio()?.nativeElement;
    if (audio && (audio.getAttribute('src') || this.objectUrl)) {
      audio.pause();
      audio.currentTime = 0;
      if (releaseAudio) {
        audio.removeAttribute('src');
        audio.load();
      }
    }
    this.isPlaying.set(false);
    this.isAudioLoading.set(false);
    if (releaseAudio && this.objectUrl) {
      URL.revokeObjectURL(this.objectUrl);
      this.objectUrl = '';
    }
  }

  private clearChallenge(field: FieldTree<DntCaptchaForm>): void {
    if (this.captchaAudioUrl()) this.previousAudioUrl = this.captchaAudioUrl();
    this.stopPlayback(true);
    field().reset({ dntCaptchaInputText: '', dntCaptchaText: '', dntCaptchaToken: '' });
    this.captchaImageUrl.set('');
    this.captchaAudioUrl.set('');
    this.audioError.set('');
    this.state.set({ status: 'loading' });
  }
}
