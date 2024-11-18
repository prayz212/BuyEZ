import { ProductStatus } from '../types/product-status';
import { ProductType } from '../types/product-type';

export interface Product {
  id: string;
  name: string;
  price: number;
  productType: ProductType;
  // image: ImageDetail;
}

export interface ProductDetail {
  id: string;
  name: string;
  description: string;
  price: number;
  productType: ProductType;
  status: ProductStatus;
  // images: ImageDetail[];
}
