import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { Trip, TripDay } from '../../models/trip-map.models';

@Component({
  selector: 'app-trip-sidebar',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './trip-sidebar.component.html',
  styleUrl: './trip-sidebar.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TripSidebarComponent {
  @Input() trip!: Trip;
  @Input() selectedDayIndex = 0;
  @Input() loading = false;
  @Input() progress = 0;
  @Output() daySelected = new EventEmitter<number>();

  get selectedDay(): TripDay | null {
    return this.trip?.days[this.selectedDayIndex] ?? null;
  }

  get allStops(): string[] {
    const day = this.selectedDay;
    if (!day) return [];
    return [day.source, ...(day.stops ?? []), day.destination];
  }

  select(index: number): void {
    this.daySelected.emit(index);
  }
}
