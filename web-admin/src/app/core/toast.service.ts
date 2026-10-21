import { Injectable, signal } from '@angular/core';

// Shows small messages (success or error) and the "server is waking up" banner.
@Injectable({ providedIn: 'root' })
export class ToastService {
  message = signal<string | null>(null);
  kind = signal<'success' | 'error'>('success');
  slow = signal(false);

  private timer: ReturnType<typeof setTimeout> | undefined;

  success(text: string): void {
    this.show(text, 'success');
  }

  error(text: string): void {
    this.show(text, 'error');
  }

  private show(text: string, kind: 'success' | 'error'): void {
    this.message.set(text);
    this.kind.set(kind);
    clearTimeout(this.timer);
    this.timer = setTimeout(() => this.message.set(null), 4000);
  }
}
