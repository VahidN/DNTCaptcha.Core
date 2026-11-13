import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./login/component/login.component').then((module) => module.LoginComponent)
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
