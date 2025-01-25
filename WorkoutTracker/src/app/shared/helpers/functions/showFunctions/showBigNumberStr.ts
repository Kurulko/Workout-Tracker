export function showBigNumberStr(bigNumber: number): string {
    var bigNumberStr = bigNumber.toString();
    var numberParts: string[] = [];
    
    var dotIndex = bigNumberStr.indexOf('.');
    var lengthTillDot = dotIndex !== -1 ? dotIndex : bigNumberStr.length;

    const numberDivider = 3;
    for(var i = lengthTillDot; ; i -= numberDivider) { //divide each 3rd number
        if(i > 0) {
            if(i - numberDivider > 0) { 
                numberParts.push(bigNumberStr.slice(i - numberDivider, i));
                continue;
            }
            else {
                numberParts.push(bigNumberStr.slice(0, i));
            }
        }
        else if(i >= 1 - numberDivider) {
            numberParts.push(bigNumberStr.slice(0, i + numberDivider));
        }
        
        break;
    }

    var result = numberParts.reverse().join(' ');
    
    if(dotIndex != -1)
        result = result.concat(bigNumberStr.slice(dotIndex));

    return result;
}