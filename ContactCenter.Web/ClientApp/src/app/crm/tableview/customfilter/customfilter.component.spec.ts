import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomfilterComponent } from './customfilter.component';

describe('CustomfilterComponent', () => {
  let component: CustomfilterComponent;
  let fixture: ComponentFixture<CustomfilterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CustomfilterComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CustomfilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
