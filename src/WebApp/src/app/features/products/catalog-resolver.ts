import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, ResolveFn } from '@angular/router';
import { catchError, map, Observable, of, throwError } from 'rxjs';
import { CatalogService } from 'src/app/core/services/catalog.service';
import { Breadcrumb } from 'src/app/shared/types/breadcrumb';
import { ProductDetail } from './models/product.model';

const catalogResolver: ResolveFn<
  Observable<{
    data: ProductDetail | null;
    breadcrumb: Breadcrumb | null;
  }>
> = (route: ActivatedRouteSnapshot, _) => {
  return inject(CatalogService)
    .getProductById(route.paramMap.get('id')!)
    .pipe(
      map((product) => ({
        data: product,
        breadcrumb: {
          label: product.name,
          url: product.id,
        },
      })),
      catchError((error: HttpErrorResponse) => {
        console.error(`Http Error Response: ${error.message}`);

        if (error && error.status === HttpStatusCode.NotFound) {
          return of({
            data: null,
            breadcrumb: null,
          });
        }

        return throwError(() => new Error(error.message));
      })
    );
};

export { catalogResolver };
