import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { AccessDeniedComponent } from "./access-denied/access-denied.component";
import { LoginComponent } from "./login/login.component";

const routes: Routes = [
  { path: "login", component: LoginComponent },
  { path: "accessDenied", component: AccessDeniedComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthenticationRoutingModule { }
