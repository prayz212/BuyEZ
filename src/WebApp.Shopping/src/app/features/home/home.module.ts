import { NgModule, Optional, SkipSelf } from '@angular/core';
import { EnsureModuleLoadedOnceGuard } from 'src/app/core/guards/ensure-module-loaded-once.guard';
import { HomeRoutingModule } from './home-routing.module';
import { HomeComponent } from './views/home/home.view';
import { CommonModule } from '@angular/common';

@NgModule({
  declarations: [HomeComponent],
  imports: [HomeRoutingModule, CommonModule],
})
export class HomeModule extends EnsureModuleLoadedOnceGuard<HomeModule> {
  constructor(@Optional() @SkipSelf() parentModule: HomeModule) {
    super(parentModule);
  }
}
