import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { lastValueFrom } from 'rxjs';

export const headerInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  if (req.url.includes("connect/token")){ //Skip if Login request
    return next(req);
  }
    
  let token;
  if (authService.isTokenExpired()){
    lastValueFrom(authService.refreshAccessToken()).then((tokenResponse) =>
      {
        authService.storeAuthInLocalStorage(tokenResponse);
        token = tokenResponse.access_token;
      }
    );
  }
  else{
    token = authService.getToken();
  }

  const authReq = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(authReq);
};