import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'buyez-outline-button',
  templateUrl: './outline-button.component.html',
})
export class OutlineButtonComponent<TData> {
  @Input() data!: TData;
  @Input() isDisabled: boolean = false;
  @Input() customClasses: string[] = [];

  @Output() onClick = new EventEmitter<TData>();

  handleOnClicked() {
    this.onClick.emit(this.data);
  }

  get classes() {
    const combinedClasses = [
      'border py-2 px-4 rounded-full focus:outline-none text-xs md:text-sm',
    ];

    if (this.isDisabled)
      combinedClasses.push('border-neutral-400 text-neutral-400');
    else
      combinedClasses.push(
        'border-[#d6d604] text-[#d6d604] hover:text-neutral-800 hover:bg-[#ff0]'
      );

    if (this.customClasses.length > 0) {
      combinedClasses.push(...this.customClasses);
    }

    return combinedClasses.join(' ');
  }
}
