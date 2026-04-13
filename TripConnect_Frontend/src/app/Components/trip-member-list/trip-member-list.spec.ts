import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TripMemberList } from './trip-member-list';

describe('TripMemberList', () => {
  let component: TripMemberList;
  let fixture: ComponentFixture<TripMemberList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TripMemberList],
    }).compileComponents();

    fixture = TestBed.createComponent(TripMemberList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
