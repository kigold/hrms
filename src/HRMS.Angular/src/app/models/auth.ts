export interface User{
    id: number,
    name: string,
    username: string,
    avatar: string
}

export interface LoginRequest{
    email: string,
    password: string
}

export interface LoginResponseModel {
    access_token: string;
    token_type: string;
    expires_in: number;
    scope: string;
    refresh_token: string;
    error: string;
    error_description: string;
    error_uri: string;
  }