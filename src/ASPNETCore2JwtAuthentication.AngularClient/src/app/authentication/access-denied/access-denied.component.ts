import { Component, OnInit } from "@angular/core";
import { Location } from "@angular/common";

import { AuthService } from "../../core/services/auth.service";

@Component({
  selector: "app-access-denied",
  templateUrl: "./access-denied.component.html",
  styleUrls: ["./access-denied.component.css"]
})
export class AccessDeniedComponent implements OnInit {

  isAuthenticated = false;

  constructor(
    private location: Location,
    private authService: AuthService
  ) {
  }

  ngOnInit() {
    this.isAuthenticated = this.authService.isLoggedIn();
  }

  goBack() {
    this.location.back(); // <-- go back to previous location on cancel
  }
}
