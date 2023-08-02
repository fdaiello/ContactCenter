import { Field } from './field';

export interface BoardField {
  id: number,
  groupId: number,
  fieldId: number,
  field: Field,
  boardId: number,
  enabled: boolean,
  value: string
}