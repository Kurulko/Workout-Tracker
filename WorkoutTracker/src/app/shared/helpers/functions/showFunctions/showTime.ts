import { TimeSpan } from "../../../models/time-span";
import { roundNumber } from "../roundNumber";
import { showCountOfSomethingStr } from "./showCountOfSomethingStr";

export function showTime(time: TimeSpan): string {
  let timeStr = '';
  const andStr = " and ";

  var hours = time.hours;
  if(hours > 0) {
    if(hours >= 24) {
      var days = roundNumber(time.hours / 24, 0);
      timeStr += showDays(days);

      var hoursLeft = hours - days * 24;
      if(hoursLeft > 0) {
        timeStr += andStr + showHours(hoursLeft);
      }
    }
    else {
      timeStr += showHours(hours);
    }
  }

  var minutes = time.minutes;
  if(minutes > 0) {

    if(timeStr !== '')
      timeStr += andStr;
    
    timeStr += showMinutes(minutes);
  } 

  return timeStr;
}

export function showDays(days: number): string {
  return showSmthIfAboveZero(days, 'day', 'days');
}

export function showHours(hours: number): string {
  return showSmthIfAboveZero(hours, 'hour', 'hours');
}

export function showMinutes(minutes: number): string {
  return showSmthIfAboveZero(minutes, 'minute', 'minutes');
}

function showSmthIfAboveZero(count: number, singularStr: string, pluralStr: string): string {
  if(count > 0){
    return showCountOfSomethingStr(count, singularStr, pluralStr);
  }

  return '';
}

