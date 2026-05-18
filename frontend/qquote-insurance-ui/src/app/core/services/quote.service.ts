import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CreateQuoteRequest {
  vehicleMake: string;
  vehicleModel: string;
  vehicleYear: number;
  coverageType: string;
  currency: string;
}

export interface QuoteResponse {
  id: string;
  vehicleMake: string;
  vehicleModel: string;
  vehicleYear: number;
  coverageType: string;
  monthlyPremium: number;
  currency: string;
  riskLevel: string;
  riskExplanation: string;
  status: string;
  createdAt: string;
  expiresAt: string;
}

@Injectable({ providedIn: 'root' })
export class QuoteService {
  private readonly apiUrl = `${environment.apiUrl}/api/quotes`;

  constructor(private http: HttpClient) {}

  getMyQuotes(): Observable<QuoteResponse[]> {
    return this.http.get<QuoteResponse[]>(this.apiUrl);
  }

  getById(id: string): Observable<QuoteResponse> {
    return this.http.get<QuoteResponse>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateQuoteRequest): Observable<QuoteResponse> {
    return this.http.post<QuoteResponse>(this.apiUrl, request);
  }
}
