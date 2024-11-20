import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './views/home/home.view';

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
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class HomeRoutingModule {}
