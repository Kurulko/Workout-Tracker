import { DateTimeRange } from '../models/date-time-range';
import { BaseService } from './base.service';
import { HttpClient, HttpParams } from '@angular/common/http';

export abstract class ModelsService extends BaseService {
    constructor(httpClient: HttpClient, controllerName: string) {
        super(httpClient, controllerName);
    }

    protected getApiResultHttpParams(pageIndex:number, pageSize:number, sortColumn:string, sortOrder:string, filterColumn:string|null, filterQuery:string|null) : HttpParams
    {
        var params =  new HttpParams()
            .set("pageIndex", pageIndex.toString())
            .set("pageSize", pageSize.toString())
            .set("sortColumn", sortColumn)
            .set("sortOrder", sortOrder);
  
        if (filterColumn && filterQuery) {
            params = params
                .set("filterColumn", filterColumn)
                .set("filterQuery", filterQuery);
        }

        return params;
    }

    protected getRangeParams(params: HttpParams, range:DateTimeRange|null) : HttpParams
    {
        if(range){
            params = params
                .set('firstDate', range.firstDate.toDateString())
                .set('lastDate', range.lastDate.toDateString());
        }

        return params;
    }
}