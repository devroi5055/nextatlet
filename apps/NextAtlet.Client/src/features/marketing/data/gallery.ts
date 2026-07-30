import { type GalleryItem } from '../types';

/**
 * Photography categories. The first featured tile spans the gallery; the rest
 * flow into the grid. Mark another item `featured` to widen it. Text is resolved
 * from the `Photography.gallery.<key>` message namespace.
 */
export const galleryItems: GalleryItem[] = [
  { key: 'action', featured: true },
  { key: 'portrait' },
  { key: 'training' },
  { key: 'podium' },
  { key: 'team' },
];
