import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewEmployeeDocComponent } from './view-employee-doc.component';

describe('EditEmployeeComponent', () => {
  let component: ViewEmployeeDocComponent;
  let fixture: ComponentFixture<ViewEmployeeDocComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ViewEmployeeDocComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ViewEmployeeDocComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
