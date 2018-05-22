import { Injectable } from "@angular/core";
import * as jwt_decode from "jwt-decode";

import { AuthTokenType } from "./../models/auth-token-type";
import { ApiConfigService } from "./api-config.service";
import { BrowserStorageService } from "./browser-storage.service";
import { UtilsService } from "./utils.service";

@Injectable({
  providedIn: 'root'
})
export class TokenStoreService {

  private rememberMeToken = "rememberMe_token";

  constructor(
    private browserStorageService: BrowserStorageService,
    private utilsService: UtilsService,
    private apiConfigService: ApiConfigService) { }

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

  getAuthUserDisplayName(): string {
    return this.getDecodedAccessToken().DisplayName;
  }

  getAccessTokenExpirationDateUtc(): Date | null {
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

  setToken(tokenType: AuthTokenType, tokenValue: string): void {
    if (this.utilsService.isEmptyString(tokenValue)) {
      console.error(`${AuthTokenType[tokenType]} is null or empty.`);
    }

    if (tokenType === AuthTokenType.AccessToken && this.utilsService.isEmptyString(tokenValue)) {
      throw new Error("AccessToken can't be null or empty.");
    }

    if (this.rememberMe()) {
      this.browserStorageService.setLocal(AuthTokenType[tokenType], tokenValue);
    } else {
      this.browserStorageService.setSession(AuthTokenType[tokenType], tokenValue);
    }
  }

  getDecodedTokenRoles(): string[] | null {
    const decodedToken = this.getDecodedAccessToken();
    const roles = decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
    if (!roles) {
      return null;
    }

    if (Array.isArray(roles)) {
      return roles.map(role => role.toLowerCase());
    } else {
      return [roles.toLowerCase()];
    }
  }

  storeLoginSession(response: any): void {
    this.setToken(AuthTokenType.AccessToken, response[this.apiConfigService.configuration.accessTokenObjectKey]);
    this.setToken(AuthTokenType.RefreshToken, response[this.apiConfigService.configuration.refreshTokenObjectKey]);
  }

  rememberMe(): boolean {
    return this.browserStorageService.getLocal(this.rememberMeToken) === true;
  }

  setRememberMe(value: boolean): void {
    this.browserStorageService.setLocal(this.rememberMeToken, value);
  }

  hasStoredAccessAndRefreshTokens(): boolean {
    const accessToken = this.getRawAuthToken(AuthTokenType.AccessToken);
    const refreshToken = this.getRawAuthToken(AuthTokenType.RefreshToken);
    return !this.utilsService.isEmptyString(accessToken) && !this.utilsService.isEmptyString(refreshToken);
  }
}
