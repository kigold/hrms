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

export interface SearchPageRequest extends PageRequest{
    query?: string
}

export interface PageData{
    page: number,
    totalPage: number,
    hasNextPage: boolean,
    hasPrevPage: boolean
}