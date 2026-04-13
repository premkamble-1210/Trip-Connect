import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExporeTrips } from './expore-trips';

describe('ExporeTrips', () => {
  let component: ExporeTrips;
  let fixture: ComponentFixture<ExporeTrips>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExporeTrips],
    }).compileComponents();

    fixture = TestBed.createComponent(ExporeTrips);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
