import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmployeeComponent } from './pages/employee/employee.component';
import { UserComponent } from './pages/user/user.component';
import { RoleComponent } from './pages/role/role.component';

const routes: Routes = [  
  { path: '', redirectTo: '', pathMatch: 'full'},
  { path: 'employee', component: EmployeeComponent },
  { path: 'user', component: UserComponent },
  { path: 'role', component: RoleComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
