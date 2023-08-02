import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CardcolorComponent } from './cardcolor.component';

describe('CardcolorComponent', () => {
  let component: CardcolorComponent;
  let fixture: ComponentFixture<CardcolorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CardcolorComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CardcolorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
