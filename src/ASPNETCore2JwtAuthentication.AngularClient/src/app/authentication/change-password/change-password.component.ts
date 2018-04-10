import { HttpErrorResponse } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { NgForm } from "@angular/forms";
import { Router } from "@angular/router";

import { ChangePassword } from "./models/change-password";
import { ChangePasswordService } from "./services/change-password.service";

@Component({
  selector: "app-change-password",
  templateUrl: "./change-password.component.html",
  styleUrls: ["./change-password.component.css"]
})
export class ChangePasswordComponent implements OnInit {

  error = "";
  model: ChangePassword = {
    oldPassword: "",
    newPassword: "",
    confirmPassword: ""
  };

  constructor(private router: Router, private changePasswordService: ChangePasswordService) { }

  ngOnInit() {
  }

  submitForm(form: NgForm) {
    console.log(this.model);
    console.log(form.value);
    this.error = "";
    this.changePasswordService.changePassword(this.model)
      .subscribe(() => {
        this.router.navigate(["/welcome"]);
      }, (error: HttpErrorResponse) => {
        console.error("ChangePassword error", error);
        this.error = `${error.error} -> ${error.statusText}: ${error.message}`;
      });
  }
}
