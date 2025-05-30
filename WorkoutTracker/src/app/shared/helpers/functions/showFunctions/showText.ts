export function showText(text: string, maxLength: number){
    if(text.length > maxLength){
        return text.slice(0, maxLength).concat("...");
    }

    return text;
}