import { Directive, ElementRef, Input, OnDestroy, OnInit } from "@angular/core";
import { Subscription } from "rxjs/Subscription";

import { AuthService } from "../../core/services/auth.service";

@Directive({
  selector: "[isVisibleForAuthUser]"
})
export class IsVisibleForAuthUserDirective implements OnInit, OnDestroy {

  private subscription: Subscription;

  @Input() isVisibleForRoles: string[];

  constructor(private elem: ElementRef, private authService: AuthService) { }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  ngOnInit(): void {
    this.subscription = this.authService.authStatus$.subscribe(status => this.changeVisibility(status));
    this.changeVisibility(this.authService.isAuthUserLoggedIn());
  }

  private changeVisibility(status: boolean) {
    const isInRoles = !this.isVisibleForRoles ? true : this.authService.isAuthUserInRoles(this.isVisibleForRoles);
    this.elem.nativeElement.style.display = isInRoles && status ? "" : "none";
  }
}
