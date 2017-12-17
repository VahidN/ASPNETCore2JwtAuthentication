import { Injectable, Injector } from "@angular/core";
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest } from "@angular/common/http";
import { Observable } from "rxjs/Observable";
import { Router } from "@angular/router";

import { AuthService, AuthTokenType } from "./auth.service";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(
    private injector: Injector,
    private router: Router) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const authService = this.injector.get(AuthService);
    const accessToken = authService.getRawAuthToken(AuthTokenType.AccessToken);
    if (accessToken) {
      request = request.clone({
        headers: request.headers.set("Authorization", `Bearer ${accessToken}`)
      });
      return next.handle(request)
        .catch((error: any, caught: Observable<HttpEvent<any>>) => {
          if (error.status === 401 || error.status === 403) {
            this.router.navigate(["/accessDenied"]);
          }
          return Observable.throw(error);
        });
    } else {
      // login page
      return next.handle(request);
    }
  }
}
