import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-trip-member-list',
  imports: [CommonModule],
  templateUrl: './trip-member-list.html',
  styleUrl: './trip-member-list.css'
})
export class TripMemberList {
  @Input() members: any[] = [];
}
