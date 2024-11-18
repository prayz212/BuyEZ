import { Pipe, PipeTransform } from '@angular/core';
import { ProductStatus } from 'src/app/features/products/types/product-status';
import { ProductType } from 'src/app/features/products/types/product-type';

const SUPPORTED_TRANSFORMATIONS: TextTransformType[] = [
  {
    key: 'product-status',
    transformFn: (value: any): string => {
      switch (value as ProductStatus) {
        case ProductStatus.InStock:
          return 'In Stock';
        case ProductStatus.OutOfStock:
          return 'Out of Stock';
        case ProductStatus.Restocking:
          return 'Restocking';
        default:
          return '';
      }
    },
  },
  {
    key: 'product-type',
    transformFn: (value: any): string => {
      switch (value as ProductType) {
        case ProductType.Men_TShirt:
          return 'Men TShirt';
        case ProductType.Men_Pants:
          return 'Men Pants';
        case ProductType.Men_Jacket:
          return 'Men Jacket';
        case ProductType.Women_Pants:
          return 'Women Pants';
        case ProductType.Women_TShirt:
          return 'Women T-Shirt';
        case ProductType.Women_Jacket:
          return 'Women Jacket';
        default:
          return '';
      }
    },
  },
];

interface TextTransformType {
  key: string;
  transformFn: (value: any) => string;
}

@Pipe({
  name: 'text',
})
export class TextPipe implements PipeTransform {
  transform(value: number, enumerationName: any): string | null {
    const transform = SUPPORTED_TRANSFORMATIONS.find(
      (transformation) => transformation.key === enumerationName
    );

    if (!transform) return null;

    return transform.transformFn(value);
  }
}
