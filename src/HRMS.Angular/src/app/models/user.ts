import { Permission, Role } from "./role"

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

export interface UserDetails{
    user: User,
    roles: Role[],
    permissions: Permission[]
}