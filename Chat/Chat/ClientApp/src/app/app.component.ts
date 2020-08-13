import { Component } from '@angular/core';
import { SignalRService } from './services/signalR';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  constructor(sr: SignalRService){}
  title = 'app';
}
