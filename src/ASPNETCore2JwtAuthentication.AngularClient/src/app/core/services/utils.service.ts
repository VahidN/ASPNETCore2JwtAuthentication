import { Injectable } from "@angular/core";

@Injectable()
export class UtilsService {

  isEmptyString(value: string): boolean {
    return !value || 0 === value.length;
  }
}
