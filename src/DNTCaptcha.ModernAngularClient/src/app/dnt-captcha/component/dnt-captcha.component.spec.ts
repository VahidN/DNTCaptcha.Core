import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DntCaptchaComponent } from './dnt-captcha.component';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { DntCaptchaForm } from '../interfaces/dnt-captcha-form.interface';
import { DntCaptchaService } from '../service/dnt-captcha.service';
import { provideHttpClient } from '@angular/common/http';
import { of } from 'rxjs';

const captchaFormMock = new FormGroup<DntCaptchaForm>({
  dntCaptchaText: new FormControl<string>('', { nonNullable: true, validators: Validators.required }),
  dntCaptchaToken: new FormControl<string>('', { nonNullable: true, validators: Validators.required }),
  dntCaptchaInputText: new FormControl<string>('', { nonNullable: true, validators: Validators.required })
});

describe('DntCaptchaComponent', () => {
  let component: DntCaptchaComponent;
  let fixture: ComponentFixture<DntCaptchaComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [DntCaptchaComponent, FormsModule, ReactiveFormsModule],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    }).overrideComponent(DntCaptchaComponent, {
      remove: {
        providers: [DntCaptchaService]
      },
      add: {
        providers: [
          {
            provide: DntCaptchaService,
            useValue: { getDntCaptchaParams: jest.fn() }
          }
        ]
      }
    });
  });
  it('should create and call loadNewCaptcha upon creation', () => {
    fixture = TestBed.createComponent(DntCaptchaComponent);
    component = fixture.componentInstance;
    component.captchaForm = captchaFormMock;
    const loadNewCaptchaSpy = jest.spyOn(component, 'loadNewCaptcha').mockImplementation(() => null);

    fixture.detectChanges();

    expect(loadNewCaptchaSpy).toHaveBeenCalledTimes(1);
    expect(component).toBeTruthy();
  });

  it('should call getDntCaptchaParams, set the image and patch form value', () => {
    fixture = TestBed.createComponent(DntCaptchaComponent);
    component = fixture.componentInstance;
    component.dntCaptchaService.getDntCaptchaParams = jest.fn().mockReturnValueOnce(
      of({
        dntCaptchaImgUrl: 'dntCaptchaImgUrl',
        dntCaptchaTextValue: 'dntCaptchaTextValue',
        dntCaptchaTokenValue: 'dntCaptchaTokenValue',
        dntCaptchaId: 'dntCaptchaId'
      })
    );
    component.captchaForm = captchaFormMock;

    component.loadNewCaptcha();

    expect(component.dntCaptchaService.getDntCaptchaParams).toHaveBeenCalledTimes(1);
    expect(component.captchaImageUrl).toBe('dntCaptchaImgUrl');
    expect(component.captchaForm.getRawValue()).toEqual({
      dntCaptchaText: 'dntCaptchaTextValue',
      dntCaptchaToken: 'dntCaptchaTokenValue',
      dntCaptchaInputText: ''
    });
    expect(component).toBeTruthy();
  });
});
