import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NavbarProfile } from './navbar-profile';

describe('NavbarProfile', () => {
  let component: NavbarProfile;
  let fixture: ComponentFixture<NavbarProfile>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavbarProfile],
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarProfile);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
