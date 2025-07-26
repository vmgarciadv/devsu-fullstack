import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface NotificationState {
  isOpen: boolean;
  type: 'success' | 'error';
  title: string;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationState = new BehaviorSubject<NotificationState>({
    isOpen: false,
    type: 'success',
    title: '',
    message: ''
  });

  notification$: Observable<NotificationState> = this.notificationState.asObservable();

  showSuccess(title: string, message: string = ''): void {
    this.notificationState.next({
      isOpen: true,
      type: 'success',
      title,
      message
    });
  }

  showError(title: string, message: string = ''): void {
    this.notificationState.next({
      isOpen: true,
      type: 'error',
      title,
      message
    });
  }

  close(): void {
    this.notificationState.next({
      ...this.notificationState.value,
      isOpen: false
    });
  }
}