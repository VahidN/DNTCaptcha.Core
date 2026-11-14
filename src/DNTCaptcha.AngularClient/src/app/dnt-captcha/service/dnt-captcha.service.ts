import { inject, Service } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { DntCaptchaParams } from '../interfaces/dnt-captcha-params.interface';
import { parseDntCaptchaParams } from '../validation/parse-dnt-captcha-params';

@Service()
export class DntCaptchaService {
  private readonly http = inject(HttpClient);

  getDntCaptchaParams(mode: 'image' | 'audio' = 'image', previousAudioUrl = ''): Observable<DntCaptchaParams> {
    const endpoint = mode === 'audio' ? 'CreateAudioCaptchaParams' : 'CreateDNTCaptchaParams';
    const params: Record<string, string> = {};
    if (mode === 'audio' && previousAudioUrl) {
      const data = new URL(previousAudioUrl, 'https://captcha.invalid/').searchParams.get('data');
      if (data) params['previousAudioData'] = data;
    }
    return this.http
      .get<unknown>('api/account/' + endpoint, { params })
      .pipe(map((value) => parseDntCaptchaParams(value, mode)));
  }

  getAudio(url: string): Observable<Blob> {
    return this.http.get(url, { responseType: 'blob' });
  }
}
