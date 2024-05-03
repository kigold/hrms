import { Component, EventEmitter, Input, Output } from '@angular/core';
import { LoginRequest } from '../../models/auth';
import { FormControl, FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ FormsModule ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  constructor() {
  }
  showLogin_: boolean = false;
  model: LoginRequest = {
    email : "",
    password: ""
  };

  @Input() showLogin: boolean = false;
  @Output() loggedIn = new EventEmitter<LoginRequest>()
  @Output() toggleModal = new EventEmitter()
  
  login() {
    this.loggedIn.emit(this.model);
  }

  toggle() {
    this.toggleModal.emit();
  }

}