import { Component, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { HelperService } from '../../services/helper.service';
import { UserDetails } from '../../models/user';

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styleUrl: './user.component.css'
})
export class UserComponent {  
  loading: boolean = false;
  userId: number = 0;
  user: UserDetails = <UserDetails>{};
  tab: string = 'details'

  constructor(
    private route: ActivatedRoute,
    private userService: UserService,
    private helperService: HelperService  ) {}
  
  ngOnInit() {
    this.userId = parseInt(this.route.snapshot.paramMap.get('id')??"0");
    console.log("Loading User Details")
    this.getUserDetails();
  }

  getUserDetails(){    
    this.loading = true;
    this.userService.getUserDetails(this.userId)
    .subscribe({
      next: (res) => {
        this.user = res;
        this.loading = false;
        console.log(res)
      },
      error: (e) => this.userService.handleError(e)
    })
  }
}
