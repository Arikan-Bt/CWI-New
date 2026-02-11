export interface PaymentListDto {
  paymentId: number;
  paymentNumber: string;
  paymentDate: string;
  currAccCode: string;
  currAccDescription: string;
  amount: number;
  currencyCode: string;
  paymentType: string;
  paymentTypeDescription: string;
  status: string;
  statusDescription: string;
}

export interface PaymentFilterDto {
  startDate?: string;
  endDate?: string;
  status?: number;
  currAccCode?: string;
  page: number;
  pageSize: number;
  projectType: 'CWI' | 'AWC';
}

export interface CreatePaymentDto {
  amount: number;
  currencyCode: string;
  paymentType: number;
  description?: string;
  bankName?: string;
  documentDate?: string;
  documentNumber?: string;
  projectType: 'CWI' | 'AWC';
}

export interface PaymentDetailDto {
  paymentId: number;
  paymentNumber: string;
  paymentDate: string;
  currAccCode: string;
  currAccDescription: string;
  amount: number;
  currencyCode: string;
  paymentType: number;
  paymentTypeDescription: string;
  status: number;
  statusDescription: string;
  description?: string;
  bankName?: string;
  documentDate?: string;
  documentNumber?: string;
  receiptUrl?: string; // Backend tarafında henüz URL dönüşü yoktu ama detayda eklenebilir
}

export interface UploadReceiptDto {
  fileName: string;
  contentType: string;
  fileContent: string;
}

export enum PaymentType {
  BankTransfer = 1,
  CreditCard = 2,
  Check = 3,
  Cash = 4,
}

export enum PaymentStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
}
