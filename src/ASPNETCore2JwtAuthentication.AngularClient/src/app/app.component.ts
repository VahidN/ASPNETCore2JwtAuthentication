import { Component, HostListener} from "@angular/core";
import { RefreshTokenService } from "app/core";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.css"]
})
export class AppComponent {
  constructor(private refreshTokenService: RefreshTokenService) {}

  @HostListener("window:unload", ["$event"])
  unloadHandler() {
    // Invalidate current tab as active RefreshToken timer
    this.refreshTokenService.invalidateCurrentTabId();
  }

  @HostListener("window:beforeunload", ["$event"])
  beforeUnloadHander() {
    // ...
  }
}
