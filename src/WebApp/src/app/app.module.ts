import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import {
  HttpClientModule,
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { apiInterceptor } from './core/interceptors/api.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { loadingInterceptor } from './core/interceptors/loading.interceptor';
import { HomeComponent } from './features/home/views/home/home.view';
import { FilterComponent } from './features/products/components/filter/filter.component';
import { ImageSliderComponent } from './features/products/components/image-slider/image-slider.component';
import { ProductItemComponent } from './features/products/components/product-item/product-item.component';
import { ProductSliderComponent } from './features/products/components/product-slider/product-slider.component';
import { PromotionComponent } from './features/products/components/promotion/promotion.component';
import { ProductDetailsComponent } from './features/products/views/product-details/product-details.view';
import { ProductListComponent } from './features/products/views/product-list/product-list.view';
import { FooterComponent } from './layout/footer/footer.component';
import { HeaderComponent } from './layout/header/header.component';
import { LoadingIndicatorComponent } from './layout/loading-indicator/loading-indicator.component';
import { BreadcrumbComponent } from './shared/components/breadcrumb/breadcrumb.component';
import { TabsComponent } from './shared/components/horizontal-tabs/horizontal-tabs.component';
import { TabContentComponent } from './shared/components/horizontal-tabs/tab-content/tab-content.component';
import { OutlineButtonComponent } from './shared/components/outline-button/outline-button.component';
import { PaginationComponent } from './shared/components/pagination/pagination.component';
import { SelectComponent } from './shared/components/select/select.component';
import { SolidButtonComponent } from './shared/components/solid-button/solid-button.component';
import { TextPipe } from './shared/pipes/text.pipe';
import { InternalServerErrorComponent } from './views/internal-server-error/internal-server-error.view';
import { NotFoundComponent } from './views/not-found/not-found.view';

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    FooterComponent,
    HomeComponent,
    ProductListComponent,
    ProductDetailsComponent,
    OutlineButtonComponent,
    SelectComponent,
    FilterComponent,
    ProductItemComponent,
    PaginationComponent,
    BreadcrumbComponent,
    ImageSliderComponent,
    PromotionComponent,
    SolidButtonComponent,
    TabsComponent,
    TabContentComponent,
    ProductSliderComponent,
    TextPipe,
    NotFoundComponent,
    InternalServerErrorComponent,
    LoadingIndicatorComponent,
  ],
  imports: [BrowserModule, AppRoutingModule, FormsModule, HttpClientModule],
  providers: [
    provideHttpClient(
      withInterceptors([apiInterceptor, errorInterceptor, loadingInterceptor])
    ),
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
