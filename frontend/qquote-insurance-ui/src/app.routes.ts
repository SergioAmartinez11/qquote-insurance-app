import { Routes } from '@angular/router';
import { AppLayout } from './app/layout/component/app.layout';
import { Landing } from './app/pages/landing/landing';
import { Notfound } from './app/pages/notfound/notfound';
import { authGuard } from './app/core/guards/auth.guard';

export const appRoutes: Routes = [
  {
    path: '',
    component: AppLayout,
    canActivate: [authGuard],
    children: [
      { path: 'pages', loadChildren: () => import('./app/pages/pages.routes') },
      {
        path: 'quotes',
        loadComponent: () => import('./app/pages/quotes/quotes').then((m) => m.Quotes),
      },
      {
        path: 'policies',
        loadComponent: () => import('./app/pages/policies/policies').then((m) => m.Policies),
      },
      {
        path: 'claims',
        loadComponent: () => import('./app/pages/claims/claims').then((m) => m.Claims),
      },
    ],
  },
  { path: 'landing', component: Landing },
  { path: 'notfound', component: Notfound },
  { path: 'auth', loadChildren: () => import('./app/pages/auth/auth.routes') },
  { path: '**', redirectTo: '/notfound' },
];
