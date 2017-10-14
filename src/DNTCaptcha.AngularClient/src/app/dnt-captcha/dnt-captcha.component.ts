import "rxjs/add/observable/throw";
import "rxjs/add/operator/catch";
import "rxjs/add/operator/map";

import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import { Observable } from "rxjs/Observable";

import { DNTCaptchaApiResponse } from "./dnt-captcha-api-response";
import { DNTCaptchaLanguage } from "./dnt-captcha-language";

@Component({
  selector: "dnt-captcha",
  templateUrl: "./dnt-captcha.component.html",
  styleUrls: ["./dnt-captcha.component.css"]
})
export class DntCaptchaComponent implements OnInit {
  apiResponse = new DNTCaptchaApiResponse();
  hiddenInputName = "DNTCaptchaText";
  hiddenTokenName = "DNTCaptchaToken";
  inputName = "DNTCaptchaInputText";

  @Input() text: string;
  @Output() textChange = new EventEmitter<string>();

  @Input() token: string;
  @Output() tokenChange = new EventEmitter<string>();

  @Input() inputText: string;
  @Output() inputTextChange = new EventEmitter<string>();

  @Input() placeholder: string;
  @Input() apiUrl: string;
  @Input() backColor: string;
  @Input() fontName: string;
  @Input() fontSize: number;
  @Input() foreColor: string;
  @Input() language: DNTCaptchaLanguage;
  @Input() max: number;
  @Input() min: number;

  constructor(private http: HttpClient) { }

  ngOnInit() {
    this.doRefresh();
  }

  private handleError(error: HttpErrorResponse): Observable<any> {
    console.error("getCaptchaInfo error: ", error);
    return Observable.throw(error.statusText);
  }

  getCaptchaInfo(): Observable<DNTCaptchaApiResponse> {
    return this.http
      .post<DNTCaptchaApiResponse>(`${this.apiUrl}`, {
        backColor: this.backColor,
        fontName: this.fontName,
        fontSize: this.fontSize,
        foreColor: this.foreColor,
        language: this.language,
        max: this.max,
        min: this.min
      }, { withCredentials: true /* For CORS */ })
      .map(response => response || {})
      .catch(this.handleError);
  }

  doRefresh() {
    this.inputText = "";
    this.getCaptchaInfo().subscribe(data => {
      this.apiResponse = data;
      this.text = data.dntCaptchaTextValue; this.onTextChange();
      this.token = data.dntCaptchaTokenValue; this.onTokenChange();
    });
  }

  onTextChange() {
    this.textChange.emit(this.text);
  }

  onTokenChange() {
    this.tokenChange.emit(this.token);
  }

  onInputTextChange() {
    this.inputTextChange.emit(this.inputText);
  }
}
