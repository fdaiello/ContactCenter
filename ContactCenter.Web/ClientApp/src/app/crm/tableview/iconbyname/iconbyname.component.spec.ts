import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IconbynameComponent } from './iconbyname.component';

describe('ImageFormaterComponent', () => {
  let component: IconbynameComponent;
  let fixture: ComponentFixture<IconbynameComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ IconbynameComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(IconbynameComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
