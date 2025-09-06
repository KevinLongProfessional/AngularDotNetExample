import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { Client, API_BASE_URL } from '../../SwaggerClient'
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule, HttpClientModule,
    AppRoutingModule,
    FormsModule
  ],
  providers: [Client,
    { provide: API_BASE_URL, useValue: 'https://localhost:7206' },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
