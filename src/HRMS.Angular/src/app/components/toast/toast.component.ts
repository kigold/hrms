import { Component } from '@angular/core';
import { ToastType } from '../../models/toast';
import { ToastService } from '../../services/toast.service';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [ CommonModule],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.css',
  animations: [
    trigger('openClose', [
      state(
        'closed',
        style({
          visibility: 'hidden',
          right: '-400px',
        })
      ),
      state(
        'open',
        style({
          right: '40px',
        })
      ),
      transition('open <=> closed', [animate('0.5s ease-in-out')]),
    ]),
  ],
})

export class ToastComponent {
  ToastTyped? = ToastType;
  progressBar: number = 0;
  progressInterval?: any;
  toastTimeout: number = 50;

  constructor(public toastService: ToastService){
    this.toastService.open.subscribe((data) => {
      if (data.show){
        this.countDown();
      }
    })
  }

  countDown() {
    this.progressBar = this.toastService.data.progressWidth ?? 0;
    this.progressInterval = setInterval(() => {
      const width = this.progressBar;

      if (width <= 0) {
        this.toastService.hide();
        clearInterval(this.progressInterval);
        return;
      }

      this.toastService.data.progressWidth = width - 2;
      this.progressBar = this.toastService.data.progressWidth;
    }, this.toastTimeout);
  }

  stopCountDown() {
    clearInterval(this.progressInterval);
  }

  close(){
    this.toastService.hide();
  }
}
