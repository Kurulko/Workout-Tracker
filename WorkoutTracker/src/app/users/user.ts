export interface User {
    userId: string;
    userName: string;
    email: string|null;
    registered: Date;
    startedWorkingOut: Date|null;
}