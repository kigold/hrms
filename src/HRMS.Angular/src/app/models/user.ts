export interface User{
    id: number
    firstName: string,
    lastName: string,
    email: string,
    avatar: string,
    isActive: boolean
}

export interface UpdateUserStatus{
    userId: number,
    lockout: boolean
}