import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { InternalServerErrorComponent } from './internal-server-error/internal-server-error.view';
import { NotFoundComponent } from './not-found/not-found.view';

const routes: Routes = [
  {
    path: 'unexpected-error',
    data: {
      root: {
        label: 'Home',
        url: '',
      },
      breadcrumb: {
        label: 'Internal Server Error',
        url: 'unexpected-error',
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
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ErrorRoutingModule {}
