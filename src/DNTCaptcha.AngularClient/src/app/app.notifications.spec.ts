import { describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ToastrService } from '@openng/ngx-toastr';
import { AppComponent } from './app.component';
import { appConfig } from './app.config';

describe('Zoneless notifications', () => {
  it('renders and automatically dismisses a real toast created asynchronously', async () => {
    expect('Zone' in globalThis).toBe(false);
    TestBed.configureTestingModule({ imports: [AppComponent], providers: appConfig.providers });
    const fixture = TestBed.createComponent(AppComponent);
    fixture.autoDetectChanges();
    const toastr = TestBed.inject(ToastrService);
    try {
      await new Promise<void>((resolve) => {
        setTimeout(() => {
          toastr.success('Login completed', 'Success', { timeOut: 500, easeTime: 0 });
          resolve();
        });
      });
      await vi.waitFor(() => {
        const toast = document.querySelector<HTMLElement>('.ngx-toastr');
        expect(toast?.querySelector('.toast-message')?.textContent).toContain('Login completed');
        expect(toast?.style.display).not.toBe('none');
      });
      await vi.waitFor(() => expect(document.querySelector('.ngx-toastr')).toBeNull());
    } finally {
      toastr.clear();
    }
  });
});
