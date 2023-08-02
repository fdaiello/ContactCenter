import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ColorfilterComponent } from './colorfilter.component';

describe('ColorfilterComponent', () => {
  let component: ColorfilterComponent;
  let fixture: ComponentFixture<ColorfilterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ColorfilterComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ColorfilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
