import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewTrip } from './new-trip';

describe('NewTrip', () => {
  let component: NewTrip;
  let fixture: ComponentFixture<NewTrip>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NewTrip],
    }).compileComponents();

    fixture = TestBed.createComponent(NewTrip);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
