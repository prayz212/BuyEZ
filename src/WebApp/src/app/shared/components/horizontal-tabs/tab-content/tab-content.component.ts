import { Component, Input } from '@angular/core';

@Component({
  selector: 'buyez-horizontal-tab-content',
  templateUrl: './tab-content.component.html',
})
export class TabContentComponent {
  @Input() isActive: boolean = false;
}
