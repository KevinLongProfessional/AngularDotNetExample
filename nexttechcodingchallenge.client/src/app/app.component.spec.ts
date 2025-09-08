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
    const mockItems = [
      {
          url: 'example url', id: 1, title: "example title"
      },
      {
          url: 'example url', id: 21, title: "example title"
      }
    ];

    component.ngOnInit();

    const req = httpMock.expectOne('/News/GetNews?itemCount=20&startIndex=0');
    expect(req.request.method).toEqual('GET');
    let jsonItems = JSON.stringify(mockItems); 
    const responseBlob = new Blob([jsonItems], { type: 'application/octet-stream' });

    req.flush(responseBlob);

    setTimeout(() => {
      component.isLoading = false;
      expect(component.hackerNewsItems.length).toEqual(mockItems.length);
    }, 1000);
   
  });
});
