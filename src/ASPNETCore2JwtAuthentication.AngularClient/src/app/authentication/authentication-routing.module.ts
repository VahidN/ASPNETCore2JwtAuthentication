import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { AuthGuard, AuthGuardPermission } from "@app/core";

import { AccessDeniedComponent } from "./access-denied/access-denied.component";
import { ChangePasswordComponent } from "./change-password/change-password.component";
import { LoginComponent } from "./login/login.component";

const routes: Routes = [
  { path: "login", component: LoginComponent },
  { path: "accessDenied", component: AccessDeniedComponent },
  {
    path: "changePassword", component: ChangePasswordComponent,
    data: {
      permission: {
        permittedRoles: ["Admin", "User"]
      } as AuthGuardPermission
    },
    canActivate: [AuthGuard]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthenticationRoutingModule { }
