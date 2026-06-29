import { type GalleryItem } from '../types';

/**
 * Photography categories. The first featured tile spans the gallery; the rest
 * flow into the grid. Mark another item `featured` to widen it.
 */
export const galleryItems: GalleryItem[] = [
  { title: 'Actionfotos', caption: 'Stævnebilleder i høj kvalitet', featured: true },
  { title: 'Portræt', caption: 'Studio' },
  { title: 'Træning', caption: 'Dagligdag' },
  { title: 'Podie', caption: 'Resultater' },
  { title: 'Hold', caption: 'Klub & team' },
];
