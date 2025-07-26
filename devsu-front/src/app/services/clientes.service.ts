import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Cliente } from '../models/cliente.model';
import { environment } from '../../environments/environment';

export interface PaginatedResponse<T> {
  Data: T[];
  TotalRecords: number;
  TotalPages: number;
  PageNumber: number;
  PageSize: number;
  HasPreviousPage: boolean;
  HasNextPage: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ClientesService {
  private apiUrl = `${environment.apiUrl}/clientes`;

  constructor(private http: HttpClient) { }

  getClientes(pageNumber: number = 1, pageSize: number = 15, searchQuery?: string): Observable<PaginatedResponse<Cliente>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    
    if (searchQuery && searchQuery.trim()) {
      params = params.set('q', searchQuery.trim());
    }
    
    return this.http.get<PaginatedResponse<Cliente>>(this.apiUrl, { params });
  }
  
  getAllClientes(): Observable<Cliente[]> {
    return this.http.get<Cliente[]>(this.apiUrl);
  }

  createCliente(cliente: Cliente): Observable<Cliente> {
    return this.http.post<Cliente>(this.apiUrl, cliente);
  }

  updateCliente(id: number, cliente: Partial<Cliente>): Observable<Cliente> {
    return this.http.put<Cliente>(`${this.apiUrl}/${id}`, cliente);
  }

  patchCliente(id: number, cliente: Partial<Cliente>): Observable<Cliente> {
    return this.http.patch<Cliente>(`${this.apiUrl}/${id}`, cliente);
  }

  deleteCliente(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
