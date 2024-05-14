export interface Role{
    id: number,
    name: string
}

export interface RolePermissions extends Role{
    permissions: PermissionInput[]
}

export interface Permission{
    id: number,
    name: string,
    description: string
}

export interface PermissionInput extends Permission{
    checked: boolean
}

export interface CreateRole{
    name?: string,
    permissionIds?: number[]
}

export interface EditRole{
    roleName?: string,
    addPermissionIds?: number[],
    removePermissionIds?: number[]
}

export interface CloneRole{
    name?: string,
    rolesToClone?: string[]
}