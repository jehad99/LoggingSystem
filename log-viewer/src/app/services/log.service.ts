import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LogResponse } from '../models/log.model';

@Injectable({
  providedIn: 'root'
})
export class LogService {
  private apiUrl = 'api/logs';

  constructor(private http: HttpClient) {}

  getLogs(startDate: Date, endDate: Date, page: number = 1, pageSize: number = 10): Observable<LogResponse> {
    return this.http.get<LogResponse>(`${this.apiUrl}/range`, {
      params: {
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString(),
        page: page.toString(),
        pageSize: pageSize.toString()
      }
    });
  }
}