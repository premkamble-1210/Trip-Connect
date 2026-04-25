import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

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
  constructor(private router: Router) {}
  @Input() label: string = '';
  @Input() route: string = '';

  isRouteActive(): boolean {
    return this.router.isActive(this.route, { paths: 'exact', queryParams: 'exact', fragment: 'ignored', matrixParams: 'ignored' });
  }
}
