import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { Observable, of, throwError } from "rxjs";
import { catchError, delay, mergeMap, retryWhen, take } from "rxjs/operators";

import { AuthTokenType } from "./../models/auth-token-type";
import { TokenStoreService } from "./token-store.service";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  private delayBetweenRetriesMs = 1000;
  private numberOfRetries = 3;

  constructor(
    private tokenStoreService: TokenStoreService,
    private router: Router) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const accessToken = this.tokenStoreService.getRawAuthToken(AuthTokenType.AccessToken);
    if (accessToken) {
      request = request.clone({
        headers: request.headers.set("Authorization", `Bearer ${accessToken}`)
      });
      return next.handle(request).pipe(
        retryWhen(errors => errors.pipe(
          mergeMap((error: HttpErrorResponse, retryAttempt: number) => {
            if (retryAttempt === this.numberOfRetries - 1) {
              console.log(`HTTP call '${request.method} ${request.url}' failed after ${this.numberOfRetries} retries.`);
              return throwError(error); // no retry
            }

            switch (error.status) {
              case 400:
              case 404:
                return throwError(error); // no retry
            }

            return of(error); // retry
          }),
          delay(this.delayBetweenRetriesMs),
          take(this.numberOfRetries)
        )),
        catchError((error: any, caught: Observable<HttpEvent<any>>) => {
          console.error({ error, caught });
          if (error.status === 401 || error.status === 403) {
            this.router.navigate(["/accessDenied"]);
          }
          return throwError(error);
        })
      );
    } else {
      // login page
      return next.handle(request);
    }
  }


}
