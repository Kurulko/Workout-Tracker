export function showCountOfSomethingStr(countOfSmth: number, singularStr: string, pluralStr: string, throwExceptionIfLessThanZero: boolean = true): string {
    if(throwExceptionIfLessThanZero && countOfSmth < 0)
     throw new Error("Incorrect count value.");
 
    return `${countOfSmth} ${countOfSmth == 1 ? singularStr : pluralStr}`;
}