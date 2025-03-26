import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './features/auth/views/login/login.component';
import { RegisterComponent } from './features/auth/views/register/register.component';

const routes: Routes = [
  {
    path: 'products',
    data: {
      root: {
        label: 'Home',
        url: '',
      },
    },
    loadChildren: () =>
      import('./features/products/product.module').then((m) => m.ProductModule),
  },
  {
    path: 'login',
    data: {
      breadcrumb: null,
    },
    component: LoginComponent,
  },
  {
    path: 'register',
    data: {
      breadcrumb: null,
    },
    component: RegisterComponent,
  },
  {
    path: '',
    data: {
      root: {
        label: 'Home',
        url: '',
      },
    },
    loadChildren: () =>
      import('./views/error-views.module').then((m) => m.ErrorViewsModule),
  },
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, {
      preloadingStrategy: PreloadAllModules,
      scrollPositionRestoration: 'enabled',
    }),
  ],
  exports: [RouterModule],
})
export class AppRoutingModule {}
