import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { NgModule, Optional, SkipSelf } from '@angular/core';
import { EnsureModuleLoadedOnceGuard } from './guards/ensure-module-loaded-once.guard';
import {
  apiInterceptor,
  errorInterceptor,
  loadingInterceptor,
} from './interceptors';
import { BreadcrumbService, CatalogService, LoadingService } from './services';

@NgModule({
  providers: [
    CatalogService,
    LoadingService,
    BreadcrumbService,
    provideHttpClient(
      withInterceptors([apiInterceptor, errorInterceptor, loadingInterceptor])
    ),
  ],
})
export class CoreModule extends EnsureModuleLoadedOnceGuard<CoreModule> {
  constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
    super(parentModule);
  }
}
