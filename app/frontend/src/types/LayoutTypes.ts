export type ElementType = 'text' | 'image' | 'qrcode' | 'barcode' | 'formula';
export type TextSource = 'fixed' | 'field' | 'formula';
export type BarcodeType = 'ean13' | 'code128';

export interface PageConfig {
  marginMm: number;
  columns: number;
  rows: number;
  gapMm: number;
  cardWidthMm: number;
  cardHeightMm: number;
}

export interface BaseElement {
  id: string;
  type: ElementType;
  xMm: number;
  yMm: number;
  widthMm: number;
  heightMm: number;
}

export interface TextElement extends BaseElement {
  type: 'text';
  source: TextSource;
  fieldName?: string;
  formula?: string;
  previewText: string;
  fontSize: number;
  bold: boolean;
  align: 'left' | 'center' | 'right';
  color: string;
}

export interface ImageElement extends BaseElement {
  type: 'image';
  source: 'fixed' | 'field';
  imagePath?: string;
  fieldName?: string;
}

export interface QRCodeElement extends BaseElement {
  type: 'qrcode';
  source: 'fixed' | 'field' | 'formula';
  value?: string;
  fieldName?: string;
  formula?: string;
}

export interface BarcodeElement extends BaseElement {
  type: 'barcode';
  barcodeType: BarcodeType;
  source: 'fixed' | 'field' | 'formula';
  value?: string;
  fieldName?: string;
  formula?: string;
}

export interface FormulaElement extends BaseElement {
  type: 'formula';
  formula: string;
  previewText: string;
  fontSize: number;
  bold: boolean;
  align: 'left' | 'center' | 'right';
  color: string;
}

export type LayoutElement = TextElement | ImageElement | QRCodeElement | BarcodeElement | FormulaElement;

export interface LayoutData {
  page: PageConfig;
  elements: LayoutElement[];
}

export const AVAILABLE_FIELDS = [
  { value: 'codigo', label: 'Código' },
  { value: 'descricao', label: 'Descrição' },
  { value: 'categoria', label: 'Categoria' },
  { value: 'valor', label: 'Valor' },
  { value: 'yield', label: 'Rendimento' },
  { value: 'quantidadeimpresa', label: 'Quantidade Impressa' },
];

export const DEFAULT_LAYOUT: LayoutData = {
  page: {
    marginMm: 10,
    columns: 2,
    rows: 2,
    gapMm: 5,
    cardWidthMm: 80,
    cardHeightMm: 120,
  },
  elements: [],
};
