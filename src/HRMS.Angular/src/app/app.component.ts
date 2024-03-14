import { Component } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { LoginComponent } from './components/login/login.component';
import { AuthService } from './services/auth.service';
import { LoginRequest, User } from './models/auth';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  loginDialogRef: MatDialogRef<LoginComponent, any> = <MatDialogRef<LoginComponent, any>>{}
  menu = ["Onboard", "Leave Management", "Logout"];
  profile: User = <User>{};
  showLogin: boolean = false;
  showSideNav = false;
  title = 'HRMS';

  constructor(public dialog: MatDialog, private authService: AuthService){}

  ngOnInit(){
    if (!this.isLoggedIn()){
      console.log("User is not Logged in, Show login screen")
      this.showLogin = true;
      this.openLoginDialog();
    }
  }

  isLoggedIn(){
    const user = this.authService.getUserProfile();
    console.log("Get Profile", user)
    if (user == undefined)
      return false;
    this.profile = {
      id: user.id,
      name: user.name,
      username: user.username,
      avatar: user.avatar
    }
    return true;
  }

  openLoginDialog() {
    this.loginDialogRef = this.dialog.open(LoginComponent)
    this.loginDialogRef.componentInstance.loggedIn.subscribe((request: LoginRequest) => this.onLogin(request))
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
        this.loginDialogRef.close();
      },
      error: (e) => this.authService.handleError(e)
    })
  }

  logout(){
    this.authService.logout()
  }

}
