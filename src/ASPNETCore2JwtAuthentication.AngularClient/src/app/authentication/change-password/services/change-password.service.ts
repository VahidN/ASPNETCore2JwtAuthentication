import { HttpClient, HttpErrorResponse, HttpHeaders } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { APP_CONFIG, IAppConfig } from "@app/core";
import { Observable, throwError } from "rxjs";
import { catchError, map } from "rxjs/operators";

import { ChangePassword } from "./../models/change-password";

@Injectable()
export class ChangePasswordService {

  constructor(
    private http: HttpClient,
    @Inject(APP_CONFIG) private appConfig: IAppConfig) { }

  changePassword(model: ChangePassword): Observable<any> {
    const headers = new HttpHeaders({ "Content-Type": "application/json" });
    const url = `${this.appConfig.apiEndpoint}/ChangePassword`;
    return this.http
      .post(url, model, { headers: headers })
      .pipe(
        map(response => response || {}),
        catchError((error: HttpErrorResponse) => throwError(error))
      );
  }
}
