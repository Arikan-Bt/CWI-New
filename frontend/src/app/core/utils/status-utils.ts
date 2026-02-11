export type StatusSeverity =
  | 'success'
  | 'info'
  | 'warn'
  | 'danger'
  | 'secondary'
  | 'contrast'
  | undefined;

/**
 * Sipariş durumlarına göre renk (severity) döner.
 * Tüm projede tutarlı bir görünüm sağlamak için kullanılır.
 *
 * @param status Sipariş durumu (string)
 * @returns PrimeNG Tag severity değeri
 */
export function getOrderStatusSeverity(status: string | null | undefined): StatusSeverity {
  if (!status) return 'secondary';

  const normalizedStatus = status.trim().toUpperCase().replace(/\s+/g, '');

  switch (normalizedStatus) {
    case 'APPROVED':
    case 'DELIVERED':
      return 'success'; // Yeşil

    case 'SHIPPED':
      return 'info'; // Mavi

    case 'PENDING':
    case 'PREORDER':
      return 'warn'; // Turuncu

    case 'PACKEDANDWAITINGSHIPMENT':
    case 'PACKED&WAITINGSHIPMENT':
      return 'info'; // Mavi (veya özel bir renk istenirse değiştirilebilir)

    case 'CANCELED':
    case 'CANCELLED':
      return 'danger'; // Kırmızı

    case 'DRAFT':
      return 'secondary'; // Gri

    default:
      return 'secondary'; // Tanımsız durumlar için varsayılan gri
  }
}
