import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewProfile } from './view-profile';

describe('ViewProfile', () => {
  let component: ViewProfile;
  let fixture: ComponentFixture<ViewProfile>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ViewProfile],
    }).compileComponents();

    fixture = TestBed.createComponent(ViewProfile);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
