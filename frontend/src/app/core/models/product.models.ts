export interface ProductDto {
  id: number;
  sku: string;
  name: string;
  brandName: string;
  purchasePrice: number;
  imageUrl: string | null;
  isInStock: boolean;
  stockCount: number;
}

export interface ProductDetailDto {
  id: number;
  sku: string;
  name: string;
  description: string;
  brandName: string;
  colorName: string;
  categoryName: string;
  subCategoryName: string;
  purchasePrice: number;
  stockCount: number;
  isInStock: boolean;
  images: string[];
  attributes: { [key: string]: string };
}

export interface BrandDto {
  id: number;
  name: string;
}

export interface ProductListResponse {
  data: ProductDto[];
  totalCount: number;
}

export interface ProductFilter {
  searchTerm?: string;
  brandIds?: number[];
  pageNumber: number;
  pageSize: number;
}
