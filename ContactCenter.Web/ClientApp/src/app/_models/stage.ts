import { Card } from './card';

export interface Stage {
    id: number;
    boardId: number;
    name: string;
    label: string;
    order: number;
    cards: Card[];
}
