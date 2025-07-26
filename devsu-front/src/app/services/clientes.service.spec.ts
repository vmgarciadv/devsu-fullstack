import { TestBed } from '@angular/core/testing';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ClientesService, PaginatedResponse } from './clientes.service';
import { Cliente } from '../models/cliente.model';
import { environment } from '../../environments/environment';

describe('ClientesService', () => {
  let service: ClientesService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/clientes`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ClientesService
      ]
    });
    service = TestBed.inject(ClientesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // Verifica que no haya solicitudes pendientes
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getClientes', () => {
    it('debería obtener clientes paginados sin query de búsqueda', () => {
      const mockResponse: PaginatedResponse<Cliente> = {
        Data: [
          { 
            ClienteId: 1,
            Nombre: 'Juan Pérez',
            Genero: 'M',
            Edad: 30,
            Identificacion: '12345678',
            Direccion: 'Calle 123',
            Telefono: '555-1234',
            Estado: true
          }
        ],
        TotalRecords: 1,
        TotalPages: 1,
        PageNumber: 1,
        PageSize: 15,
        HasPreviousPage: false,
        HasNextPage: false
      };

      service.getClientes(1, 15).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}?pageNumber=1&pageSize=15`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('debería obtener clientes paginados con query de búsqueda', () => {
      const mockResponse: PaginatedResponse<Cliente> = {
        Data: [],
        TotalRecords: 0,
        TotalPages: 0,
        PageNumber: 1,
        PageSize: 15,
        HasPreviousPage: false,
        HasNextPage: false
      };

      service.getClientes(1, 15, 'Juan').subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}?pageNumber=1&pageSize=15&q=Juan`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('debería ignorar espacios en blanco en la query de búsqueda', () => {
      const mockResponse: PaginatedResponse<Cliente> = {
        Data: [],
        TotalRecords: 0,
        TotalPages: 0,
        PageNumber: 1,
        PageSize: 15,
        HasPreviousPage: false,
        HasNextPage: false
      };

      service.getClientes(1, 15, '  ').subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}?pageNumber=1&pageSize=15`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('getAllClientes', () => {
    it('debería obtener todos los clientes', () => {
      const mockClientes: Cliente[] = [
        { 
          ClienteId: 1,
          Nombre: 'Juan Pérez',
          Genero: 'M',
          Edad: 30,
          Identificacion: '12345678',
          Direccion: 'Calle 123',
          Telefono: '555-1234',
          Estado: true
        },
        { 
          ClienteId: 2,
          Nombre: 'María García',
          Genero: 'F',
          Edad: 25,
          Identificacion: '87654321',
          Direccion: 'Av. Principal 456',
          Telefono: '555-5678',
          Estado: true
        }
      ];

      service.getAllClientes().subscribe(clientes => {
        expect(clientes.length).toBe(2);
        expect(clientes).toEqual(mockClientes);
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('GET');
      req.flush(mockClientes);
    });
  });

  describe('createCliente', () => {
    it('debería crear un nuevo cliente', () => {
      const nuevoCliente: Cliente = {
        Nombre: 'Nuevo Cliente',
        Genero: 'M',
        Edad: 35,
        Identificacion: '11111111',
        Direccion: 'Nueva Dirección',
        Telefono: '555-9999',
        Estado: true
      };

      const clienteCreado = { ...nuevoCliente, ClienteId: 3 };

      service.createCliente(nuevoCliente).subscribe(cliente => {
        expect(cliente).toEqual(clienteCreado);
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(nuevoCliente);
      req.flush(clienteCreado);
    });
  });

  describe('updateCliente', () => {
    it('debería actualizar un cliente existente', () => {
      const clienteId = 1;
      const clienteActualizado: Partial<Cliente> = {
        Nombre: 'Juan Pérez Actualizado',
        Direccion: 'Nueva Dirección 789',
        Telefono: '555-0000'
      };

      const clienteCompleto: Cliente = {
        ClienteId: 1,
        Nombre: 'Juan Pérez Actualizado',
        Genero: 'M',
        Edad: 30,
        Identificacion: '12345678',
        Direccion: 'Nueva Dirección 789',
        Telefono: '555-0000',
        Estado: true
      };

      service.updateCliente(clienteId, clienteActualizado).subscribe(cliente => {
        expect(cliente).toEqual(clienteCompleto);
      });

      const req = httpMock.expectOne(`${apiUrl}/${clienteId}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(clienteActualizado);
      req.flush(clienteCompleto);
    });
  });

  describe('patchCliente', () => {
    it('debería actualizar parcialmente un cliente', () => {
      const clienteId = 1;
      const actualizacionParcial: Partial<Cliente> = {
        Estado: false
      };

      const clienteActualizado: Cliente = {
        ClienteId: 1,
        Nombre: 'Juan Pérez',
        Genero: 'M',
        Edad: 30,
        Identificacion: '12345678',
        Direccion: 'Calle 123',
        Telefono: '555-1234',
        Estado: false
      };

      service.patchCliente(clienteId, actualizacionParcial).subscribe(cliente => {
        expect(cliente).toEqual(clienteActualizado);
      });

      const req = httpMock.expectOne(`${apiUrl}/${clienteId}`);
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body).toEqual(actualizacionParcial);
      req.flush(clienteActualizado);
    });
  });

  describe('deleteCliente', () => {
    it('debería eliminar un cliente', () => {
      const clienteId = 1;

      service.deleteCliente(clienteId).subscribe(response => {
        expect(response).toBeUndefined();
      });

      const req = httpMock.expectOne(`${apiUrl}/${clienteId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });
});
