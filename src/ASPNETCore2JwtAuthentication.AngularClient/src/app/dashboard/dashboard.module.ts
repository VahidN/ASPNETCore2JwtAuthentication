import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";

import { SharedModule } from "../shared/shared.module";
import { CallProtectedApiComponent } from "./call-protected-api/call-protected-api.component";
import { DashboardRoutingModule } from "./dashboard-routing.module";
import { ProtectedPageComponent } from "./protected-page/protected-page.component";

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    DashboardRoutingModule
  ],
  declarations: [ProtectedPageComponent, CallProtectedApiComponent]
})
export class DashboardModule { }
