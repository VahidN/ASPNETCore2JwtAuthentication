import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";

import { ProtectedPageComponent } from "./protected-page/protected-page.component";
import { AuthGuardPermission } from "../core/models/auth-guard-permission";
import { AuthGuard } from "../core/services/auth.guard";
import { CallProtectedApiComponent } from "./call-protected-api/call-protected-api.component";

const routes: Routes = [
  {
    path: "protectedPage",
    component: ProtectedPageComponent,
    data: {
      permission: {
        permittedRoles: ["Admin"],
        deniedRoles: null
      } as AuthGuardPermission
    },
    canActivate: [AuthGuard]
  },
  {
    path: "callProtectedApi",
    component: CallProtectedApiComponent,
    data: {
      permission: {
        permittedRoles: ["Admin", "User"],
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
