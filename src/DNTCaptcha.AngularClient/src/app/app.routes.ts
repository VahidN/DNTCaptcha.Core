import { Routes } from '@angular/router';
import { LoginComponent } from './login/component/login.component';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
