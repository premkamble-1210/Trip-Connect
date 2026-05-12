import {
  Component, Input, Output, EventEmitter, OnInit, OnDestroy,
  HostListener, inject, ElementRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { GeocodingService, LocationSuggestion } from '../../features/trip-map/services/geocoding.service';

export interface LocationSelectedEvent {
  name: string;
  lat: number;
  lng: number;
}

@Component({
  selector: 'app-location-autocomplete',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './location-autocomplete.component.html',
})
export class LocationAutocompleteComponent implements OnInit, OnDestroy {
  @Input() placeholder = 'e.g. Goa Beach';
  @Input() set initialValue(v: string) { this.inputText = v; }
  @Output() locationSelected = new EventEmitter<LocationSelectedEvent>();

  private readonly geocoding = inject(GeocodingService);
  private readonly elRef = inject(ElementRef);

  inputText = '';
  suggestions: LocationSuggestion[] = [];
  isOpen = false;
  isLoading = false;

  private readonly search$ = new Subject<string>();
  private sub!: Subscription;

  ngOnInit(): void {
    this.sub = this.search$.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(q => {
        this.isLoading = true;
        return this.geocoding.searchSuggestions(q);
      }),
    ).subscribe(results => {
      this.isLoading = false;
      this.suggestions = results;
      this.isOpen = results.length > 0;
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  onInput(value: string): void {
    this.inputText = value;
    if (value.trim().length < 2) {
      this.suggestions = [];
      this.isOpen = false;
      return;
    }
    this.search$.next(value);
  }

  selectSuggestion(s: LocationSuggestion): void {
    this.inputText = s.displayName;
    this.suggestions = [];
    this.isOpen = false;
    this.locationSelected.emit({ name: s.displayName, lat: s.lat, lng: s.lng });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elRef.nativeElement.contains(event.target)) {
      this.isOpen = false;
    }
  }
}
