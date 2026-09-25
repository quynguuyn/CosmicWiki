import { PagedResult } from '../../../core/models/paged-result.model';

export interface Story {
  id: string;
  category: string;
  era: string;
  code: string;
  title: string;
  slug: string;
  description: string;
  content: string;
  author: string;
  readTimeMinutes: number;
  imageUrl?: string | null;
  imageAlt?: string | null;
  status: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export type StoriesResult = PagedResult<Story>;
