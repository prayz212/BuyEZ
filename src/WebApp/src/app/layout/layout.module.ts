import { NgModule, Optional, SkipSelf } from '@angular/core';
import { EnsureModuleLoadedOnceGuard } from '../core/guards/ensure-module-loaded-once.guard';
import { SharedModule } from '../shared/shared.module';
import { FooterComponent } from './footer/footer.component';
import { HeaderComponent } from './header/header.component';
import { LoadingIndicatorComponent } from './loading-indicator/loading-indicator.component';

@NgModule({
  declarations: [HeaderComponent, FooterComponent, LoadingIndicatorComponent],
  imports: [SharedModule],
  exports: [HeaderComponent, FooterComponent, LoadingIndicatorComponent],
})
export class LayoutModule extends EnsureModuleLoadedOnceGuard<LayoutModule> {
  constructor(@Optional() @SkipSelf() parentModule: LayoutModule) {
    super(parentModule);
  }
}
