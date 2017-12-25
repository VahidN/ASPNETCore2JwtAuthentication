import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { AuthGuardPermission } from "../core/models/auth-guard-permission";
import { AuthGuard } from "../core/services/auth.guard";
import { CallProtectedApiComponent } from "./call-protected-api/call-protected-api.component";
import { ProtectedPageComponent } from "./protected-page/protected-page.component";

const routes: Routes = [
  {
    path: "protectedPage",
    component: ProtectedPageComponent,
    data: {
      permission: {
        permittedRoles: ["Admin"]
      } as AuthGuardPermission
    },
    canActivate: [AuthGuard]
  },
  {
    path: "callProtectedApi",
    component: CallProtectedApiComponent,
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
export class DashboardRoutingModule { }
