import { Injectable } from "@angular/core";

import { BrowserStorageService } from "./browser-storage.service";

@Injectable({
  providedIn: 'root'
})
export class UtilsService {

  constructor(private browserStorageService: BrowserStorageService) { }

  isEmptyString(value: string): boolean {
    return !value || 0 === value.length;
  }

  getCurrentTabId(): number {
    const tabIdToken = "currentTabId";
    let tabId = this.browserStorageService.getSession(tabIdToken);
    if (tabId) {
      return tabId;
    }
    tabId = Math.random();
    this.browserStorageService.setSession(tabIdToken, tabId);
    return tabId;
  }
}
