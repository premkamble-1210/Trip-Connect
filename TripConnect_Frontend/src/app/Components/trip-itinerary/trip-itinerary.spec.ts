import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TripItinerary } from './trip-itinerary';

describe('TripItinerary', () => {
  let component: TripItinerary;
  let fixture: ComponentFixture<TripItinerary>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TripItinerary],
    }).compileComponents();

    fixture = TestBed.createComponent(TripItinerary);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
