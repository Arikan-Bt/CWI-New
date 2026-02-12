import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ImageService {
  resolveImageUrl(path: string | undefined): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    return `${environment.apiUrl}/${path}`;
  }

  getCDNUrl(sku: string | undefined): string {
    if (!sku) return '';
    const cdn = environment.cdnUrl || '';
    return `${cdn}/ProductImages/${sku}.jpg`;
  }

  /**
   * Determines the image URL to display based on the product state.
   * Logic:
   * 1. If fallbackLevel is 0 (default), try system image (if exists).
   * 2. If fallbackLevel is 1, try CDN image.
   * 3. If fallbackLevel is > 1, return empty (which usually triggers placeholder in UI).
   *
   * Note: The component must handle the (error) event to increment fallbackLevel.
   */
  getImageUrl(product: { imageUrl?: string; sku: string; imageFallback?: number }): string {
    const fallback = product.imageFallback || 0;

    // Level 0: Try System Image
    if (fallback === 0) {
      if (product.imageUrl) {
        return this.resolveImageUrl(product.imageUrl);
      }
      // If no system image, treat as if error occurred and go to next level immediately
      // But we can't mutate state here easily without side effects.
      // Better approach: If no imageUrl, return CDN url immediately as effectively level 1.
      return this.getCDNUrl(product.sku);
    }

    // Level 1: Try CDN Image
    // (If we came here via error from Level 0, or if we skipped Level 0 above)
    // However, if we returned CDN url in Level 0 block (because !imageUrl),
    // and that fails, the error handler will bump to Level 1.
    // So if fallback == 1, it means the FIRST attempt failed.
    // IF the first attempt was system image, now we try CDN.
    // IF the first attempt was CDN (because no system image), now we are actually at Level 2 (Placeholder).

    // Let's refine the logic to be state-driven explicitly.
    // State 0: Prefer System.
    // State 1: Prefer CDN.
    // State 2: Failed.

    if (fallback === 1) {
      // If we are here, it means whatever we tried at 0 failed.
      // Check if we already tried CDN at 0?
      // If product.imageUrl existed, we tried system. So now try CDN.
      if (product.imageUrl) {
        return this.getCDNUrl(product.sku);
      }
      // If product.imageUrl did NOT exist, we returned CDN at 0. So now we are done.
      return '';
    }

    return '';
  }

  /**
   * Helper to handle the error event.
   * Returns the new fallback level.
   */
  handleImageError(currentFallback: number): number {
    return currentFallback + 1;
  }
}
