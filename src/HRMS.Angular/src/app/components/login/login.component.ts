import { Component, EventEmitter, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { LoginRequest } from '../../models/auth';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  constructor(public dialog: MatDialog) {
  }
  model: LoginRequest = {
    email : "",
    password: ""
  };

  @Output() loggedIn = new EventEmitter<LoginRequest>()
  @Output() toggleDialog = new EventEmitter()
  
  login() {
    this.loggedIn.emit(this.model);
  }

  toggle() {
    this.toggleDialog.emit();
  }

}