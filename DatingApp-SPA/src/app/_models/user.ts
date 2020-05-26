import { Photo } from './photo';

export interface User {
    id: number;
    userName: string;
    knownAs: string;
    email: string;
    age: number;
    dateOfBirth: string;
    gender: string;
    created: Date;
    lastActive: any;
    photoUrl: string;
    city: string;
    country: string;
    emailConfirmed: boolean;
    interests?: string;
    introduction?: string;
    lookingFor?: string;
    photos?: Photo[];
    roles?: string[];
}
