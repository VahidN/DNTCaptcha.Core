import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DntCaptchaComponent } from './dnt-captcha.component';

describe('DntCaptchaComponent', () => {
  let component: DntCaptchaComponent;
  let fixture: ComponentFixture<DntCaptchaComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DntCaptchaComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DntCaptchaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
