export interface SearchResultResponse {
    id: string;
    sourceId: string;
    sourceType: string;
    name: string;
    description: string | null;
    imageUrl: string | null;
}