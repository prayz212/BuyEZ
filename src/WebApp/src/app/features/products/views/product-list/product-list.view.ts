import { Component, OnInit } from '@angular/core';
import { CatalogService } from 'src/app/core/services/catalog.service';
import { QueryResponse } from 'src/app/shared/models/query-response.model';
import { SelectDataSource } from 'src/app/shared/types/select-data-source';
import { ProductQueryRequest } from '../../models/api.model';
import { Product } from '../../models/product.model';
import { ShowOptions } from '../../types/show-options';

@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.view.html',
})
export class ProductListComponent implements OnInit {
  readonly ShowOptions = ShowOptions;

  showOptions?: ShowOptions;

  sortOptions: SelectDataSource<string>[] = [
    { text: 'Name: A - Z', value: 'ascendingName' },
    { text: 'Name: Z - A', value: 'descendingName' },
    { text: 'Price: Decreasing', value: 'decreasingPrice' },
    { text: 'Price: Increasing', value: 'increasingPrice' },
    { text: 'Date: Newest to oldest', value: 'descendingCreated' },
    { text: 'Date: Oldest to newest', value: 'ascendingCreated' },
  ];

  categoryFilterOptions: string[] = [
    'T-Shirts',
    'Shirts',
    'Pants',
    'Jackets',
    'Shoes',
    'Accessories',
  ];

  priceFilterOptions: string[] = [
    'Below $10',
    '$10 - $100',
    '$100 - $200',
    'Above $200',
  ];

  isShowFilter: boolean = false;

  products: Product[] = [];
  currentPage: number = 0;
  totalPages: number = 0;
  totalCount: number = 0;
  hasPreviousPage: boolean = false;
  hasNextPage: boolean = false;

  constructor(private readonly catalogService: CatalogService) {}

  ngOnInit(): void {
    this.fetchData();
  }

  fetchData(page: number = 1, size: number = 10) {
    const payload: ProductQueryRequest = {
      pageNumber: page,
      pageSize: size,
    };

    this.catalogService
      .queryProducts(payload)
      .subscribe((response: QueryResponse<Product[]>) => {
        this.products = response.items;
        this.currentPage = response.pageNumber;
        this.totalPages = response.totalPages;
        this.totalCount = response.totalCount;
        this.hasPreviousPage = response.hasPreviousPage;
        this.hasNextPage = response.hasNextPage;
      });
  }

  handleShowOptionClicked(option: ShowOptions) {
    console.log('choosing option: ', option);
  }

  handleSortOptionChanged(option: string) {
    console.log('sorting option: ', option);
  }

  handleShowFiltersClicked() {
    this.isShowFilter = !this.isShowFilter;
  }

  handlePageChanged(newPage: number) {
    this.fetchData(newPage);
  }
}
