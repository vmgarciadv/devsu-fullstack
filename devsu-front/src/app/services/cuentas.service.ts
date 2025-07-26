import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Cuenta } from '../models/cuenta.model';
import { environment } from '../../environments/environment';
import { PaginatedResponse } from './clientes.service';

@Injectable({
  providedIn: 'root'
})
export class CuentasService {
  private apiUrl = `${environment.apiUrl}/cuentas`;

  constructor(private http: HttpClient) { }

  getCuentas(pageNumber: number = 1, pageSize: number = 15, searchQuery?: string): Observable<PaginatedResponse<Cuenta>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    
    if (searchQuery && searchQuery.trim()) {
      params = params.set('q', searchQuery.trim());
    }
    
    return this.http.get<PaginatedResponse<Cuenta>>(this.apiUrl, { params });
  }
  
  getAllCuentas(): Observable<Cuenta[]> {
    return this.http.get<Cuenta[]>(this.apiUrl);
  }

  createCuenta(cuenta: Cuenta): Observable<Cuenta> {
    return this.http.post<Cuenta>(this.apiUrl, cuenta);
  }

  updateCuenta(id: number, cuenta: Partial<Cuenta>): Observable<Cuenta> {
    return this.http.put<Cuenta>(`${this.apiUrl}/${id}`, cuenta);
  }

  patchCuenta(id: number, cuenta: Partial<Cuenta>): Observable<Cuenta> {
    return this.http.patch<Cuenta>(`${this.apiUrl}/${id}`, cuenta);
  }

  deleteCuenta(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}