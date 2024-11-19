import { NgModule, Optional, SkipSelf } from '@angular/core';
import { EnsureModuleLoadedOnceGuard } from 'src/app/core/guards/ensure-module-loaded-once.guard';
import { SharedModule } from 'src/app/shared/shared.module';
import {
  FilterComponent,
  ImageSliderComponent,
  ProductItemComponent,
  ProductSliderComponent,
  PromotionComponent,
} from './components';
import { ProductRoutingModule } from './product-routing.module';
import { ProductDetailsComponent, ProductListComponent } from './views';

@NgModule({
  declarations: [
    ProductListComponent,
    ProductDetailsComponent,
    FilterComponent,
    ProductItemComponent,
    ImageSliderComponent,
    PromotionComponent,
    ProductSliderComponent,
  ],
  imports: [SharedModule, ProductRoutingModule],
})
export class ProductModule extends EnsureModuleLoadedOnceGuard<ProductModule> {
  constructor(@Optional() @SkipSelf() parentModule: ProductModule) {
    super(parentModule);
  }
}
