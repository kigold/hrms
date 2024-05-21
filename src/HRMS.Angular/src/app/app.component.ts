import { Component } from '@angular/core';
import { AuthService } from './services/auth.service';
import { LoginRequest, AuthUser } from './models/auth';
import { Menu } from './models/menu';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {
  menu: Menu[] = [
    {id: 1, isActive: true, name: "Users", link:'/user' },
    {id: 2, isActive: true, name: "Role", link:'/role' },  
    {id: 3, isActive: true, name: "Employee", link:'/employee' }, 
    {id: 4, isActive: true, name: "Leave Management" , link:'/leave'}, 
    {id: 5, isActive: true, name: "Setting" , link:'/'}
  ];
  profile: AuthUser = <AuthUser>{};
  showLogin: boolean = false;
  title = 'HRMS';
  loading: boolean = false;

  constructor(private authService: AuthService
  ){}

  ngOnInit(){
    this.setUserProfile();
    if (!this.isLoggedIn()){
      console.log("User is not Logged in, Show login screen")
      this.showLogin = true;
    }
  }

  setUserProfile(){
    const user = this.authService.getUserProfile();
    console.log("Get Profile", user)
    if (user == undefined)
      return;
    this.profile = {
      id: user.id,
      name: user.name,
      username: user.username,
      avatar: user.avatar
    }
  }

  isLoggedIn(){
    if (this.profile == undefined || this.profile.id == undefined)
      return false;
    return true;
  }

  onLogin(request: LoginRequest){
    this.authService.login(request)
    .subscribe({
      next: (res) => {
        let user = this.authService.storeAuthInLocalStorage(res);
        this.profile = {
          id: user.id,
          name: user.name,
          username: user.username,
          avatar: user.avatar
        };
        this.showLogin = false;
      },
      error: (e) => this.authService.handleError(e)
    })
  }

  logout(){
    this.authService.logout()
  }

}
