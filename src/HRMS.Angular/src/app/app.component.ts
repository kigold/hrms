import { Component } from '@angular/core';
import { AuthService } from './services/auth.service';
import { LoginRequest, User } from './models/auth';
import { Menu } from './models/menu';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  menu: Menu[] = [{id: 1, isActive: false, name: "Employee"}, {id: 2, isActive: true, name: "Leave Management"}, {id: 3, isActive: false, name: "Setting"}];
  profile: User = <User>{};
  showLogin: boolean = false;
  title = 'HRMS';

  constructor(private authService: AuthService){}

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
