import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Result } from '../models/auth.models';
import { environment } from '../../../environments/environment';
import {
  CreateStockAdjustmentRequest,
  CreateStockAdjustmentResponse,
} from '../models/stock-adjustment.models';

@Injectable({
  providedIn: 'root',
})
export class InventoryService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/inventory`;

  /**
   * Stok düzenleme işlemini başlatır.
   * @param request Düzenleme bilgileri ve Excel dosyası
   */
  createStockAdjustment(
    request: CreateStockAdjustmentRequest,
  ): Observable<Result<CreateStockAdjustmentResponse>> {
    const formData = new FormData();
    formData.append('adjustmentDate', request.adjustmentDate.toISOString());
    formData.append('description', request.description);
    formData.append('file', request.file);

    return this.http.post<Result<CreateStockAdjustmentResponse>>(
      `${this.apiUrl}/stock-adjustment`,
      formData,
    );
  }

  getWarehouses(): Observable<Result<WarehouseDto[]>> {
    return this.http.get<Result<WarehouseDto[]>>(`${this.apiUrl}/warehouses`);
  }

  checkOrderStock(orderId: number): Observable<Result<ProductStockStatusDto[]>> {
    return this.http.get<Result<ProductStockStatusDto[]>>(
      `${this.apiUrl}/check-order-stock/${orderId}`,
    );
  }

  downloadTemplate(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/stock-adjustment/template`, {
      responseType: 'blob',
    });
  }
}

export interface WarehouseDto {
  id: number;
  name: string;
  code: string;
}

export interface ProductStockStatusDto {
  productCode: string;
  productName: string;
  requiredQty: number;
  warehouses: WarehouseStockDto[];
  hasMultipleWarehouses: boolean;
  hasSufficientStock: boolean;
}

export interface WarehouseStockDto {
  warehouseId: number;
  warehouseName: string;
  availableQty: number;
}
