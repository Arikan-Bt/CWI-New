export interface ImportOrderRequest {
  fileContent: string;
  projectType: 'CWI' | 'AWC';
  customerCode: string;
  orderType: string;
}

export interface ImportResultDto {
  totalRows: number;
  successCount: number;
  errorCount: number;
  warnings: ImportWarningDto[];
  errors: ImportErrorDto[];
  previewItems: ExcelOrderItemDto[];
  batchId?: string;
}

export interface ImportErrorDto {
  row: number;
  message: string;
}

export interface ImportWarningDto {
  row: number;
  message: string;
}

export interface ExcelOrderItemDto {
  row: number;
  itemCode: string;
  quantity: number;
  currAccCode?: string;
  description?: string;
}

export interface ValidateExcelResultDto {
  isValid: boolean;
  totalRows: number;
  errorCount: number;
  details: ImportErrorDto[];
}
