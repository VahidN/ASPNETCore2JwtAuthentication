import { NgModule, SkipSelf, Optional, } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";

// import RxJs needed operators only once
import "./rxjs-operators";

import { BrowserStorageService } from "./browser-storage.service";
import { AuthService } from "./auth.service";
import { AppConfig, APP_CONFIG } from "./app.config";

@NgModule({
  imports: [CommonModule, RouterModule],
  exports: [
    // components that are used in app.component.ts will be listed here.
  ],
  declarations: [
    // components that are used in app.component.ts will be listed here.
  ],
  providers: [
    // global singleton services of the whole app will be listed here.
    BrowserStorageService,
    AuthService,
    { provide: APP_CONFIG, useValue: AppConfig }
  ]
})
export class CoreModule {
  constructor( @Optional() @SkipSelf() core: CoreModule) {
    if (core) {
      throw new Error("CoreModule should be imported ONLY in AppModule.");
    }
  }
}
