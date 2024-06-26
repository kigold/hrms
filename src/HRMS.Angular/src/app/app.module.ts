import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import {  HttpClientModule, provideHttpClient, withInterceptors } from '@angular/common/http';

import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { LoginComponent } from './components/login/login.component';
import { headerInterceptor } from './interceptor/header.interceptor';
import { ToastComponent } from './components/toast/toast.component';
import { UsersComponent } from './pages/users/users.component';
import { RoleComponent } from './pages/role/role.component';
import { UserComponent } from './pages/user/user.component';


@NgModule({
  declarations: [
    AppComponent,
    UserComponent,
  ],
  imports: [
    AppRoutingModule,
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    ToastComponent,
    LoginComponent,    
    UsersComponent,
    RoleComponent    
  ],
  providers: [
    provideAnimationsAsync(),
    provideHttpClient(withInterceptors([headerInterceptor]))
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
