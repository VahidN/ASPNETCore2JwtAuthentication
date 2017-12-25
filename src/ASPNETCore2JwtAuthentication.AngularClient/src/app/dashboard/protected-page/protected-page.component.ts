import { Component, OnInit } from "@angular/core";

import { AuthService } from "../../core/services/auth.service";

@Component({
  selector: "app-protected-page",
  templateUrl: "./protected-page.component.html",
  styleUrls: ["./protected-page.component.css"]
})
export class ProtectedPageComponent implements OnInit {

  decodedAccessToken: any = {};
  accessTokenExpirationDate: Date | null = null;

  constructor(private authService: AuthService) { }

  ngOnInit() {
    this.decodedAccessToken = this.authService.getDecodedAccessToken();
    this.accessTokenExpirationDate = this.authService.getAccessTokenExpirationDateUtc();
  }

}
