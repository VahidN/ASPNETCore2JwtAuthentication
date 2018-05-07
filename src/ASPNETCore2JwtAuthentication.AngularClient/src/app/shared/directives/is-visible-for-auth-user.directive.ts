import { Directive, ElementRef, Input, OnDestroy, OnInit } from "@angular/core";
import { AuthService } from "@app/core";
import { Subscription } from "rxjs";

@Directive({
  selector: "[isVisibleForAuthUser]"
})
export class IsVisibleForAuthUserDirective implements OnInit, OnDestroy {

  private subscription: Subscription | null = null;

  @Input() isVisibleForRoles: string[] | null = null;

  constructor(private elem: ElementRef, private authService: AuthService) { }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
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
