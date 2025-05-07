export interface UploadWithPhoto<T> {
    model: T;
    photo: File | null;
}