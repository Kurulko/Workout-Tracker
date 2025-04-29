import { differenceInDays, differenceInMonths, differenceInWeeks, differenceInYears } from "date-fns";
import { showCountOfSomethingStr } from "./showCountOfSomethingStr";

export function showLastDateStr(lastDate: Date): string {
    var today = new Date();

    var countOfDays = differenceInDays(today, lastDate);
    var countOfWeeks = differenceInWeeks(today, lastDate);
    var countOfMonths = differenceInMonths(today, lastDate);
    var countOfYears = differenceInYears(today, lastDate);

    if(countOfWeeks === 0) {
        const todayNumber = 0;
        const yesterdayNumber = 1;
        const dayBeforeYesterdayNumber = 2;
    
        if(countOfDays === todayNumber) {
            return "today";
        }
        else if(countOfDays === yesterdayNumber) {
            return "yesterday";
        }
        else if(countOfDays === dayBeforeYesterdayNumber) {
            return "the day before yesterday";
        }
        else {
            return `${countOfDays} days ago`;
        }
    }
    else if(countOfMonths == 0) {
        return `${showCountOfSomethingStr(countOfWeeks, "week", "weeks")} ago`;
    }
    else if(countOfYears == 0) {
        return `${showCountOfSomethingStr(countOfMonths, "month", "months")} ago`;
    }
    else {
        return "a long time ago";
    }
}
