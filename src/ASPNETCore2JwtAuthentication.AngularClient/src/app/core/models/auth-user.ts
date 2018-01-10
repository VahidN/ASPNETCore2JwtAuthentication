export interface AuthUser {
  userId: string;
  userName: string;
  displayName: string;
  roles: string[] | null;
}
