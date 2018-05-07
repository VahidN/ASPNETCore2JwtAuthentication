import { Component, OnDestroy, OnInit } from "@angular/core";
import { Subscription } from "rxjs";

import { AuthService } from "../../services/auth.service";

@Component({
  selector: "app-header",
  templateUrl: "./header.component.html",
  styleUrls: ["./header.component.css"]
})
export class HeaderComponent implements OnInit, OnDestroy {

  title = "Angular.Jwt.Core";

  isLoggedIn = false;
  subscription: Subscription | null = null;
  displayName = "";

  constructor(private authService: AuthService) { }

  ngOnInit() {
    this.subscription = this.authService.authStatus$.subscribe(status => {
      this.isLoggedIn = status;
      if (status) {
        const authUser = this.authService.getAuthUser();
        this.displayName = authUser ? authUser.displayName : "";
      }
    });
  }

  ngOnDestroy() {
    // prevent memory leak when component is destroyed
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  logout() {
    this.authService.logout(true);
  }
}
