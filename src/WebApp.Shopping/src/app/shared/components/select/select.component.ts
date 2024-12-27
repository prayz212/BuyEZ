import { Component, EventEmitter, Input, Output } from '@angular/core';
import { SelectDataSource } from '../../types/select-data-source';

@Component({
  selector: 'buyez-select',
  templateUrl: './select.component.html',
})
export class SelectComponent<TValue> {
  @Input() placeholder: string = 'Select one option';
  @Input() showPlaceholder: boolean = true;
  @Input() options: SelectDataSource<TValue>[] = [];

  @Output() onSelected = new EventEmitter<TValue>();

  handleSelected(event: any) {
    const selectedOptions = event.target.value as TValue;
    this.onSelected.emit(selectedOptions);
  }
}
