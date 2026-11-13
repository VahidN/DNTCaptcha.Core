import { computed, DestroyRef, inject, signal } from '@angular/core';

/** Creates an independent cooldown whose timer is cleaned up with its component. */
export function createRetryAfterCooldown() {
  const destroyRef = inject(DestroyRef);
  const remainingSeconds = signal(0);
  let timer: ReturnType<typeof setInterval> | undefined;

  function clear(): void {
    clearInterval(timer);
    timer = undefined;
    remainingSeconds.set(0);
  }

  destroyRef.onDestroy(clear);

  function start(retryAfter: string | null): void {
    clear();
    if (destroyRef.destroyed || !retryAfter?.trim()) return;
    const value = retryAfter.trim();
    const now = Date.now();
    const deadline = /^\d+$/.test(value)
      ? now + Number(value) * 1_000
      : /^[a-z]{3,9}[, ]/i.test(value)
        ? Date.parse(value)
        : NaN;
    if (!Number.isFinite(deadline) || deadline <= now) return;

    function update(): void {
      remainingSeconds.set(Math.max(0, Math.ceil((deadline - Date.now()) / 1_000)));
      if (remainingSeconds() === 0) clear();
    }

    update();
    timer = setInterval(update, 1_000);
  }

  return {
    remainingSeconds: remainingSeconds.asReadonly(),
    active: computed(() => remainingSeconds() > 0),
    start
  };
}
