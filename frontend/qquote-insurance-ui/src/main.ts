import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app';
import { appConfig } from './app.config';
import { inject } from '@vercel/analytics';

bootstrapApplication(AppComponent, appConfig)
    .catch(err => console.error(err));

// Initialize Vercel Analytics
inject();
