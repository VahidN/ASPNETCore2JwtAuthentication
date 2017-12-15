import { Injectable } from "@angular/core";

@Injectable()
export class BrowserStorageService {

  getSession(key: string): any {
    const data = window.sessionStorage.getItem(key);
    return JSON.parse(data);
  }

  setSession(key: string, value: any): void {
    const data = value === undefined ? null : JSON.stringify(value);
    window.sessionStorage.setItem(key, data);
  }

  removeSession(key: string): void {
    window.sessionStorage.removeItem(key);
  }

  removeAllSessions(): void {
    for (const key in window.sessionStorage) {
      if (window.sessionStorage.hasOwnProperty(key)) {
        this.removeSession(key);
      }
    }
  }

  getLocal(key: string): any {
    const data = window.localStorage.getItem(key);
    return JSON.parse(data);
  }

  setLocal(key: string, value: any): void {
    const data = value === undefined ? null : JSON.stringify(value);
    window.localStorage.setItem(key, data);
  }

  removeLocal(key: string): void {
    window.localStorage.removeItem(key);
  }

  removeAllLocals(): void {
    for (const key in window.localStorage) {
      if (window.localStorage.hasOwnProperty(key)) {
        this.removeLocal(key);
      }
    }
  }
}
