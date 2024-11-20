import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { productResolver } from './product-resolver';
import { ProductDetailsComponent, ProductListComponent } from './views';

const routes: Routes = [
  {
    path: '',
    data: {
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
        resolve: { dataSource: productResolver },
        component: ProductDetailsComponent,
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ProductRoutingModule {}
