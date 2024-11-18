import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './features/home/views/home/home.view';
import { catalogResolver } from './features/products/catalog-resolver';
import { ProductDetailsComponent } from './features/products/views/product-details/product-details.view';
import { ProductListComponent } from './features/products/views/product-list/product-list.view';
import { InternalServerErrorComponent } from './views/internal-server-error/internal-server-error.view';
import { NotFoundComponent } from './views/not-found/not-found.view';

const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    data: {
      root: {
        label: 'Home',
        url: '',
      },
    },
    component: HomeComponent,
  },
  {
    path: 'products',
    data: {
      root: {
        label: 'Home',
        url: '',
      },
      breadcrumb: {
        label: 'Products',
        url: 'products',
      },
    },
    children: [
      {
        path: '',
        component: ProductListComponent,
        data: {
          breadcrumb: null,
        },
      },
      {
        path: ':id',
        data: {
          breadcrumb: null,
        },
        resolve: { dataSource: catalogResolver },
        component: ProductDetailsComponent,
      },
    ],
  },
  {
    path: 'error',
    data: {
      root: {
        label: 'Home',
        url: '',
      },
      breadcrumb: {
        label: 'Internal Server Error',
        url: 'error',
      },
    },
    component: InternalServerErrorComponent,
  },
  {
    path: 'not-found',
    data: {
      root: {
        label: 'Home',
        url: '',
      },
      breadcrumb: {
        label: 'Not Found',
        url: 'not-found',
      },
    },
    component: NotFoundComponent,
  },
  {
    path: '**',
    redirectTo: '/not-found',
  },
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, {
      scrollPositionRestoration: 'enabled',
    }),
  ],
  exports: [RouterModule],
})
export class AppRoutingModule {}
