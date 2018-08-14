interface Match {
    comapnies?: string[];
    price?: number;
    currency?: string,
    product?: string;
}

interface FirebaseMatch {
    owner: string; // PK, email
    matches: Match[];
}

export { FirebaseMatch, Match }