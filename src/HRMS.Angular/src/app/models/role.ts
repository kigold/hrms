export interface Role{
    id: number,
    name: string
}

export interface Permission{
    id: number,
    name: string,
    description: string
}

export interface PermissionInput{
    checked: boolean,
    id: number,
    name: string,
    description: string
}

export interface CreateRole{
    name?: string,
    permissionIds?: number[]
}

export interface CloneRole{
    name?: string,
    rolesToClone?: string[]
}