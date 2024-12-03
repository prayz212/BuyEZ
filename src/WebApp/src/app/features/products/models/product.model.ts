import { ProductStatus } from '../types/product-status';
import { ProductType } from '../types/product-type';

interface Product {
  id: string;
  name: string;
  price: number;
  type: ProductType;
  // image: ImageDetail;
}

interface ProductDetail {
  id: string;
  name: string;
  description: string;
  price: number;
  type: ProductType;
  status: ProductStatus;
  // images: ImageDetail[];
}

export { Product, ProductDetail };
