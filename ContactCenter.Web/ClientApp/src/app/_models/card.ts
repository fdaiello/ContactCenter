import { Contact } from './contact';
import { BoardField } from './boardField';

export interface Card {
    id: number,
    contactId: string,
    contact: Contact[],
    stageId: number,
    color?: string,
    cardFieldValues: BoardField[],
    order: number,
    createdDate: Date,
    updatedDate: Date
}