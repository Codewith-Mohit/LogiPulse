// Zone.js should be imported with correct casing to match the package name
import 'zone.js';
import { bootstrapApplication } from '@angular/platform-browser';
import { applicationConfig } from './app/app.config';
import { AppComponent } from './app/app.component';

bootstrapApplication(AppComponent, applicationConfig)
  .catch((err) => console.error(err));
