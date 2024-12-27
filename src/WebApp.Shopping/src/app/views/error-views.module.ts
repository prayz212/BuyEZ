import { NgModule, Optional, SkipSelf } from '@angular/core';
import { EnsureModuleLoadedOnceGuard } from '../core/guards/ensure-module-loaded-once.guard';
import { SharedModule } from '../shared/shared.module';
import { ErrorRoutingModule } from './error-routing.module';
import { InternalServerErrorComponent } from './internal-server-error/internal-server-error.view';
import { NotFoundComponent } from './not-found/not-found.view';

@NgModule({
  declarations: [InternalServerErrorComponent, NotFoundComponent],
  imports: [SharedModule, ErrorRoutingModule],
})
export class ErrorViewsModule extends EnsureModuleLoadedOnceGuard<ErrorViewsModule> {
  constructor(@Optional() @SkipSelf() parentModule: ErrorViewsModule) {
    super(parentModule);
  }
}
