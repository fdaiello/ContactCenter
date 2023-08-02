import { BoardField } from './boardField';
import { Stage } from './stage';

export interface Board {
    id: number,
    name: string,
    label: string,
    cardName?: string,
    groupId?: number,
    allowMultipleCardsForSameContact: boolean,
    departmentId?: string,
    applicationUserId?: string,
    stages: Stage[],
    boardFields: BoardField[]
}