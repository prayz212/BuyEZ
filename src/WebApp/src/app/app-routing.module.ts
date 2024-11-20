import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';

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
