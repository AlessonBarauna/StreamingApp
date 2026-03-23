import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Content {
  id: string;
  title: string;
  description: string;
  type: string;
  thumbnailUrl: string;
  backdropUrl?: string;
  releaseYear: number;
  durationMinutes?: number;
  ageRating: string;
  status: string;
  hlsManifestUrl?: string;
  categoryName: string;
  categoryId: string;
  viewCount: number;
  isFeatured: boolean;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  limit: number;
}

export interface Category {
  id: string;
  name: string;
  slug: string;
  iconName: string;
}

export interface Progress {
  progressSeconds: number;
  totalSeconds: number;
  percentComplete: number;
  isCompleted: boolean;
}

@Injectable({ providedIn: 'root' })
export class ContentService {
  constructor(private http: HttpClient) {}

  getAll(page = 1, limit = 20, categoryId?: string, search?: string, type?: string): Observable<PagedResult<Content>> {
    let params = new HttpParams().set('page', page).set('limit', limit);
    if (categoryId) params = params.set('category', categoryId);
    if (search) params = params.set('search', search);
    if (type) params = params.set('type', type);
    return this.http.get<PagedResult<Content>>('/api/content', { params });
  }

  getFeatured(): Observable<Content[]> {
    return this.http.get<Content[]>('/api/content/featured');
  }

  getTrending(): Observable<Content[]> {
    return this.http.get<Content[]>('/api/content/trending');
  }

  getNewReleases(): Observable<Content[]> {
    return this.http.get<Content[]>('/api/content/new-releases');
  }

  getById(id: string): Observable<Content> {
    return this.http.get<Content>(`/api/content/${id}`);
  }

  search(q: string): Observable<PagedResult<Content>> {
    return this.http.get<PagedResult<Content>>(`/api/content/search?q=${q}`);
  }

  getProgress(contentId: string): Observable<Progress> {
    return this.http.get<Progress>(`/api/stream/${contentId}/progress`);
  }

  saveProgress(contentId: string, seconds: number, total: number): Observable<any> {
    return this.http.post(`/api/stream/${contentId}/progress`, { seconds, total });
  }

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>('/api/categories');
  }

  addToWatchlist(contentId: string): Observable<any> {
    return this.http.post(`/api/user/watchlist/${contentId}`, {});
  }

  removeFromWatchlist(contentId: string): Observable<any> {
    return this.http.delete(`/api/user/watchlist/${contentId}`);
  }

  rate(contentId: string, isLiked: boolean): Observable<any> {
    return this.http.post(`/api/user/rating/${contentId}`, { isLiked });
  }
}
