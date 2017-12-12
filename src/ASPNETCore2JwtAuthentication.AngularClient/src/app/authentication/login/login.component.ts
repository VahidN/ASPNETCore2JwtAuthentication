import { Component, OnInit } from "@angular/core";
import { NgForm } from "@angular/forms";
import { Router, ActivatedRoute } from "@angular/router";
import { HttpErrorResponse } from "@angular/common/http";

import { AuthService } from "../../core/auth.service";
import { Credentials } from "../../core/credentials";

@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.css"]
})
export class LoginComponent implements OnInit {

  model: Credentials = { username: "", password: "", rememberMe: false };
  error = "";
  returnUrl: string;

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute) { }

  ngOnInit() {
    // reset the login status
    this.authService.logout(false);

    // get the return url from route parameters
    this.returnUrl = this.route.snapshot.queryParams["returnUrl"];
  }

  submitForm(form: NgForm) {
    this.error = "";
    this.authService.login(this.model)
      .subscribe(isLoggedIn => {
        if (isLoggedIn) {
          if (this.returnUrl) {
            this.router.navigate([this.returnUrl]);
          } else {
            this.router.navigate(["/protectedPage"]);
          }
        }
      },
      (error: HttpErrorResponse) => {
        console.log("Login error", error);
        if (error.status === 401) {
          this.error = "Invalid User name or Password. Please try again.";
        } else {
          this.error = `${error.statusText}: ${error.message}`;
        }
      });
  }
}
