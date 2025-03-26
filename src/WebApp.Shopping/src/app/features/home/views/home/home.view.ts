import { Component } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { AuthSelectors } from 'src/app/features/auth/store/auth.selectors';

@Component({
  selector: 'buyez-home',
  templateUrl: './home.view.html',
})
export class HomeComponent {
  isAuthenticated$: Observable<boolean>;

  constructor(private store: Store) {
    this.isAuthenticated$ = this.store.select(AuthSelectors.selectIsAuthenticated);
  }
}
