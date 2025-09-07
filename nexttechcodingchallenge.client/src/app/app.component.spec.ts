import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { HackerNewsItem } from '../../SwaggerClient';

describe('AppComponent', () => {
  let component: AppComponent;
  let fixture: ComponentFixture<AppComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AppComponent],
      imports: [HttpClientTestingModule]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create the app', () => {
    expect(component).toBeTruthy();
  });

  it('should retrieve news items from the server', () => {
    const mockItems: HackerNewsItem[] = [
      {
          url: 'example url', id: 1, title: "example title",
          init: function(_data?: any): void {
              throw new Error('Function not implemented.');
          },
          toJSON: function(data?: any) {
              throw new Error('Function not implemented.');
          }
      },
      {
          url: 'example url', id: 21, title: "example title",
          init: function(_data?: any): void {
              throw new Error('Function not implemented.');
          },
          toJSON: function(data?: any) {
              throw new Error('Function not implemented.');
          }
      }
    ];

    component.ngOnInit();

    const req = httpMock.expectOne('/weatherforecast');
    expect(req.request.method).toEqual('GET');
    req.flush(mockItems);

    expect(component.hackerNewsItems).toEqual(mockItems);
  });
});
