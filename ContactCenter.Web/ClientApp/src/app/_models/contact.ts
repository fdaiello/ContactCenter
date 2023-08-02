export interface Contact {
  id: string,
  name: string,
  fullName: string,
  nickName?: string,
  mobilePhone?: string,
  email: string,
  firstActivity: Date,
  lastActivity: Date,
  lastText: string,
  applicationUserId: string,
  pictureFileName?: string,
  contactFieldValues: any,
}