import { Injectable, OnDestroy, OnInit } from '@angular/core';
import { WeightType } from 'src/app/shared/models/weight-type';
import { SizeType } from 'src/app/shared/models/size-type';
import { BaseManager } from './base-manager';
import { Observable, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PreferencesManager extends BaseManager implements OnDestroy, OnInit  {
    private preferableWeightTypeChanged = new Subject<WeightType|undefined>();
    private preferableSizeTypeChanged = new Subject<SizeType|undefined>();
    
    private readonly preferableWeightTypeStorageName: string = 'preferable-weight-type';
    private readonly preferableSizeTypeStorageName: string = 'preferable-size-type';

    public readonly defaultSizeType = SizeType.Centimeter;
    public readonly defaultWeightType = WeightType.Kilogram;

    ngOnInit(): void {
        this.setPreferableSizeType(this.defaultSizeType);
        this.setPreferableWeightType(this.defaultWeightType);
    }

    public hasPreferableWeightType(): boolean {
        const localStoragePreferableWeightType = localStorage[this.preferableWeightTypeStorageName];
        return this.hasStorageValue(localStoragePreferableWeightType);
    }

    public getPreferableWeightType(): WeightType|undefined{
        const hasValue =  this.hasPreferableWeightType();

        if(hasValue){
            return this.getPreferableWeightTypeFromStorage();
        }

        return undefined;
    }

    public isPreferableWeightTypeChanged(): Observable<WeightType|undefined> {
        return this.preferableWeightTypeChanged.asObservable();
    }

    private getPreferableWeightTypeFromStorage(): WeightType|undefined {
        return JSON.parse(localStorage[this.preferableWeightTypeStorageName]) as WeightType|undefined;
    }

    public setPreferableWeightType(weightType: WeightType): void {
        localStorage[this.preferableWeightTypeStorageName] = JSON.stringify(weightType);
        this.preferableWeightTypeChanged.next(weightType);
    }

    public clearPreferableWeightType(): void {
        localStorage.removeItem(this.preferableWeightTypeStorageName);
        this.preferableWeightTypeChanged.next(undefined);
    }



    public hasPreferableSizeType(): boolean {
        const localStoragePreferableSizeType = localStorage[this.preferableSizeTypeStorageName];
        return this.hasStorageValue(localStoragePreferableSizeType);
    }

    public getPreferableSizeType(): SizeType|undefined{
        const hasValue =  this.hasPreferableSizeType();

        if(hasValue){
            return this.getPreferableSizeTypeFromStorage();
        }

        return undefined;
    }

    public isPreferableSizeTypeChanged(): Observable<SizeType|undefined> {
        return this.preferableSizeTypeChanged.asObservable();
    }

    private getPreferableSizeTypeFromStorage(): SizeType|undefined {
        return JSON.parse(localStorage[this.preferableSizeTypeStorageName]) as SizeType|undefined;
    }

    public setPreferableSizeType(sizeType: SizeType): void {
        localStorage[this.preferableSizeTypeStorageName] = JSON.stringify(sizeType);
        this.preferableSizeTypeChanged.next(sizeType);
    }

    public clearPreferableSizeType(): void {
        localStorage.removeItem(this.preferableSizeTypeStorageName);
        this.preferableSizeTypeChanged.next(undefined);
    }


    ngOnDestroy(): void {
        this.preferableWeightTypeChanged.complete();
        this.preferableSizeTypeChanged.complete();
    }
}