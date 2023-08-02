export interface Filter {
    id: number,
    groupId?: number,
    applicationUserId?: string,
    boardId: number;
    title: string,
    jsonFilter: string,
}
