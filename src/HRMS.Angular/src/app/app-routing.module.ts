import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmployeeComponent } from './pages/employee/employee.component';
import { UsersComponent } from './pages/users/users.component';
import { RoleComponent } from './pages/role/role.component';
import { UserComponent } from './pages/user/user.component';
import { authguardGuard } from './util/auth-guard';

const routes: Routes = [  
  { path: '', redirectTo: '', pathMatch: 'full'},
  { path: 'employee', component: EmployeeComponent, canActivate: [authguardGuard] },
  { path: 'user', component: UsersComponent, canActivate: [authguardGuard] },
  { path: 'user/:id', component: UserComponent, canActivate: [authguardGuard] },
  { path: 'role', component: RoleComponent, canActivate: [authguardGuard] },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
