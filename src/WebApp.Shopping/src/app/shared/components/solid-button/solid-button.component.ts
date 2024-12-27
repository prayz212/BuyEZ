import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'buyez-solid-button',
  templateUrl: './solid-button.component.html',
})
export class SolidButtonComponent<TData> {
  @Input() data!: TData;
  @Input() isDisabled: boolean = false;
  @Input() customClasses: string[] = [];

  @Output() onClick = new EventEmitter<TData>();

  handleOnClicked() {
    this.onClick.emit(this.data);
  }

  get classes() {
    const combinedClasses = [
      'bg-yellow-400 border border-transparent text-neutral-800 py-2 px-4 rounded-full text-xs md:text-sm font-semibold',
    ];

    if (this.customClasses.length > 0)
      combinedClasses.push(...this.customClasses);

    return combinedClasses.join(' ');
  }
}
