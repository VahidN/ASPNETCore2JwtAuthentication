import { Component, OnDestroy, OnInit } from "@angular/core";
import { Subscription } from "rxjs/Subscription";

import { AuthService } from "../../services/auth.service";

@Component({
  selector: "app-header",
  templateUrl: "./header.component.html",
  styleUrls: ["./header.component.css"]
})
export class HeaderComponent implements OnInit, OnDestroy {

  title = "Angular.Jwt.Core";

  isLoggedIn: boolean;
  subscription: Subscription;
  displayName: string;

  constructor(private authService: AuthService) { }

  ngOnInit() {
    this.subscription = this.authService.authStatus$.subscribe(status => {
      this.isLoggedIn = status;
      if (status) {
        this.displayName = this.authService.getAuthUserDisplayName();
      }
    });
  }

  ngOnDestroy() {
    // prevent memory leak when component is destroyed
    this.subscription.unsubscribe();
  }

  logout() {
    this.authService.logout(true);
  }
}
