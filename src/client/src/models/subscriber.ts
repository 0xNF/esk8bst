import { Match } from 'src/models/Match';

interface Subscriber {
    email: string;
    matches: Match[];
}

export { Subscriber }