export function showCountOfSomethingLeftStr(elements: any[], countOfElementsShown: number): string|null {
  var difference = elements.length - countOfElementsShown;

  if(difference <= 0)
    return null;

  return `... ${difference} more`;
}