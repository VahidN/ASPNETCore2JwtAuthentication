import { CommonModule } from "@angular/common";
import { HttpClientModule } from "@angular/common/http";
import { ModuleWithProviders, NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";

import { EqualValidatorDirective } from "./directives/equal-validator.directive";
import { HasAuthUserViewPermissionDirective } from "./directives/has-auth-user-view-permission.directive";
import { IsVisibleForAuthUserDirective } from "./directives/is-visible-for-auth-user.directive";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    HttpClientModule
  ],
  entryComponents: [
    // All components about to be loaded "dynamically" need to be declared in the entryComponents section.
  ],
  declarations: [
    // common and shared components/directives/pipes between more than one module and components will be listed here.
    IsVisibleForAuthUserDirective,
    HasAuthUserViewPermissionDirective,
    EqualValidatorDirective
  ],
  exports: [
    // common and shared components/directives/pipes between more than one module and components will be listed here.
    CommonModule,
    FormsModule,
    HttpClientModule,
    IsVisibleForAuthUserDirective,
    HasAuthUserViewPermissionDirective,
    EqualValidatorDirective
  ]
  /* No providers here! Since they’ll be already provided in AppModule. */
})
export class SharedModule {
  static forRoot(): ModuleWithProviders {
    // Forcing the whole app to use the returned providers from the AppModule only.
    return {
      ngModule: SharedModule,
      providers: [ /* All of your services here. It will hold the services needed by `itself`. */]
    };
  }
}
