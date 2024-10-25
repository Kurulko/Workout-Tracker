import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MusclesComponent } from './muscles.component';

describe('MusclesComponent', () => {
  let component: MusclesComponent;
  let fixture: ComponentFixture<MusclesComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [MusclesComponent]
    });
    fixture = TestBed.createComponent(MusclesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
