import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { FieldTree, form as createForm, readonly as readOnly, required } from '@angular/forms/signals';
import { DntCaptchaComponent } from './dnt-captcha.component';
import { DntCaptchaForm } from '../interfaces/dnt-captcha-form.interface';
import { DntCaptchaService } from '../service/dnt-captcha.service';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { throwError } from 'rxjs';

const endpoint = 'api/account/CreateDNTCaptchaParams';

describe('DntCaptchaComponent', () => {
  let fixture: ComponentFixture<DntCaptchaComponent>;
  let component: DntCaptchaComponent;
  let form: FieldTree<DntCaptchaForm>;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [DntCaptchaComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    http = TestBed.inject(HttpTestingController);
    form = TestBed.runInInjectionContext(() =>
      createForm(
        signal<DntCaptchaForm>({
          dntCaptchaInputText: 'old answer',
          dntCaptchaText: 'old text',
          dntCaptchaToken: 'old token'
        }),
        (path) => {
          readOnly(path.dntCaptchaInputText, { when: () => component.busy() || component.state().status !== 'ready' });
          required(path.dntCaptchaInputText);
          required(path.dntCaptchaText);
          required(path.dntCaptchaToken);
        }
      )
    );
    fixture = TestBed.createComponent(DntCaptchaComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('captchaForm', form);
    fixture.autoDetectChanges();
  });

  afterEach(() => http.verify());

  function respond(id = 'new'): string {
    fixture.detectChanges();
    const imageUrl = new URL('/captcha-' + id + '.png', document.baseURI).href;
    http.expectOne(endpoint).flush({
      dntCaptchaImgUrl: imageUrl,
      dntCaptchaTextValue: id + ' text',
      dntCaptchaTokenValue: id + ' token',
      dntCaptchaId: id
    });
    return imageUrl;
  }

  it('clears the previous challenge and makes the input read-only while loading', () => {
    expect(component.state().status).toBe('loading');
    expect(form().value()).toEqual({ dntCaptchaInputText: '', dntCaptchaText: '', dntCaptchaToken: '' });
    expect(fixture.nativeElement.querySelector('input').readOnly).toBe(true);
    expect(fixture.nativeElement.querySelector('button').disabled).toBe(true);
    respond();
  });

  it('renders an asynchronous challenge, resets the answer, and associates the input label', async () => {
    const imageUrl = respond();
    await fixture.whenStable();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(fixture.nativeElement.querySelector('img').getAttribute('src')).toBe(imageUrl);
    expect(input.labels?.[0]?.textContent).toContain('Captcha');
    expect(input.readOnly).toBe(false);
    expect(fixture.nativeElement.querySelector('form')).toBeNull();
    expect(form().value()).toEqual({
      dntCaptchaInputText: '',
      dntCaptchaText: 'new text',
      dntCaptchaToken: 'new token'
    });
  });

  it('cancels an older refresh before accepting the new challenge', async () => {
    const oldRequest = http.expectOne(endpoint);
    component.loadNewCaptcha();
    fixture.detectChanges();
    expect(oldRequest.cancelled).toBe(true);
    const imageUrl = respond('latest');
    await fixture.whenStable();
    expect(component.captchaImageUrl()).toBe(imageUrl);
    expect(form.dntCaptchaToken().value()).toBe('latest token');
  });

  it.each([
    [0, 'Check your connection'],
    [429, 'Too many CAPTCHA requests'],
    [500, 'Could not load the CAPTCHA']
  ])('handles HTTP %s and permits a successful retry', async (status, message) => {
    const request = http.expectOne(endpoint);
    if (status === 0) request.error(new ProgressEvent('error'));
    else request.flush({}, { status, statusText: 'Failed' });
    await fixture.whenStable();
    expect(component.state()).toMatchObject({ status: 'error' });
    expect(fixture.nativeElement.querySelector('[role="status"]').textContent).toContain(message);
    expect(fixture.nativeElement.querySelector('button').disabled).toBe(false);
    expect(form().invalid()).toBe(true);
    component.loadNewCaptcha();
    respond();
    await fixture.whenStable();
    expect(component.state().status).toBe('ready');
  });

  it('invalidates the challenge if the image fails to load', async () => {
    respond();
    await fixture.whenStable();
    fixture.nativeElement.querySelector('img').dispatchEvent(new Event('error'));
    await fixture.whenStable();
    expect(component.state().status).toBe('error');
    expect(form.dntCaptchaToken().value()).toBe('');
    expect(fixture.nativeElement.querySelector('img')).toBeNull();
  });

  it('keeps a malformed challenge unusable and permits a valid refresh', async () => {
    http.expectOne(endpoint).flush({
      dntCaptchaImgUrl: '/captcha.png',
      dntCaptchaId: 'id',
      dntCaptchaTextValue: 'text'
    });
    await fixture.whenStable();
    expect(component.state().status).toBe('error');
    expect(fixture.nativeElement.querySelector('[role="status"]').textContent).toContain('invalid CAPTCHA response');
    expect(fixture.nativeElement.querySelector('img')).toBeNull();
    expect(form().value()).toEqual({ dntCaptchaInputText: '', dntCaptchaText: '', dntCaptchaToken: '' });
    expect(form().invalid()).toBe(true);
    expect(fixture.nativeElement.querySelector('input').readOnly).toBe(true);
    expect(fixture.nativeElement.querySelector('button').disabled).toBe(false);
    component.loadNewCaptcha();
    respond('valid');
    await fixture.whenStable();
    expect(component.state().status).toBe('ready');
    expect(fixture.nativeElement.querySelector('input').readOnly).toBe(false);
    expect(form.dntCaptchaToken().value()).toBe('valid token');
  });

  it('blocks refresh until Retry-After expires, then permits a manual retry', async () => {
    vi.useFakeTimers();
    try {
      http.expectOne(endpoint).flush(
        {},
        {
          status: 429,
          statusText: 'Too Many Requests',
          headers: { 'Retry-After': '3' }
        }
      );
      fixture.detectChanges();
      const button: HTMLButtonElement = fixture.nativeElement.querySelector('button');
      expect(button.disabled).toBe(true);
      expect(fixture.nativeElement.querySelector('[role="status"]').textContent).toContain('Try again in 3s');
      expect(form.dntCaptchaToken().value()).toBe('');
      component.loadNewCaptcha();
      fixture.detectChanges();
      http.expectNone(endpoint);
      const replacement = TestBed.runInInjectionContext(() =>
        createForm(
          signal<DntCaptchaForm>({ dntCaptchaInputText: 'old', dntCaptchaText: 'old', dntCaptchaToken: 'old' })
        )
      );
      fixture.componentRef.setInput('captchaForm', replacement);
      fixture.detectChanges();
      http.expectNone(endpoint);
      expect(replacement.dntCaptchaToken().value()).toBe('');
      await vi.advanceTimersByTimeAsync(2_000);
      fixture.detectChanges();
      expect(button.disabled).toBe(true);
      expect(fixture.nativeElement.querySelector('[role="status"]').textContent).toContain('Try again in 1s');
      await vi.advanceTimersByTimeAsync(1_000);
      fixture.detectChanges();
      expect(button.disabled).toBe(false);
      http.expectNone(endpoint);
      button.click();
      respond('retry');
      fixture.detectChanges();
      expect(component.state().status).toBe('ready');
    } finally {
      fixture.destroy();
      vi.useRealTimers();
    }
  });

  it('cleans up an active cooldown when destroyed', () => {
    vi.useFakeTimers();
    try {
      http.expectOne(endpoint).flush(
        {},
        {
          status: 429,
          statusText: 'Too Many Requests',
          headers: { 'Retry-After': '60' }
        }
      );
      expect(component.cooldown.active()).toBe(true);
      fixture.destroy();
      expect(component.cooldown.active()).toBe(false);
    } finally {
      vi.useRealTimers();
    }
  });

  it('reports a timeout and permits a successful manual refresh', async () => {
    respond();
    vi.spyOn(TestBed.inject(DntCaptchaService), 'getDntCaptchaParams').mockReturnValueOnce(
      throwError(() => new HttpErrorResponse({ status: 0, error: new DOMException('Timed out', 'TimeoutError') }))
    );
    component.loadNewCaptcha();
    await fixture.whenStable();
    expect(component.state().status).toBe('error');
    expect(fixture.nativeElement.querySelector('[role="status"]').textContent).toContain('CAPTCHA request timed out');
    expect(fixture.nativeElement.querySelector('button').disabled).toBe(false);
    expect(form().invalid()).toBe(true);
    expect(form.dntCaptchaToken().value()).toBe('');
    component.loadNewCaptcha();
    respond('retry');
    await fixture.whenStable();
    expect(component.state().status).toBe('ready');
    expect(fixture.nativeElement.querySelector('input').readOnly).toBe(false);
  });

  it('disables refresh and editing while the parent submits', async () => {
    respond();
    fixture.componentRef.setInput('busy', true);
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('button').disabled).toBe(true);
    expect(fixture.nativeElement.querySelector('input').readOnly).toBe(true);
  });

  it('cancels the pending request when destroyed', () => {
    const request = http.expectOne(endpoint);
    fixture.destroy();
    expect(request.cancelled).toBe(true);
  });

  it('does not refetch when the answer or touched state changes', async () => {
    const imageUrl = respond();
    await fixture.whenStable();
    form.dntCaptchaInputText().value.set('42');
    form.dntCaptchaInputText().markAsTouched();
    await fixture.whenStable();
    http.expectNone(endpoint);
    expect(component.captchaImageUrl()).toBe(imageUrl);
    expect(form().value().dntCaptchaInputText).toBe('42');
  });

  it('ignores a stale response arriving before the refresh effect runs', async () => {
    const oldRequest = http.expectOne(endpoint);
    component.loadNewCaptcha();
    oldRequest.flush({
      dntCaptchaImgUrl: '/stale.png',
      dntCaptchaTextValue: 'stale text',
      dntCaptchaTokenValue: 'stale token',
      dntCaptchaId: 'stale'
    });
    expect(component.captchaImageUrl()).toBe('');
    expect(form.dntCaptchaToken().value()).toBe('');
    const imageUrl = respond('latest');
    await fixture.whenStable();
    expect(component.captchaImageUrl()).toBe(imageUrl);
    expect(form.dntCaptchaToken().value()).toBe('latest token');
  });

  it('cancels the old request when its form input is replaced', async () => {
    const oldRequest = http.expectOne(endpoint);
    const replacement = TestBed.runInInjectionContext(() =>
      createForm(signal<DntCaptchaForm>({ dntCaptchaInputText: '', dntCaptchaText: '', dntCaptchaToken: '' }))
    );
    fixture.componentRef.setInput('captchaForm', replacement);
    fixture.detectChanges();
    expect(oldRequest.cancelled).toBe(true);
    respond('replacement');
    await fixture.whenStable();
    expect(replacement.dntCaptchaToken().value()).toBe('replacement token');
    expect(form.dntCaptchaToken().value()).toBe('');
  });

  it('keeps labels and descriptions separate when two CAPTCHAs are rendered', async () => {
    respond();
    const secondForm = TestBed.runInInjectionContext(() =>
      createForm(signal<DntCaptchaForm>({ dntCaptchaInputText: '', dntCaptchaText: '', dntCaptchaToken: '' }))
    );
    const secondFixture = TestBed.createComponent(DntCaptchaComponent);
    secondFixture.componentRef.setInput('captchaForm', secondForm);
    secondFixture.autoDetectChanges();
    http.expectOne(endpoint).flush({
      dntCaptchaImgUrl: new URL('/second.png', document.baseURI).href,
      dntCaptchaTextValue: 'second text',
      dntCaptchaTokenValue: 'second token',
      dntCaptchaId: 'second'
    });
    await fixture.whenStable();
    await secondFixture.whenStable();
    const firstInput: HTMLInputElement = fixture.nativeElement.querySelector('input');
    const secondInput: HTMLInputElement = secondFixture.nativeElement.querySelector('input');
    expect(firstInput.id).not.toBe(secondInput.id);
    expect(firstInput.labels?.[0]).toBe(fixture.nativeElement.querySelector('label'));
    expect(secondInput.labels?.[0]).toBe(secondFixture.nativeElement.querySelector('label'));
    expect(firstInput.getAttribute('aria-describedby')).toBe(fixture.nativeElement.querySelector('[role="status"]').id);
    expect(secondInput.getAttribute('aria-describedby')).toBe(
      secondFixture.nativeElement.querySelector('[role="status"]').id
    );
  });

  it('supports an explicit stable input ID without starting another request', async () => {
    respond();
    fixture.componentRef.setInput('inputId', 'custom-captcha');
    await fixture.whenStable();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.id).toBe('custom-captcha');
    expect(input.labels?.[0]?.htmlFor).toBe('custom-captcha');
    expect(input.getAttribute('aria-describedby')).toBe('custom-captcha-status');
    http.expectNone(endpoint);
  });
});

