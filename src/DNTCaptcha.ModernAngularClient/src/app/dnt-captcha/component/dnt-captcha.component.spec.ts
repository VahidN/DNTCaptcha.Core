import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DntCaptchaComponent } from './dnt-captcha.component';

describe('DntCaptchaComponent', () => {
  let component: DntCaptchaComponent;
  let fixture: ComponentFixture<DntCaptchaComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [DntCaptchaComponent]
    });
    fixture = TestBed.createComponent(DntCaptchaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
