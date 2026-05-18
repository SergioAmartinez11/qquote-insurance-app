import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface PolicyResponse {
  id: string;
  quoteId: string;
  vehicleMake: string;
  vehicleModel: string;
  vehicleYear: number;
  coverageType: string;
  monthlyPremium: number;
  currency: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class PolicyService {
  private readonly apiUrl = 'http://localhost:5001/api/policies';

  constructor(private http: HttpClient) {}

  getMyPolicies(): Observable<PolicyResponse[]> {
    return this.http.get<PolicyResponse[]>(this.apiUrl);
  }

  convertFromQuote(quoteId: string): Observable<{ policyId: string }> {
    return this.http.post<{ policyId: string }>(`${this.apiUrl}/${quoteId}/convert`, {});
  }
}