describe('DntCaptchaComponent audio', () => {
  let fixture: ComponentFixture<DntCaptchaComponent>;
  let component: DntCaptchaComponent;
  let form: FieldTree<DntCaptchaForm>;
  let http: HttpTestingController;
  const endpoint = 'api/account/CreateAudioCaptchaParams';
  const audioUrl = '/my-custom-captcha/Audio?data=first';

  beforeEach(() => {
    vi.spyOn(HTMLMediaElement.prototype, 'pause').mockImplementation(() => undefined);
    vi.spyOn(HTMLMediaElement.prototype, 'load').mockImplementation(() => undefined);
    vi.spyOn(HTMLMediaElement.prototype, 'play').mockResolvedValue(undefined);
    vi.stubGlobal('URL', URL);
    Object.defineProperty(URL, 'createObjectURL', { configurable: true, value: vi.fn(() => 'blob:test-audio') });
    Object.defineProperty(URL, 'revokeObjectURL', { configurable: true, value: vi.fn() });
    TestBed.configureTestingModule({
      imports: [DntCaptchaComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(DntCaptchaComponent);
    component = fixture.componentInstance;
    form = TestBed.runInInjectionContext(() =>
      createForm(
        signal<DntCaptchaForm>({
          dntCaptchaInputText: '',
          dntCaptchaText: '',
          dntCaptchaToken: ''
        }),
        (path) => {
          readOnly(path.dntCaptchaInputText, { when: () => component.busy() || component.state().status !== 'ready' });
          required(path.dntCaptchaInputText);
          required(path.dntCaptchaText);
          required(path.dntCaptchaToken);
        }
      )
    );
    fixture.componentRef.setInput('captchaForm', form);
    fixture.componentRef.setInput('mode', 'audio');
    fixture.autoDetectChanges();
  });
  afterEach(() => {
    fixture.destroy();
    http.verify();
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
  });

  function respond(id = 'first') {
    fixture.detectChanges();
    const request = http.expectOne((request) => request.url === endpoint);
    request.flush({
      dntCaptchaImgUrl: '',
      dntCaptchaAudioUrl: '/my-custom-captcha/Audio?data=' + id,
      dntCaptchaId: id,
      dntCaptchaTextValue: 'text',
      dntCaptchaTokenValue: 'token'
    });
    return request;
  }
  async function play() {
    component.playCaptcha();
    http.expectOne(audioUrl).flush(new Blob(['wav'], { type: 'audio/wav' }));
    await fixture.whenStable();
  }

  it('renders audio parameters without autoplay and updates the signal form', async () => {
    respond();
    await fixture.whenStable();
    expect(component.captchaAudioUrl()).toBe(audioUrl);
    expect(fixture.nativeElement.querySelector('img')).toBeNull();
    expect(HTMLMediaElement.prototype.play).not.toHaveBeenCalled();
    expect(form().value()).toEqual({ dntCaptchaInputText: '', dntCaptchaText: 'text', dntCaptchaToken: 'token' });
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.labels?.[0]?.textContent).toContain('digits you hear');
    expect(input.getAttribute('aria-describedby')).toBe(component.statusId());
  });

  it('replays cached audio and permits stopping without another download', async () => {
    respond();
    await play();
    expect(component.isPlaying()).toBe(true);
    component.playCaptcha();
    expect(component.isPlaying()).toBe(false);
    component.playCaptcha();
    await fixture.whenStable();
    http.expectNone(audioUrl);
    expect(HTMLMediaElement.prototype.play).toHaveBeenCalledTimes(2);
    component.playbackEnded();
    expect(component.isPlaying()).toBe(false);
  });

  it('stops playback, revokes the URL, and invalidates the previous challenge on refresh', async () => {
    respond();
    await play();
    form.dntCaptchaInputText().value.set('1234');
    component.loadNewCaptcha();
    expect(component.isPlaying()).toBe(false);
    expect(form.dntCaptchaInputText().value()).toBe('');
    expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:test-audio');
    expect(HTMLMediaElement.prototype.pause).toHaveBeenCalled();
    expect(respond('second').request.params.get('previousAudioData')).toBe('first');
  });

  it('cancels an audio download and ignores late playback after refresh', async () => {
    respond();
    component.playCaptcha();
    const request = http.expectOne(audioUrl);
    component.loadNewCaptcha();
    expect(request.cancelled).toBe(true);
    expect(URL.createObjectURL).not.toHaveBeenCalled();
    expect(component.isAudioLoading()).toBe(false);
    respond('second');
    await fixture.whenStable();
    expect(component.isPlaying()).toBe(false);
  });

  it('ignores a pending play promise after refresh', async () => {
    let finish!: () => void;
    vi.mocked(HTMLMediaElement.prototype.play).mockReturnValueOnce(
      new Promise<void>((resolve) => {
        finish = resolve;
      })
    );
    respond();
    component.playCaptcha();
    http.expectOne(audioUrl).flush(new Blob(['wav']));
    component.loadNewCaptcha();
    finish();
    respond('second');
    await fixture.whenStable();
    expect(component.isPlaying()).toBe(false);
    expect(component.isAudioLoading()).toBe(false);
  });

  it('retries cached audio after the browser blocks playback', async () => {
    vi.mocked(HTMLMediaElement.prototype.play).mockRejectedValueOnce(new DOMException('Blocked', 'NotAllowedError'));
    respond();
    await play();
    expect(component.audioError()).toContain('Playback was blocked');
    component.playCaptcha();
    await fixture.whenStable();
    expect(component.isPlaying()).toBe(true);
    http.expectNone(audioUrl);
  });

  it('reports a media error and releases the failed blob', async () => {
    respond();
    await play();
    fixture.nativeElement.querySelector('audio').dispatchEvent(new Event('error'));
    await fixture.whenStable();
    expect(component.audioError()).toContain('Could not play');
    expect(component.isPlaying()).toBe(false);
    expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:test-audio');
  });

  it('clears an expired challenge so it cannot be submitted', async () => {
    respond();
    component.playCaptcha();
    http.expectOne(audioUrl).flush(new Blob(), { status: 400, statusText: 'Expired' });
    await fixture.whenStable();
    expect(component.state()).toMatchObject({ status: 'error' });
    expect(component.statusMessage()).toContain('expired');
    expect(form.dntCaptchaToken().value()).toBe('');
    expect(fixture.nativeElement.querySelector('input').readOnly).toBe(true);
    component.loadNewCaptcha();
    respond('second');
  });

  it('honors Retry-After for audio downloads without automatically retrying', async () => {
    respond();
    vi.useFakeTimers();
    try {
      component.playCaptcha();
      http
        .expectOne(audioUrl)
        .flush(new Blob(), { status: 429, statusText: 'Too Many Requests', headers: { 'Retry-After': '3' } });
      fixture.detectChanges();
      expect(component.audioCooldown.active()).toBe(true);
      expect(fixture.nativeElement.querySelector('.captcha-listen').disabled).toBe(true);
      component.playCaptcha();
      http.expectNone(audioUrl);
      await vi.advanceTimersByTimeAsync(3000);
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('.captcha-listen').disabled).toBe(false);
      http.expectNone(audioUrl);
      component.playCaptcha();
      http.expectOne(audioUrl).flush(new Blob(['wav']));
      await Promise.resolve();
    } finally {
      fixture.destroy();
      vi.useRealTimers();
    }
  });

  it('keeps creation cooldowns active after changing mode', async () => {
    vi.useFakeTimers();
    try {
      http
        .expectOne(endpoint)
        .flush({}, { status: 429, statusText: 'Too Many Requests', headers: { 'Retry-After': '3' } });
      fixture.componentRef.setInput('mode', 'image');
      fixture.detectChanges();
      http.expectNone('api/account/CreateDNTCaptchaParams');
      component.loadNewCaptcha();
      fixture.detectChanges();
      http.expectNone('api/account/CreateDNTCaptchaParams');
      await vi.advanceTimersByTimeAsync(3000);
      fixture.detectChanges();
      expect(component.state().status).toBe('error');
      component.loadNewCaptcha();
      fixture.detectChanges();
      http.expectOne('api/account/CreateDNTCaptchaParams').flush({
        dntCaptchaImgUrl: '/captcha.png',
        dntCaptchaId: 'image',
        dntCaptchaTextValue: 'text',
        dntCaptchaTokenValue: 'token'
      });
    } finally {
      fixture.destroy();
      vi.useRealTimers();
    }
  });

  it('ignores a stale audio response after changing mode before the effect runs', async () => {
    const old = http.expectOne(endpoint);
    fixture.componentRef.setInput('mode', 'image');
    old.flush({
      dntCaptchaImgUrl: '',
      dntCaptchaAudioUrl: audioUrl,
      dntCaptchaId: 'stale',
      dntCaptchaTextValue: 'text',
      dntCaptchaTokenValue: 'token'
    });
    expect(component.captchaAudioUrl()).toBe('');
    fixture.detectChanges();
    http.expectOne('api/account/CreateDNTCaptchaParams').flush({
      dntCaptchaImgUrl: '/captcha.png',
      dntCaptchaId: 'image',
      dntCaptchaTextValue: 'text',
      dntCaptchaTokenValue: 'token'
    });
    await fixture.whenStable();
    expect(component.captchaImageUrl()).toBe('/captcha.png');
  });

  it('disables playback while submitting and cleans up media on destruction', async () => {
    respond();
    fixture.componentRef.setInput('busy', true);
    component.playCaptcha();
    http.expectNone(audioUrl);
    fixture.componentRef.setInput('busy', false);
    await play();
    fixture.destroy();
    expect(component.isPlaying()).toBe(false);
    expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:test-audio');
  });
});
