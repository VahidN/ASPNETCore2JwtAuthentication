import { HttpClient, HttpErrorResponse, HttpHeaders } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { ErrorObservable } from "rxjs/observable/ErrorObservable";
import { timer } from "rxjs/observable/timer";
import { catchError, finalize, map } from "rxjs/operators";
import { Subscription } from "rxjs/Subscription";

import { AuthTokenType } from "./../models/auth-token-type";
import { APP_CONFIG, IAppConfig } from "./app.config";
import { TokenStoreService } from "./token-store.service";

@Injectable()
export class RefreshTokenService {

  private refreshTokenSubscription: Subscription | null = null;

  constructor(
    private tokenStoreService: TokenStoreService,
    @Inject(APP_CONFIG) private appConfig: IAppConfig,
    private http: HttpClient) { }

  scheduleRefreshToken(isAuthUserLoggedIn: boolean) {
    this.unscheduleRefreshToken();

    if (!isAuthUserLoggedIn) {
      return;
    }

    const expDateUtc = this.tokenStoreService.getAccessTokenExpirationDateUtc();
    if (!expDateUtc) {
      throw new Error("This access token has not the `exp` property.");
    }
    const expiresAtUtc = expDateUtc.valueOf();
    const nowUtc = new Date().valueOf();
    const initialDelay = Math.max(1, expiresAtUtc - nowUtc);
    console.log("Initial scheduleRefreshToken Delay(ms)", initialDelay);
    const timerSource$ = timer(initialDelay);
    this.refreshTokenSubscription = timerSource$.subscribe(() => {
      this.refreshToken(isAuthUserLoggedIn);
    });
  }

  unscheduleRefreshToken() {
    if (this.refreshTokenSubscription) {
      this.refreshTokenSubscription.unsubscribe();
    }
  }

  private refreshToken(isAuthUserLoggedIn: boolean) {
    const headers = new HttpHeaders({ "Content-Type": "application/json" });
    const model = { refreshToken: this.tokenStoreService.getRawAuthToken(AuthTokenType.RefreshToken) };
    return this.http
      .post(`${this.appConfig.apiEndpoint}/${this.appConfig.refreshTokenPath}`, model, { headers: headers })
      .pipe(
        map(response => response || {}),
        catchError((error: HttpErrorResponse) => ErrorObservable.create(error)),
        finalize(() => {
          this.scheduleRefreshToken(isAuthUserLoggedIn);
        })
      )
      .subscribe(result => {
        console.log("RefreshToken Result", result);
        this.tokenStoreService.storeLoginSession(result);
      });
  }
}
