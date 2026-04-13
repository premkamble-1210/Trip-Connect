import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-navbar-profile',
  imports: [],
  templateUrl: './navbar-profile.html',
  styleUrl: './navbar-profile.css',
})
export class NavbarProfile {
  userAvatar=signal("https://images.unsplash.com/photo-1494790108377-be9c29b29330?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=facearea&facepad=2&w=256&h=256&q=80");
}
