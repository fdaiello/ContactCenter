const enum FieldType {
  Integer,
  Decimal,
  Money,
  Date,
  Time,
  DateTime,
  Text,
  TextArea,
  DataList,
  Image,
  Document
}
export interface Field {
  id: number,
  isGlobal: boolean,
  label: string,
  fieldType: FieldType,
  dataListId?: number,
}