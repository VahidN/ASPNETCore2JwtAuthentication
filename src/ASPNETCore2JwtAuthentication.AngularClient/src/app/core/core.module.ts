import { NgModule, SkipSelf, Optional, } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { HTTP_INTERCEPTORS } from "@angular/common/http";

// import RxJs needed operators only once
import "./services/rxjs-operators";

import { BrowserStorageService } from "./services/browser-storage.service";
import { AuthService } from "./services/auth.service";
import { AppConfig, APP_CONFIG } from "./services/app.config";
import { HeaderComponent } from "./component/header/header.component";
import { AuthGuard } from "./services/auth.guard";
import { AuthInterceptor } from "./services/auth.interceptor";

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
    BrowserStorageService,
    AuthService,
    { provide: APP_CONFIG, useValue: AppConfig },
    AuthGuard,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ]
})
export class CoreModule {
  constructor( @Optional() @SkipSelf() core: CoreModule) {
    if (core) {
      throw new Error("CoreModule should be imported ONLY in AppModule.");
    }
  }
}
