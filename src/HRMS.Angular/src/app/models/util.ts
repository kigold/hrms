export interface PagedList<T>{
    totalCount: number,
    pageSize: number,
    currentPage: number,
    totalPages: number
    items: T
}

export interface PageRequest{
    page: number,
    pageSize: number,
}

export interface PageData{
    page: number,
    totalPage: number,
    hasNextPage: boolean,
    hasPrevPage: boolean
}

export interface ToastType{
    message: string;
    type: string; //Info, Error
}