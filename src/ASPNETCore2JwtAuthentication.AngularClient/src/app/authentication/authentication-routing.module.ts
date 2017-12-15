import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";

import { LoginComponent } from "./login/login.component";
import { AccessDeniedComponent } from "./access-denied/access-denied.component";

const routes: Routes = [
  { path: "login", component: LoginComponent },
  { path: "accessDenied", component: AccessDeniedComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthenticationRoutingModule { }
