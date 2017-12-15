import { InjectionToken } from "@angular/core";

export let APP_CONFIG = new InjectionToken<string>("app.config");

export interface IAppConfig {
  apiEndpoint: string;
  loginPath: string;
  logoutPath: string;
  refreshTokenPath: string;
  accessTokenObjectKey: string;
  refreshTokenObjectKey: string;
}

export const AppConfig: IAppConfig = {
  apiEndpoint: "http://localhost:5000/api",
  loginPath: "account/login",
  logoutPath: "account/logout",
  refreshTokenPath: "account/RefreshToken",
  accessTokenObjectKey: "access_token",
  refreshTokenObjectKey: "refresh_token"
};
