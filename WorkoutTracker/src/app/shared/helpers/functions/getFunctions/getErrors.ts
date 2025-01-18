import { AbstractControl } from '@angular/forms';

export function getErrors(control: AbstractControl, displayName: string, customMessages: { [k: string]: string } = {}): string[] {
    var errors: string[] = [];
    Object.keys(control.errors || {}).forEach((key) => {
      switch (key) {
        case 'required':
          errors.push(`${displayName} ${customMessages?.[key] ?? "is required."}`);
          break;
        case 'minlength':
          const minlengthError = control.getError('minlength');
          errors.push(`${displayName} ${customMessages?.[key] ?? `must be at least ${minlengthError?.requiredLength} characters long.`}`);
          break;
        case 'pattern':
          errors.push(`${displayName} ${customMessages?.[key] ?? "contains invalid characters."}`);
          break;
        default:
          errors.push(`${displayName} ${customMessages?.[key] ?? "is invalid."}`);
          break;
      }
    });
    return errors;
}
