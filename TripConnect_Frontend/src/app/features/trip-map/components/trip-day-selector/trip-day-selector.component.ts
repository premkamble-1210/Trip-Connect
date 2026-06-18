import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { TripDay } from '../../models/trip-map.models';

@Component({
  selector: 'app-trip-day-selector',
  standalone: true,
  imports: [SlicePipe],
  templateUrl: './trip-day-selector.component.html',
  styleUrl: './trip-day-selector.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TripDaySelectorComponent {
  @Input() days: TripDay[] = [];
  @Input() selectedDay = 0;
  @Output() daySelected = new EventEmitter<number>();

  select(index: number): void {
    this.daySelected.emit(index);
  }
}
