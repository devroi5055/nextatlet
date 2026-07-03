/**
 * Turns a display name into a URL-safe slug suggestion (Danish-aware:
 * æ→ae, ø→oe, å→aa). The server still validates uniqueness + reserved words,
 * so this is a convenience suggestion only.
 */
export const slugify = (input: string): string =>
  input
    .toLowerCase()
    .trim()
    .replace(/æ/g, 'ae')
    .replace(/ø/g, 'oe')
    .replace(/å/g, 'aa')
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '') // strip remaining diacritics
    .replace(/[^a-z0-9]+/g, '-') // non-alphanumerics → hyphen
    .replace(/^-+|-+$/g, ''); // trim leading/trailing hyphens
