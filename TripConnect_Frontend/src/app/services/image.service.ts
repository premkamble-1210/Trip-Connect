import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ImageUploadResponse {
  fileId: string;
  imageUrl: string;
  publicUrl: string;
  uploadedAt: string; // DateTime from backend
  success: boolean;
  message: string;
  fileSize: number;
  width?: number; // nullable int from backend
  height?: number; // nullable int from backend
  fileName: string;
}

@Injectable({ providedIn: 'root' })
export class ImageService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/api/image`;

  uploadTripImage(tripId: number, file: File): Observable<ImageUploadResponse> {
    const formData = new FormData();
    formData.append('imageFile', file);
    
    return this.http.post<ImageUploadResponse>(`${this.base}/upload-trip/${tripId}`, formData);
  }
}