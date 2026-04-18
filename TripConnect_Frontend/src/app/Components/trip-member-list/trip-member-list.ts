import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-trip-member-list',
  imports: [CommonModule, RouterLink],
  templateUrl: './trip-member-list.html',
  styleUrl: './trip-member-list.css'
})
export class TripMemberList {
  @Input() members: any[] = [];
}
