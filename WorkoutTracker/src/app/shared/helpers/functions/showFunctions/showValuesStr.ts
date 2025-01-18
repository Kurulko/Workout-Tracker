export function showValuesStr(values: string[], maxLength?: number, separator: string = ','): string {
  if(values.length == 0)
    return '';

  if(!maxLength)
    return values.join(separator);

  var result = values[0];
  if(result.length > maxLength)
    return result.slice(0, maxLength).concat("...");

  for(let i  = 1; i < values.length; i++){
    var value = values[i];
    
    if(result.length + value.length > maxLength){
      return result.concat(`${separator} ...`);
    }

    result += `${separator} ${value}`;
  }
  
  return result;
}