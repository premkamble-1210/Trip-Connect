import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-nav-link',
  imports: [CommonModule, RouterModule],
  templateUrl: './nav-link.html',
  styleUrl: './nav-link.css',
  host: {
    class: 'inline-flex h-full'
  }
})
export class NavLink {
  @Input() label: string = '';
  @Input() route: string = '';
}
