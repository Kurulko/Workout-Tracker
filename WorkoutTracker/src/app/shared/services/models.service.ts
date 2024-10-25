import { TokenManager } from '../helpers/token-manager';
import { BaseService } from './base.service';
import { HttpClient, HttpParams } from '@angular/common/http';

export abstract class ModelsService extends BaseService {
    protected readonly emptyPath:string = '';

    constructor(httpClient: HttpClient, tokenManager: TokenManager, controllerName: string) {
        super(httpClient, tokenManager, controllerName);
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
}