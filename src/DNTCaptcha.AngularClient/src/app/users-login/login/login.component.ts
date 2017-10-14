import { HttpErrorResponse } from "@angular/common/http";
import { Component, OnInit, ViewChild } from "@angular/core";
import { NgForm } from "@angular/forms";

import { LoginForm } from "../login-form";
import { DntCaptchaComponent } from "./../../dnt-captcha/dnt-captcha.component";
import { FormPosterService } from "./../form-poster.service";

@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.css"]
})
export class LoginComponent implements OnInit {
  captchaApiUrl = "http://localhost:5000/DNTCaptchaApi/CreateDNTCaptcha";
  model = new LoginForm("Vahid", "");
  validationErrorMessage: string;
  success: boolean;
  @ViewChild("appDntCaptcha") appDntCaptcha: DntCaptchaComponent;

  constructor(private formPoster: FormPosterService) { }

  ngOnInit() { }

  submitForm(form: NgForm) {
    console.log(this.model);
    console.log(form.value);

    this.success = false;
    this.validationErrorMessage = "";

    this.formPoster
      .postUserForm(this.model)
      .subscribe(data => {
        console.log("success: ", data);
        this.success = true;
        this.appDntCaptcha.doRefresh(); // You can not send the same captcha twice.
      },
      (err: HttpErrorResponse) => {
        console.log("error: ", err);
        if (err.status === 400) {
          const errors = JSON.parse(err.error);
          console.log(errors);
          this.validationErrorMessage = this.errorToString(errors);
        } else {
          this.validationErrorMessage = err.error;
        }
        this.appDntCaptcha.doRefresh(); // You can not send the same captcha twice.
      });
  }

  errorToString(obj: any): string {
    const parts = [];
    parts.push("<ul>");
    for (const key in obj) {
      if (obj.hasOwnProperty(key)) {
        const value = obj[key];
        if (value !== null && value !== undefined) {
          parts.push(`<li>${value}</li>`);
        }
      }
    }
    parts.push("</ul>");
    return parts.join("");
  }
}
