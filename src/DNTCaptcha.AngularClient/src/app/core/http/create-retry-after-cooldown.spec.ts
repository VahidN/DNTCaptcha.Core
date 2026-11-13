import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { createRetryAfterCooldown } from './create-retry-after-cooldown';

describe('Retry-After cooldown', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-11-13T12:00:00Z'));
    TestBed.configureTestingModule({});
  });

  afterEach(() => {
    TestBed.resetTestingModule();
    vi.useRealTimers();
  });

  it('counts down seconds without permitting an early retry', () => {
    const cooldown = TestBed.runInInjectionContext(createRetryAfterCooldown);
    cooldown.start(' 3 ');
    expect(cooldown.active()).toBe(true);
    expect(cooldown.remainingSeconds()).toBe(3);
    vi.advanceTimersByTime(2_999);
    expect(cooldown.active()).toBe(true);
    expect(cooldown.remainingSeconds()).toBe(1);
    vi.advanceTimersByTime(1);
    expect(cooldown.active()).toBe(false);
    expect(cooldown.remainingSeconds()).toBe(0);
    expect(vi.getTimerCount()).toBe(0);
  });

  it('supports HTTP dates and rounds up a partial second', () => {
    vi.setSystemTime(new Date('2026-11-13T12:00:00.500Z'));
    const cooldown = TestBed.runInInjectionContext(createRetryAfterCooldown);
    cooldown.start('Fri, 13 Nov 2026 12:00:03 GMT');
    expect(cooldown.remainingSeconds()).toBe(3);
    vi.advanceTimersByTime(2_000);
    expect(cooldown.active()).toBe(true);
    vi.advanceTimersByTime(1_000);
    expect(cooldown.active()).toBe(false);
  });

  it.each([null, '', 'invalid', '-1', '1.5', '0', '1e300', 'Fri, 13 Nov 2026 11:59:59 GMT'])(
    'does not block retries for an unusable header: %s',
    (header) => {
      const cooldown = TestBed.runInInjectionContext(createRetryAfterCooldown);
      cooldown.start(header);
      expect(cooldown.active()).toBe(false);
      expect(vi.getTimerCount()).toBe(0);
    }
  );

  it('replaces a previous deadline and disposes its timer on destruction', () => {
    const cooldown = TestBed.runInInjectionContext(createRetryAfterCooldown);
    cooldown.start('30');
    cooldown.start('5');
    expect(vi.getTimerCount()).toBe(1);
    expect(cooldown.remainingSeconds()).toBe(5);
    TestBed.resetTestingModule();
    expect(vi.getTimerCount()).toBe(0);
    cooldown.start('30');
    expect(vi.getTimerCount()).toBe(0);
  });

  it('uses the deadline rather than accumulating timer ticks', () => {
    const cooldown = TestBed.runInInjectionContext(createRetryAfterCooldown);
    cooldown.start('30');
    vi.setSystemTime(new Date('2026-11-13T12:01:00Z'));
    vi.advanceTimersByTime(1_000);
    expect(cooldown.active()).toBe(false);
  });
});
