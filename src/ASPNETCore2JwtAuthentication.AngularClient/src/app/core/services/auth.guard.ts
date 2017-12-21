import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, CanActivate, CanActivateChild, Router, RouterStateSnapshot } from "@angular/router";

import { AuthGuardPermission } from "../models/auth-guard-permission";
import { AuthService } from "./auth.service";

@Injectable()
export class AuthGuard implements CanActivate, CanActivateChild {

  constructor(private authService: AuthService, private router: Router) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    return this.hasAuthUserAccessToThisRoute(route, state);
  }

  canActivateChild(childRoute: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    return this.hasAuthUserAccessToThisRoute(childRoute, state);
  }

  private hasAuthUserAccessToThisRoute(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (!this.authService.isAuthUserLoggedIn()) {
      this.showAccessDenied(state);
      return false;
    }

    const permissionData = route.data["permission"] as AuthGuardPermission;
    if (!permissionData) {
      return true;
    }

    if (Array.isArray(permissionData.deniedRoles) && Array.isArray(permissionData.permittedRoles)) {
      throw new Error("Don't set both 'deniedRoles' and 'permittedRoles' in route data.");
    }

    if (Array.isArray(permissionData.permittedRoles)) {
      const isInRole = this.authService.isAuthUserInRoles(permissionData.permittedRoles);
      if (isInRole) {
        return true;
      }

      this.showAccessDenied(state);
      return false;
    }

    if (Array.isArray(permissionData.deniedRoles)) {
      const isInRole = this.authService.isAuthUserInRoles(permissionData.deniedRoles);
      if (!isInRole) {
        return true;
      }

      this.showAccessDenied(state);
      return false;
    }
  }


  private showAccessDenied(state: RouterStateSnapshot) {
    this.router.navigate(["/accessDenied"], { queryParams: { returnUrl: state.url } });
  }
}
