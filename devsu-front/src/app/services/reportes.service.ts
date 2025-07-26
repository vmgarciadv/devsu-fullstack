import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Reporte } from '../models/reporte.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReportesService {
  private apiUrl = `${environment.apiUrl}/reportes`;

  constructor(private http: HttpClient) { }

  getReportes(cliente: string, fecha?: string, fechaInicio?: string, fechaFin?: string): Observable<Reporte[]> {
    let params = new HttpParams();
    params = params.set('cliente', cliente);
    
    // Ajustar segun zona horaria del usuario
    const timezoneOffset = -(new Date().getTimezoneOffset() / 60);
    params = params.set('timezone', timezoneOffset.toString());
    
    if (fecha) {
      params = params.set('fecha', fecha);
    } else if (fechaInicio && fechaFin) {
      params = params.set('fechaInicio', fechaInicio);
      params = params.set('fechaFin', fechaFin);
    }
    
    return this.http.get<Reporte[]>(this.apiUrl, { params });
  }
}