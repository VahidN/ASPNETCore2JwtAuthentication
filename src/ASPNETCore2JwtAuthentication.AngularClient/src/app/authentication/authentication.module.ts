import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { SharedModule } from "@app/shared/shared.module";

import { AccessDeniedComponent } from "./access-denied/access-denied.component";
import { AuthenticationRoutingModule } from "./authentication-routing.module";
import { ChangePasswordComponent } from "./change-password/change-password.component";
import { ChangePasswordService } from "./change-password/services/change-password.service";
import { LoginComponent } from "./login/login.component";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    SharedModule,
    AuthenticationRoutingModule
  ],
  declarations: [LoginComponent, AccessDeniedComponent, ChangePasswordComponent],
  providers: [ChangePasswordService]
})
export class AuthenticationModule { }
