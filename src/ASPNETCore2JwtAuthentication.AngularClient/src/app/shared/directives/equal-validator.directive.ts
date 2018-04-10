import { Attribute, Directive } from "@angular/core";
import { AbstractControl, NG_VALIDATORS, ValidationErrors, Validator } from "@angular/forms";

@Directive({
  // https://angular.io/guide/styleguide#style-02-08
  // Do use a custom prefix for the selector of directives (e.g, the prefix toh from Tour of Heroes).
  // Do spell non-element selectors in lower camel case unless the selector is meant to match a native HTML attribute.

  // the directive matches elements that have the attribute appValidateEqual and one of the formControlName or formControl or ngModel
  selector:
    "[appValidateEqual][formControlName],[appValidateEqual][formControl],[appValidateEqual][ngModel]",
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: EqualValidatorDirective,
      multi: true // the new directives are added to the previously registered directives instead of overriding them.
    }
  ]
})
export class EqualValidatorDirective implements Validator {
  constructor(@Attribute("compare-to") public compareToControl: string) { }

  validate(element: AbstractControl): ValidationErrors | null {
    const selfValue = element.value;
    const otherControl = element.root.get(this.compareToControl);

    /*
    console.log("EqualValidatorDirective", {
      thisControlValue: selfValue,
      otherControlValue: otherControl ? otherControl.value : null
    });
    */

    if (otherControl && selfValue !== otherControl.value) {
      return {
        appValidateEqual: true // Or a string such as 'Password mismatch.' or an abject.
      };
    }

    if (
      otherControl &&
      otherControl.errors &&
      selfValue === otherControl.value
    ) {
      delete otherControl.errors["appValidateEqual"];
      if (!Object.keys(otherControl.errors).length) {
        otherControl.setErrors(null);
      }
    }

    return null;
  }
}
