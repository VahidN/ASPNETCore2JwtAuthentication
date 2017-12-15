import { Injectable, Inject } from "@angular/core";
import { Router } from "@angular/router";
import { HttpClient, HttpHeaders, HttpErrorResponse, HttpResponse } from "@angular/common/http";
import { BehaviorSubject } from "rxjs/BehaviorSubject";
import { Observable } from "rxjs/Observable";
import { Subscription } from "rxjs/Subscription";

import * as jwt_decode from "jwt-decode";

import { APP_CONFIG, IAppConfig } from "./app.config";
import { BrowserStorageService } from "./browser-storage.service";
import { Credentials } from "./../models/credentials";
import { AuthUser } from "./../models/auth-user";

export enum AuthTokenType {
  AccessToken,
  RefreshToken
}

@Injectable()
export class AuthService {

  private rememberMeToken = "rememberMe_token";

  private authStatusSource = new BehaviorSubject<boolean>(false);
  authStatus$ = this.authStatusSource.asObservable();

  private refreshTokenSubscription: Subscription;

  constructor(
    @Inject(APP_CONFIG) private appConfig: IAppConfig,
    private browserStorageService: BrowserStorageService,
    private http: HttpClient,
    private router: Router
  ) {
    this.updateStatusOnPageRefresh();
    this.scheduleRefreshToken();
  }

  login(credentials: Credentials): Observable<boolean> {
    const headers = new HttpHeaders({ "Content-Type": "application/json" });
    return this.http
      .post(`${this.appConfig.apiEndpoint}/${this.appConfig.loginPath}`, credentials, { headers: headers })
      .map((response: any) => {
        this.browserStorageService.setLocal(this.rememberMeToken, credentials.rememberMe);
        if (!response) {
          this.authStatusSource.next(false);
          return false;
        }
        this.setLoginSession(response);
        this.scheduleRefreshToken();
        this.authStatusSource.next(true);
        return true;
      })
      .catch((error: HttpErrorResponse) => Observable.throw(error));
  }

  logout(navigateToHome: boolean): void {
    this.http
      .get(`${this.appConfig.apiEndpoint}/${this.appConfig.logoutPath}`)
      .map(response => response || {})
      .catch((error: HttpErrorResponse) => Observable.throw(error))
      .subscribe(result => {
        this.deleteAuthTokens();
        this.unscheduleRefreshToken();
        this.authStatusSource.next(false);
        if (navigateToHome) {
          this.router.navigate(["/"]);
        }
      });
  }

  isLoggedIn(): boolean {
    const accessToken = this.getRawAuthToken(AuthTokenType.AccessToken);
    const refreshToken = this.getRawAuthToken(AuthTokenType.RefreshToken);
    const hasTokens = !this.isEmptyString(accessToken) && !this.isEmptyString(refreshToken);
    return hasTokens && !this.isAccessTokenTokenExpired();
  }

  rememberMe(): boolean {
    return this.browserStorageService.getLocal(this.rememberMeToken) === true;
  }

  getRawAuthToken(tokenType: AuthTokenType): string {
    if (this.rememberMe()) {
      return this.browserStorageService.getLocal(AuthTokenType[tokenType]);
    } else {
      return this.browserStorageService.getSession(AuthTokenType[tokenType]);
    }
  }

  getDecodedAccessToken(): any {
    return jwt_decode(this.getRawAuthToken(AuthTokenType.AccessToken));
  }

  getDisplayName(): string {
    return this.getDecodedAccessToken().DisplayName;
  }

  getAuthUser(): AuthUser {
    if (!this.isLoggedIn()) {
      return null;
    }

    const decodedToken = this.getDecodedAccessToken();
    let roles = decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
    if (roles) {
      roles = roles.map(role => role.toLowerCase());
    }
    return Object.freeze({
      userId: decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
      userName: decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
      displayName: decodedToken["DisplayName"],
      roles: roles
    });
  }

  isAuthUserInRoles(requiredRoles: string[]): boolean {
    const user = this.getAuthUser();
    if (!user || !user.roles) {
      return false;
    }
    return requiredRoles.some(requiredRole => user.roles.indexOf(requiredRole.toLowerCase()) >= 0);
  }

  isAuthUserInRole(requiredRole: string): boolean {
    return this.isAuthUserInRoles([requiredRole]);
  }

  getAccessTokenExpirationDateUtc(): Date {
    const decoded = this.getDecodedAccessToken();
    if (decoded.exp === undefined) {
      return null;
    }
    const date = new Date(0); // The 0 sets the date to the epoch
    date.setUTCSeconds(decoded.exp);
    return date;
  }

  isAccessTokenTokenExpired(): boolean {
    const expirationDateUtc = this.getAccessTokenExpirationDateUtc();
    if (!expirationDateUtc) {
      return true;
    }
    return !(expirationDateUtc.valueOf() > new Date().valueOf());
  }

  deleteAuthTokens() {
    if (this.rememberMe()) {
      this.browserStorageService.removeLocal(AuthTokenType[AuthTokenType.AccessToken]);
      this.browserStorageService.removeLocal(AuthTokenType[AuthTokenType.RefreshToken]);
    } else {
      this.browserStorageService.removeSession(AuthTokenType[AuthTokenType.AccessToken]);
      this.browserStorageService.removeSession(AuthTokenType[AuthTokenType.RefreshToken]);
    }
    this.browserStorageService.removeLocal(this.rememberMeToken);
  }

  scheduleRefreshToken() {
    if (!this.isLoggedIn()) {
      return;
    }

    this.unscheduleRefreshToken();

    const expiresAtUtc = this.getAccessTokenExpirationDateUtc().valueOf();
    const nowUtc = new Date().valueOf();
    const initialDelay = Math.max(1, expiresAtUtc - nowUtc);
    console.log("Initial scheduleRefreshToken Delay(ms)", initialDelay);
    const timerSource$ = Observable.timer(initialDelay);
    this.refreshTokenSubscription = timerSource$.subscribe(() => {
      this.refreshToken();
    });
  }

  unscheduleRefreshToken() {
    if (this.refreshTokenSubscription) {
      this.refreshTokenSubscription.unsubscribe();
    }
  }

  refreshToken() {
    const headers = new HttpHeaders({ "Content-Type": "application/json" });
    const model = { refreshToken: this.getRawAuthToken(AuthTokenType.RefreshToken) };
    return this.http
      .post(`${this.appConfig.apiEndpoint}/${this.appConfig.refreshTokenPath}`, model, { headers: headers })
      .finally(() => {
        this.scheduleRefreshToken();
      })
      .map(response => response || {})
      .catch((error: HttpErrorResponse) => Observable.throw(error))
      .subscribe(result => {
        console.log("RefreshToken Result", result);
        this.setLoginSession(result);
      });
  }

  private updateStatusOnPageRefresh(): void {
    this.authStatusSource.next(this.isLoggedIn());
  }

  private setLoginSession(response: any): void {
    this.setToken(AuthTokenType.AccessToken, response[this.appConfig.accessTokenObjectKey]);
    this.setToken(AuthTokenType.RefreshToken, response[this.appConfig.refreshTokenObjectKey]);
    console.log("Logged-in user info", this.getAuthUser());
  }

  private setToken(tokenType: AuthTokenType, tokenValue: string): void {
    if (this.rememberMe()) {
      this.browserStorageService.setLocal(AuthTokenType[tokenType], tokenValue);
    } else {
      this.browserStorageService.setSession(AuthTokenType[tokenType], tokenValue);
    }
  }

  private isEmptyString(value: string): boolean {
    return !value || 0 === value.length;
  }
}
