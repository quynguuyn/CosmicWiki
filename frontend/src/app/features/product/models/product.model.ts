export interface ProductImage {
  id: string;
  url: string;
  displayOrder: number;
}

export interface Product {
  id: string;
  sku: string;
  name: string;
  slug: string;
  description: string | null;
  originalPrice: number;
  salePrice: number;
  stockQuantity: number;
  status: string;
  categoryId: string;
  images: ProductImage[];
  urls?: string[];
}

export interface ProductCategory {
  id: string;
  name: string;
  slug: string;
  description: string | null;
}
