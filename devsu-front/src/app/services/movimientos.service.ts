import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Movimiento } from '../models/movimiento.model';
import { environment } from '../../environments/environment';
import { PaginatedResponse } from './clientes.service';

@Injectable({
  providedIn: 'root'
})
export class MovimientosService {
  private apiUrl = `${environment.apiUrl}/movimientos`;

  constructor(private http: HttpClient) { }

  getMovimientos(pageNumber: number = 1, pageSize: number = 15, searchQuery?: string): Observable<PaginatedResponse<Movimiento>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    
    const timezoneOffset = -(new Date().getTimezoneOffset() / 60);
    params = params.set('timezone', timezoneOffset.toString());
    
    if (searchQuery && searchQuery.trim()) {
      params = params.set('q', searchQuery.trim());
    }
    
    return this.http.get<PaginatedResponse<Movimiento>>(this.apiUrl, { params });
  }
  
  getAllMovimientos(): Observable<Movimiento[]> {
    return this.http.get<Movimiento[]>(this.apiUrl);
  }

  createMovimiento(movimiento: Movimiento): Observable<Movimiento> {
    return this.http.post<Movimiento>(this.apiUrl, movimiento);
  }

  updateMovimiento(id: number, movimiento: Partial<Movimiento>): Observable<Movimiento> {
    return this.http.put<Movimiento>(`${this.apiUrl}/${id}`, movimiento);
  }

  patchMovimiento(id: number, movimiento: Partial<Movimiento>): Observable<Movimiento> {
    return this.http.patch<Movimiento>(`${this.apiUrl}/${id}`, movimiento);
  }

  deleteMovimiento(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}