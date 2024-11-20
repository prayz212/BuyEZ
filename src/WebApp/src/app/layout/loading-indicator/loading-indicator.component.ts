import { Component } from '@angular/core';
import { LoadingService } from 'src/app/core/services/loading.service';

@Component({
  selector: 'app-loading-indicator',
  templateUrl: './loading-indicator.component.html',
})
export class LoadingIndicatorComponent {
  loading$ = this.loadingService.loading$;

  constructor(private loadingService: LoadingService) {}
}
