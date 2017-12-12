import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { ProtectedPageComponent } from "./protected-page/protected-page.component";

const routes: Routes = [
  { path: "protectedPage", component: ProtectedPageComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }
