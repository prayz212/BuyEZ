import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HorizontalTabsDataSource } from 'src/app/shared/types/horizontal-tabs-data-source';
import { ProductDetail } from '../../models/product.model';

@Component({
  selector: 'app-product-details',
  templateUrl: './product-details.view.html',
})
export class ProductDetailsComponent implements OnInit {
  tabs: HorizontalTabsDataSource[] = [
    { key: 'description-tab', text: 'Description', isActive: true },
    {
      key: 'additional-info-tab',
      text: 'Additional information',
      isActive: false,
    },
    { key: 'size-sharp-tab', text: 'Size & Shape', isActive: false },
    { key: 'reviews-tab', text: 'Reviews', isActive: false },
  ];

  product: ProductDetail | undefined;

  constructor(private router: Router, private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.data.subscribe((routeData) => {
      const { data: productDetail } = routeData['dataSource'];

      if (!productDetail) this.router.navigate(['not-found']);
      this.product = productDetail;
    });
  }
}
