import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";

import { AuthenticationRoutingModule } from "./authentication-routing.module";
import { LoginComponent } from "./login/login.component";
import { FormsModule } from "@angular/forms";
import { AccessDeniedComponent } from "./access-denied/access-denied.component";

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    AuthenticationRoutingModule
  ],
  declarations: [LoginComponent, AccessDeniedComponent]
})
export class AuthenticationModule { }
