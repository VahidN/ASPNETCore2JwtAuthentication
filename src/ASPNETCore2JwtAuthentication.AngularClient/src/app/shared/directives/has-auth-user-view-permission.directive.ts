import { Directive, Input, OnDestroy, OnInit, TemplateRef, ViewContainerRef } from "@angular/core";
import { AuthService } from "@app/core";
import { Subscription } from "rxjs";

@Directive({
  selector: "[hasAuthUserViewPermission]"
})
export class HasAuthUserViewPermissionDirective implements OnInit, OnDestroy {
  private isVisible = false;
  private requiredRoles: string[] | null = null;
  private subscription: Subscription | null = null;

  @Input()
  set hasAuthUserViewPermission(roles: string[] | null) {
    this.requiredRoles = roles;
  }

  // Note, if you don't place the * in front, you won't be able to inject the TemplateRef<any> or ViewContainerRef into your directive.
  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.subscription = this.authService.authStatus$.subscribe(status => this.changeVisibility(status));
    this.changeVisibility(this.authService.isAuthUserLoggedIn());
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  private changeVisibility(status: boolean) {
    const isInRoles = !this.requiredRoles ? true : this.authService.isAuthUserInRoles(this.requiredRoles);
    if (isInRoles && status) {
      if (!this.isVisible) {
        this.viewContainer.createEmbeddedView(this.templateRef);
        this.isVisible = true;
      }
    } else {
      this.isVisible = false;
      this.viewContainer.clear();
    }
  }
}
