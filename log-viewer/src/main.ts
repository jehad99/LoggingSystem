import { bootstrapApplication } from '@angular/platform-browser';
import { Component } from '@angular/core';
import { LogViewerComponent } from './app/components/log-viewer/log-viewer.component';
import { provideHttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [LogViewerComponent],
  template: '<app-log-viewer></app-log-viewer>'
})
export class App {}

bootstrapApplication(App, {
  providers: [
    provideHttpClient()
  ]
});