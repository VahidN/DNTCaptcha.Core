import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DntCaptchaComponent } from './dnt-captcha.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { DntCaptchaForm } from '../interfaces/dnt-captcha-form.interface';

describe('DntCaptchaComponent', () => {
  let component: DntCaptchaComponent;
  let fixture: ComponentFixture<DntCaptchaComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [DntCaptchaComponent, HttpClientTestingModule]
    });
    fixture = TestBed.createComponent(DntCaptchaComponent);
  });

  it('should create', () => {
    component = fixture.componentInstance;
    component.captchaForm = new FormGroup<DntCaptchaForm>({
      dntCaptchaText: new FormControl<string>('', { nonNullable: true, validators: Validators.required }),
      dntCaptchaToken: new FormControl<string>('', { nonNullable: true, validators: Validators.required }),
      dntCaptchaInputText: new FormControl<string>('', { nonNullable: true, validators: Validators.required })
    });
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
