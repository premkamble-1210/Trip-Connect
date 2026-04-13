import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-nav-link',
  imports: [CommonModule],
  templateUrl: './nav-link.html',
  styleUrl: './nav-link.css',
  host: {
    class: 'inline-flex h-full'
  }
})
export class NavLink {
  @Input() label: string = '';
  @Input() isActive: boolean = false;
}
