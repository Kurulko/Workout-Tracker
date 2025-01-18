export function getEnumElements<T extends Record<string, string|number>>(enumType: T): T[keyof T][] {
    return Object.values(enumType).filter(key => isNaN(Number(key))).map(value => enumType[value  as keyof T]);
}

