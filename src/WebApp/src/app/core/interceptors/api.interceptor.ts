import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const apiReq = req.clone({
    url: `${environment.apiBaseUrl}/api/${environment.apiVersion}/${req.url}`,
  });
  return next(apiReq);
};
