import { Component, OnInit } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { IStaticMethods } from 'preline/preline';

declare global {
  interface Window {
    HSStaticMethods: IStaticMethods;
  }
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  title = 'WebApp';

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.router.events.subscribe({
      next: (event) => {
        /* Preline configuration */
        if (event instanceof NavigationEnd) {
          setTimeout(() => {
            window.HSStaticMethods.autoInit();
          }, 100);
        }
      },
    });
  }
}
