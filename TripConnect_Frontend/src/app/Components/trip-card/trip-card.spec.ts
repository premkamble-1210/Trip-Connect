import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';

import { TripCard, Trip } from './trip-card';

describe('TripCard', () => {
  let component: TripCard;
  let fixture: ComponentFixture<TripCard>;

  const mockTrip: Trip = {
    id: 1,
    title: 'Bali Adventure',
    image: 'https://example.com/bali.jpg',
    dates: 'Jan 1, 2026 - Jan 10, 2026',
    status: 'Planned',
    price: 1200,
    hostName: 'Alice',
    memberCount: 3,
    openSeats: 2
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TripCard],
      providers: [
        { provide: Router, useValue: { navigate: () => Promise.resolve(true) } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TripCard);
    component = fixture.componentInstance;
    component.trip = mockTrip;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});