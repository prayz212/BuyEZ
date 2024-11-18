import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'buyez-pagination',
  templateUrl: './pagination.component.html',
})
export class PaginationComponent {
  @Input() currentPage: number = 0;
  @Input() totalPages: number = 0;
  @Input() hasPrevious: boolean = false;
  @Input() hasNext: boolean = false;

  @Output() onPageChange = new EventEmitter<number>();

  get pages(): number[] {
    const availablePages = Array.from(
      { length: this.totalPages },
      (_, i) => i + 1
    );

    const currentIndex = this.currentPage - 1;
    let pageFrom = currentIndex - 2;
    let pageTo = currentIndex + 2;

    if (pageFrom < 0) {
      pageTo = pageTo + Math.abs(pageFrom);
      pageFrom = 0;
    }

    if (pageTo > this.totalPages - 1) {
      pageFrom = pageFrom - Math.abs(pageTo - this.totalPages + 1);
      pageTo = this.totalPages - 1;
    }

    return availablePages.slice(pageFrom, pageTo + 1);
  }

  handlePageChanged(newPage: number) {
    console.log('Changed to page: ' + newPage);
    this.onPageChange.emit(newPage);
  }
}
