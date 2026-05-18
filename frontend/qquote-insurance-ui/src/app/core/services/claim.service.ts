import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface CreateClaimRequest {
  policyId: string;
  incidentDate: string;
  description: string;
}

export interface ClaimResponse {
  id: string;
  policyId: string;
  incidentDate: string;
  description: string;
  status: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class ClaimService {
  private readonly apiUrl = 'http://localhost:5001/api/claims';

  constructor(private http: HttpClient) {}

  getByPolicy(policyId: string): Observable<ClaimResponse[]> {
    return this.http.get<ClaimResponse[]>(`${this.apiUrl}/policy/${policyId}`);
  }

  file(request: CreateClaimRequest): Observable<ClaimResponse> {
    return this.http.post<ClaimResponse>(this.apiUrl, request);
  }
}
