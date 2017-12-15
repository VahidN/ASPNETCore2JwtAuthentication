import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";

import { ProtectedPageComponent } from "./protected-page/protected-page.component";
import { AuthGuardPermission } from "../core/models/auth-guard-permission";
import { AuthGuard } from "../core/services/auth.guard";

const routes: Routes = [
  {
    path: "protectedPage",
    component: ProtectedPageComponent,
    data: {
      permission: {
        permittedRoles: ["Admin1"],
        deniedRoles: null
      } as AuthGuardPermission
    },
    canActivate: [AuthGuard]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }
