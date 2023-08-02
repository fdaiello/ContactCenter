export class User {
    id: string;
    jobTitle: string;
    fullName: string;
    nickName: string;
    departmentId: bigint;
    createdDate: Date;
    updatedDate: Date;
    lastActivity: Date;
    lastText: string;
    webPushedId: string;
    token?: string;
}