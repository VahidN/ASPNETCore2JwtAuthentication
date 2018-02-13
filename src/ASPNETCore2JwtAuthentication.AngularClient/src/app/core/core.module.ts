import { CommonModule } from "@angular/common";
import { HTTP_INTERCEPTORS } from "@angular/common/http";
import { NgModule, Optional, SkipSelf } from "@angular/core";
import { RouterModule } from "@angular/router";

import { HeaderComponent } from "./component/header/header.component";
import { APP_CONFIG, AppConfig } from "./services/app.config";
import { AuthGuard } from "./services/auth.guard";
import { AuthInterceptor } from "./services/auth.interceptor";
import { AuthService } from "./services/auth.service";
import { BrowserStorageService } from "./services/browser-storage.service";
import { RefreshTokenService } from "./services/refresh-token.service";
import { TokenStoreService } from "./services/token-store.service";
import { UtilsService } from "./services/utils.service";

@NgModule({
  imports: [CommonModule, RouterModule],
  exports: [
    // components that are used in app.component.ts will be listed here.
    HeaderComponent
  ],
  declarations: [
    // components that are used in app.component.ts will be listed here.
    HeaderComponent
  ],
  providers: [
    // global singleton services of the whole app will be listed here.
    UtilsService,
    BrowserStorageService,
    TokenStoreService,
    RefreshTokenService,
    {
      provide: APP_CONFIG,
      useValue: AppConfig
    },
    AuthService,
    AuthGuard,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ]
})
export class CoreModule {
  constructor(@Optional() @SkipSelf() core: CoreModule) {
    if (core) {
      throw new Error("CoreModule should be imported ONLY in AppModule.");
    }
  }
}
