import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JoinRequest } from './join-request';

describe('JoinRequest', () => {
  let component: JoinRequest;
  let fixture: ComponentFixture<JoinRequest>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JoinRequest],
    }).compileComponents();

    fixture = TestBed.createComponent(JoinRequest);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
