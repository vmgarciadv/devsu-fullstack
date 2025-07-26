import { TestBed } from '@angular/core/testing';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { CuentasService } from './cuentas.service';
import { PaginatedResponse } from './clientes.service';
import { Cuenta } from '../models/cuenta.model';
import { environment } from '../../environments/environment';

describe('CuentasService', () => {
  let service: CuentasService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/cuentas`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        CuentasService
      ]
    });
    service = TestBed.inject(CuentasService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // Verifica que no haya solicitudes pendientes
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getCuentas', () => {
    it('debería obtener cuentas paginadas sin query de búsqueda', () => {
      const mockResponse: PaginatedResponse<Cuenta> = {
        Data: [
          { 
            CuentaId: 1,
            NumeroCuenta: '1234567890',
            TipoCuenta: 'Ahorros',
            SaldoInicial: 1000,
            Estado: true,
            ClienteId: 1,
            NombreCliente: 'Juan Pérez'
          }
        ],
        TotalRecords: 1,
        TotalPages: 1,
        PageNumber: 1,
        PageSize: 15,
        HasPreviousPage: false,
        HasNextPage: false
      };

      service.getCuentas(1, 15).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}?pageNumber=1&pageSize=15`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('debería obtener cuentas paginadas con query de búsqueda', () => {
      const mockResponse: PaginatedResponse<Cuenta> = {
        Data: [],
        TotalRecords: 0,
        TotalPages: 0,
        PageNumber: 1,
        PageSize: 15,
        HasPreviousPage: false,
        HasNextPage: false
      };

      service.getCuentas(1, 15, '1234').subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}?pageNumber=1&pageSize=15&q=1234`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('getAllCuentas', () => {
    it('debería obtener todas las cuentas', () => {
      const mockCuentas: Cuenta[] = [
        { 
          CuentaId: 1,
          NumeroCuenta: '1234567890',
          TipoCuenta: 'Ahorros',
          SaldoInicial: 1000,
            Estado: true,
          ClienteId: 1,
          NombreCliente: 'Juan Pérez'
        },
        { 
          CuentaId: 2,
          NumeroCuenta: '0987654321',
          TipoCuenta: 'Corriente',
          SaldoInicial: 2000,
          Estado: true,
          ClienteId: 2,
          NombreCliente: 'María García'
        }
      ];

      service.getAllCuentas().subscribe(cuentas => {
        expect(cuentas.length).toBe(2);
        expect(cuentas).toEqual(mockCuentas);
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('GET');
      req.flush(mockCuentas);
    });
  });

  describe('createCuenta', () => {
    it('debería crear una nueva cuenta', () => {
      const nuevaCuenta: Cuenta = {
        NumeroCuenta: '1111111111',
        TipoCuenta: 'Ahorros',
        SaldoInicial: 500,
        Estado: true,
        ClienteId: 3,
        NombreCliente: 'Nuevo Cliente'
      };

      const cuentaCreada = { ...nuevaCuenta, CuentaId: 3 };

      service.createCuenta(nuevaCuenta).subscribe(cuenta => {
        expect(cuenta).toEqual(cuentaCreada);
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(nuevaCuenta);
      req.flush(cuentaCreada);
    });
  });

  describe('updateCuenta', () => {
    it('debería actualizar una cuenta existente', () => {
      const cuentaId = 1;
      const cuentaActualizada: Partial<Cuenta> = {
        Estado: false
      };

      const cuentaCompleta: Cuenta = {
        CuentaId: 1,
        NumeroCuenta: '1234567890',
        TipoCuenta: 'Ahorros',
        SaldoInicial: 1000,
        Estado: false,
        ClienteId: 1,
        NombreCliente: 'Juan Pérez'
      };

      service.updateCuenta(cuentaId, cuentaActualizada).subscribe(cuenta => {
        expect(cuenta).toEqual(cuentaCompleta);
      });

      const req = httpMock.expectOne(`${apiUrl}/${cuentaId}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(cuentaActualizada);
      req.flush(cuentaCompleta);
    });
  });

  describe('patchCuenta', () => {
    it('debería actualizar parcialmente una cuenta', () => {
      const cuentaId = 1;
      const actualizacionParcial: Partial<Cuenta> = {
        Estado: false
      };

      const cuentaActualizada: Cuenta = {
        CuentaId: 1,
        NumeroCuenta: '1234567890',
        TipoCuenta: 'Ahorros',
        SaldoInicial: 1000,
        Estado: false,
        ClienteId: 1,
        NombreCliente: 'Juan Pérez'
      };

      service.patchCuenta(cuentaId, actualizacionParcial).subscribe(cuenta => {
        expect(cuenta).toEqual(cuentaActualizada);
      });

      const req = httpMock.expectOne(`${apiUrl}/${cuentaId}`);
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body).toEqual(actualizacionParcial);
      req.flush(cuentaActualizada);
    });
  });

  describe('deleteCuenta', () => {
    it('debería eliminar una cuenta', () => {
      const cuentaId = 1;

      service.deleteCuenta(cuentaId).subscribe(response => {
        expect(response).toBeUndefined();
      });

      const req = httpMock.expectOne(`${apiUrl}/${cuentaId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });
});