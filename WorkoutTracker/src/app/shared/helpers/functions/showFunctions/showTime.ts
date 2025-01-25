import { TimeSpan } from "../../../models/time-span";
import { showCountOfSomethingStr } from "./showCountOfSomethingStr";

export function showTime(time: TimeSpan, isWithSeconds: boolean = false, isWithMilliseconds: boolean = false): string {
  var years = Math.trunc(time.days / 365);
  var months = Math.trunc((time.days - years * 365) / 30);
  var days = time.days - years * 365 - months * 30;

  var timesStr: (string|null)[] = 
    [showYears(years), showMonths(months), showDays(days), showHours(time.hours), showMinutes(time.minutes)];

  if(isWithSeconds) {
    timesStr.push(showSeconds(time.seconds));

    if(isWithMilliseconds) {
      timesStr.push(showMilliseconds(time.milliseconds));
    }
  }

  timesStr = timesStr.filter(t => t)
  return timesStr.join(' and ');
}

export function showYears(years: number): string|null {
  return showSmthIfAboveZero(years, 'year', 'years');
}

export function showMonths(months: number): string|null {
  return showSmthIfAboveZero(months, 'month', 'months');
}

export function showDays(days: number): string|null {
  return showSmthIfAboveZero(days, 'day', 'days');
}

export function showHours(hours: number): string|null {
  return showSmthIfAboveZero(hours, 'hour', 'hours');
}

export function showMinutes(minutes: number): string|null {
  return showSmthIfAboveZero(minutes, 'minute', 'minutes');
}

export function showSeconds(seconds: number): string|null {
  return showSmthIfAboveZero(seconds, 'second', 'seconds');
}

export function showMilliseconds(milliseconds: number): string|null {
  return showSmthIfAboveZero(milliseconds, 'millisecond', 'milliseconds');
}

function showSmthIfAboveZero(count: number, singularStr: string, pluralStr: string): string|null {
  if(count > 0){
    return showCountOfSomethingStr(count, singularStr, pluralStr);
  }

  return null;
}

