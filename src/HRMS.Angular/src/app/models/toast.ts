export enum ToastType{
    error,
    info,
    success
}

export interface ToastData{    
    title?: string;
    content?: string;
    type?: ToastType;
    progressWidth?: number;
    show?: boolean;
    errors?: string[]
}