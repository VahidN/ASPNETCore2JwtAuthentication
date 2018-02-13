import { Component, OnInit } from "@angular/core";

import { TokenStoreService } from "../../core/services/token-store.service";

@Component({
  selector: "app-protected-page",
  templateUrl: "./protected-page.component.html",
  styleUrls: ["./protected-page.component.css"]
})
export class ProtectedPageComponent implements OnInit {

  decodedAccessToken: any = {};
  accessTokenExpirationDate: Date | null = null;

  constructor(private tokenStoreService: TokenStoreService) { }

  ngOnInit() {
    this.decodedAccessToken = this.tokenStoreService.getDecodedAccessToken();
    this.accessTokenExpirationDate = this.tokenStoreService.getAccessTokenExpirationDateUtc();
  }

}
