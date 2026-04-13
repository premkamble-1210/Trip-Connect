import { Component } from '@angular/core';
import { NavLink } from './nav-link/nav-link';
import { NavbarProfile } from './navbar-profile/navbar-profile';

@Component({
  selector: 'app-navbar',
  imports: [NavLink, NavbarProfile],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {}
