export class BaseManager {
    protected hasStorageValue(value: any): boolean {
        return value !== undefined && value !== null && value !== "undefined" && value !== "null" && value !== ''; 
    }
}