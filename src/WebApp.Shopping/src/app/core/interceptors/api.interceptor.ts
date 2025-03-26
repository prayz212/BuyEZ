import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, catchError, switchMap, take, throwError } from 'rxjs';
import { AuthActions } from 'src/app/features/auth/store/auth.actions';
import { AuthSelectors } from 'src/app/features/auth/store/auth.selectors';
import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth.service';

export const apiInterceptor: HttpInterceptorFn = (
  req: HttpRequest<any>,
  next: HttpHandlerFn
): Observable<HttpEvent<any>> => {
  /*  Adding apis prefix  */
  const apiReq = req.clone({
    url: `${environment.apiBaseUrl}/${environment.apiVersion}/api/${req.url}`,
  });

  /*  Skip authentication for public endpoints  */
  if (isPublicEndpoint(req.url)) {
    return next(apiReq);
  }

  const store = inject(Store);

  /*  Add authentication headers  */
  let accessToken: string | undefined;
  store
    .select(AuthSelectors.selectAccessToken)
    .pipe(take(1)) // Why we need to take 1 here?
    .subscribe((token) => (accessToken = token));

  if (accessToken) {
    apiReq.headers.set('Authorization', `Bearer ${accessToken}`);
  }

  return next(apiReq).pipe(
    catchError((error) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        return handleUnauthorizedError(req, next);
      }

      return throwError(() => error);
    })
  );
};

const isPublicEndpoint = (url: string) => {
  /*  Add your public endpoints here  */
  const publicEndpoints = ['/catalog', '/identity'];
  return publicEndpoints.some((endpoint) => url.includes(endpoint));
};

// TODO: Improve the way we inject services
const handleUnauthorizedError = (
  req: HttpRequest<any>,
  next: HttpHandlerFn
) => {
  let refreshToken: string | undefined;
  inject(Store)
    .select(AuthSelectors.selectRefreshToken)
    .pipe(take(1)) // Why we need to take 1 here?
    .subscribe((token) => (refreshToken = token));

  /*  No refresh token available, redirect to login page  */
  if (!refreshToken) {
    redirectToLoginPage();
    return throwError(() => new Error('No refresh token available'));
  }

  return inject(AuthService)
    .refreshToken(refreshToken)
    .pipe(
      // Why we need to switchMap here?
      switchMap((response) => {
        inject(Store).dispatch(
          AuthActions.loginSuccess({
            token: {
              accessToken: response.accessToken,
              refreshToken: response.refreshToken,
              expiresAt: new Date(Date.now() + response.expiresIn),
            },
          })
        );

        // Why we need to use refreshTokenSubject here?
        // this.refreshTokenSubject.next(response.accessToken);

        return next(
          req.clone({
            setHeaders: { Authorization: `Bearer ${response.accessToken}` },
          })
        );
      }),
      catchError((err) => {
        inject(Store).dispatch(AuthActions.refreshTokenFailure({ error: err }));
        redirectToLoginPage();

        return throwError(() => err);
      })
    );
};

const redirectToLoginPage = () => {
  // inject(AuthService).clearTokens();
  // inject(Store).dispatch(AuthActions.logout());
  inject(Router).navigate(['/login']);
};
