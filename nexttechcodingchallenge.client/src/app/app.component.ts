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
  public startIndex: number = 0;
  public searchText: string = "";
  public isLoading: boolean = false;
  public reachedEnd: boolean = false;

  constructor(private client: Client, private http: HttpClient) {
  }

  ngOnInit() {
    this.getNewsItems();
  }

  async swapPage(moveForward: boolean) {
    if (moveForward) {
      this.startIndex += this.itemCount;
    }
    else {
      this.startIndex -= this.itemCount;
    }

    if (this.searchText.trim().length == 0) {
      await this.getNewsItems();
    }
    else {
      await this.onSearchChanged(false);
    }
  }

  //to do: refactor to reduce duplicated logic.
  async onSearchChanged(resetPage: boolean = true) {
    this.reachedEnd = false;

    if (resetPage) {
      this.startIndex = 0;
    }

    this.isLoading = true;
    this.hackerNewsItems = [];
    this.client.searchNews(this.itemCount, this.startIndex, this.searchText).subscribe((result) => {
      this.hackerNewsItems = result;
      this.isLoading = false;
      if (this.hackerNewsItems.length < this.itemCount) {
        this.reachedEnd = true;
      }

    });
  }

  async getNewsItems() {

    this.reachedEnd = false;
    this.isLoading = true;
    this.hackerNewsItems = [];

    this.client.getNews(this.itemCount, this.startIndex).subscribe((result) => {
      this.hackerNewsItems = result;
      this.isLoading = false;

      if (this.hackerNewsItems.length < this.itemCount) {
        this.reachedEnd = true;
      }
    });
  }

  title = 'hackernewsKevinLong.client';
}
