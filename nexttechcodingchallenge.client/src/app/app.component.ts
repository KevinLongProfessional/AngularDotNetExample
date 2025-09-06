import { Component, OnInit } from '@angular/core';
import { Client, HackerNewsItem } from '../../SwaggerClient'
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  providers: [Client]
})
export class AppComponent implements OnInit {
  public hackerNewsItems: HackerNewsItem[] = [];
  public itemCount: number = 20;
  public startIndex: number | undefined = undefined;
  public searchText: string = "";

  constructor(private client: Client, private http: HttpClient) {
  }

  ngOnInit() {
    this.getNewsItems();
  }

  async onSearchChanged(newValue: string) {
    this.client.searchNews(this.itemCount, this.startIndex, this.searchText).subscribe((result) => {
      this.hackerNewsItems = result;
    });
  }

  async getNewsItems() {
    console.log("!");

   
    //var headers = new HttpHeaders();
    //headers.append("itemCount", this.itemCount.toString());
    ////this is a hack, if the startIndex is 0 or below the server gets the highest start index possible instead.
    //headers.append("startIndex", this.startIndex == null ? "-1" : this.startIndex.toString());

    //this.http.get<HackerNewsItem[]>("//news/getnews/", { headers: headers }).subscribe(
    //  (result) => {
    //    this.hackerNewsItems = result;
    //  },
    //  (error) => {
    //    console.error(error);
    //  }
    //);

    this.client.getNews(this.itemCount, this.startIndex).subscribe((result) => {
      this.hackerNewsItems = result;
    });
  }

  title = 'hackernewsKevinLong.client';
}
