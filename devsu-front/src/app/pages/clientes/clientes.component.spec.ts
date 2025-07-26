import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { ClientesComponent } from './clientes.component';
import { ClientesService, PaginatedResponse } from '../../services/clientes.service';
import { NotificationService } from '../../services/notification.service';
import { Cliente } from '../../models/cliente.model';
import { FormsModule } from '@angular/forms';

describe('ClientesComponent', () => {
  let component: ClientesComponent;
  let fixture: ComponentFixture<ClientesComponent>;
  let clientesService: jasmine.SpyObj<ClientesService>;
  let notificationService: jasmine.SpyObj<NotificationService>;

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

  const mockPaginatedResponse: PaginatedResponse<Cliente> = {
    Data: mockClientes,
    TotalRecords: 2,
    TotalPages: 1,
    PageNumber: 1,
    PageSize: 15,
    HasPreviousPage: false,
    HasNextPage: false
  };

  beforeEach(async () => {
    const clientesServiceSpy = jasmine.createSpyObj('ClientesService', [
      'getClientes',
      'createCliente',
      'updateCliente',
      'patchCliente',
      'deleteCliente'
    ]);
    
    const notificationServiceSpy = jasmine.createSpyObj('NotificationService', 
      ['showSuccess', 'showError', 'close'],
      { notification$: of({ visible: false, type: 'success', message: '' }) }
    );

    await TestBed.configureTestingModule({
      imports: [ClientesComponent, FormsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ClientesService, useValue: clientesServiceSpy },
        { provide: NotificationService, useValue: notificationServiceSpy }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ClientesComponent);
    component = fixture.componentInstance;
    clientesService = TestBed.inject(ClientesService) as jasmine.SpyObj<ClientesService>;
    notificationService = TestBed.inject(NotificationService) as jasmine.SpyObj<NotificationService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('debería cargar clientes al inicializar', () => {
      spyOn(component, 'loadClientes');
      
      component.ngOnInit();
      
      expect(component.loadClientes).toHaveBeenCalled();
    });
  });

  describe('loadClientes', () => {
    it('debería cargar clientes exitosamente', () => {
      clientesService.getClientes.and.returnValue(of(mockPaginatedResponse));
      
      component.loadClientes();
      
      expect(clientesService.getClientes).toHaveBeenCalledWith(1, 15, '');
      expect(component.clientes).toEqual(mockClientes);
      expect(component.filteredClientes).toEqual(mockClientes);
      expect(component.totalPages).toBe(1);
      expect(component.totalItems).toBe(2);
    });

    it('debería cargar clientes de una página específica', () => {
      clientesService.getClientes.and.returnValue(of(mockPaginatedResponse));
      
      component.loadClientes(2);
      
      expect(clientesService.getClientes).toHaveBeenCalledWith(2, 15, '');
    });

    it('debería manejar errores al cargar clientes', () => {
      clientesService.getClientes.and.returnValue(throwError(() => new Error('Error de red')));
      spyOn(console, 'error');
      
      component.loadClientes();
      
      expect(console.error).toHaveBeenCalled();
    });
  });

  describe('onNewCliente', () => {
    it('debería abrir el modal en modo crear', () => {
      component.onNewCliente();
      
      expect(component.modalMode).toBe('create');
      expect(component.selectedCliente).toBeNull();
      expect(component.isModalOpen).toBe(true);
    });
  });

  describe('onEditCliente', () => {
    it('debería abrir el modal en modo editar con cliente seleccionado', () => {
      const cliente = mockClientes[0];
      
      component.onEditCliente(cliente);
      
      expect(component.modalMode).toBe('edit');
      expect(component.selectedCliente).toEqual(cliente);
      expect(component.isModalOpen).toBe(true);
    });
  });

  describe('onDeleteCliente', () => {
    it('debería abrir el modal de confirmación con cliente a eliminar', () => {
      const cliente = mockClientes[0];
      
      component.onDeleteCliente(cliente);
      
      expect(component.clienteToDelete).toEqual(cliente);
      expect(component.isConfirmationModalOpen).toBe(true);
    });

    it('debería mostrar error si el cliente está inactivo', () => {
      const clienteInactivo = { ...mockClientes[0], Estado: false };
      
      component.onDeleteCliente(clienteInactivo);
      
      expect(notificationService.showError).toHaveBeenCalledWith('Error', 'No se puede eliminar un cliente inactivo');
      expect(component.isConfirmationModalOpen).toBe(false);
    });
  });

  describe('onSaveCliente', () => {
    it('debería crear un nuevo cliente', () => {
      const nuevoCliente = { ...mockClientes[0], ClienteId: undefined };
      clientesService.createCliente.and.returnValue(of(mockClientes[0]));
      component.modalMode = 'create';
      component.clientes = [];
      
      component.onSaveCliente({ data: nuevoCliente, isPartialUpdate: false, modifiedFields: [] });
      
      expect(clientesService.createCliente).toHaveBeenCalledWith(nuevoCliente);
      expect(component.clientes.length).toBe(1);
      expect(notificationService.showSuccess).toHaveBeenCalledWith('Operación exitosa');
      expect(component.isModalOpen).toBe(false);
    });

    it('debería actualizar un cliente existente con PUT', () => {
      const clienteActualizado = mockClientes[0];
      clientesService.updateCliente.and.returnValue(of(clienteActualizado));
      component.modalMode = 'edit';
      component.selectedCliente = clienteActualizado;
      component.clientes = [...mockClientes];
      
      component.onSaveCliente({ data: clienteActualizado, isPartialUpdate: false, modifiedFields: [] });
      
      expect(clientesService.updateCliente).toHaveBeenCalledWith(1, clienteActualizado);
      expect(notificationService.showSuccess).toHaveBeenCalledWith('Operación exitosa');
      expect(component.isModalOpen).toBe(false);
    });

    it('debería actualizar parcialmente un cliente con PATCH', () => {
      const actualizacion = { Nombre: 'Nuevo Nombre' };
      const clienteActualizado = { ...mockClientes[0], Nombre: 'Nuevo Nombre' };
      clientesService.patchCliente.and.returnValue(of(clienteActualizado));
      component.modalMode = 'edit';
      component.selectedCliente = mockClientes[0];
      component.clientes = [...mockClientes];
      
      component.onSaveCliente({ data: actualizacion, isPartialUpdate: true, modifiedFields: ['Nombre'] });
      
      expect(clientesService.patchCliente).toHaveBeenCalledWith(1, actualizacion);
      expect(notificationService.showSuccess).toHaveBeenCalledWith('Operación exitosa');
      expect(component.isModalOpen).toBe(false);
    });

    it('debería manejar errores al crear', () => {
      const nuevoCliente = { ...mockClientes[0], ClienteId: undefined };
      const errorResponse = { error: { detail: 'Ya existe un cliente con esa identificación' } };
      clientesService.createCliente.and.returnValue(throwError(() => errorResponse));
      component.modalMode = 'create';
      
      component.onSaveCliente({ data: nuevoCliente, isPartialUpdate: false, modifiedFields: [] });
      
      expect(notificationService.showError).toHaveBeenCalledWith('Error', 'Ya existe un cliente con esa identificación');
      expect(component.isModalOpen).toBe(false);
    });

    it('debería manejar errores al actualizar', () => {
      const clienteActualizado = mockClientes[0];
      clientesService.updateCliente.and.returnValue(throwError(() => new Error('Error')));
      component.modalMode = 'edit';
      component.selectedCliente = clienteActualizado;
      
      component.onSaveCliente({ data: clienteActualizado, isPartialUpdate: false, modifiedFields: [] });
      
      expect(notificationService.showError).toHaveBeenCalledWith('Error', 'Error al actualizar cliente');
      expect(component.isModalOpen).toBe(false);
      expect(component.selectedCliente).toBeNull();
    });
  });

  describe('onConfirmDelete', () => {
    it('debería eliminar cliente exitosamente', () => {
      component.clienteToDelete = mockClientes[0];
      clientesService.deleteCliente.and.returnValue(of(undefined));
      spyOn(component, 'loadClientes');
      
      component.onConfirmDelete();
      
      expect(clientesService.deleteCliente).toHaveBeenCalledWith(1);
      expect(notificationService.showSuccess).toHaveBeenCalledWith('Operación exitosa');
      expect(component.isConfirmationModalOpen).toBe(false);
      expect(component.clienteToDelete).toBeNull();
      expect(component.loadClientes).toHaveBeenCalled();
    });

    it('debería manejar errores al eliminar', () => {
      component.clienteToDelete = mockClientes[0];
      const errorResponse = { error: { detail: 'No se puede eliminar porque tiene cuentas asociadas' } };
      clientesService.deleteCliente.and.returnValue(throwError(() => errorResponse));
      
      component.onConfirmDelete();
      
      expect(notificationService.showError).toHaveBeenCalledWith('Error', 'No se puede eliminar porque tiene cuentas asociadas');
      expect(component.isConfirmationModalOpen).toBe(false);
      expect(component.clienteToDelete).toBeNull();
    });
  });

  describe('onPageChange', () => {
    it('debería cambiar de página válida', () => {
      component.totalPages = 5;
      spyOn(component, 'loadClientes');
      
      component.onPageChange(3);
      
      expect(component.loadClientes).toHaveBeenCalledWith(3);
    });

    it('no debería cambiar a página inválida', () => {
      component.totalPages = 5;
      spyOn(component, 'loadClientes');
      
      component.onPageChange(6);
      
      expect(component.loadClientes).not.toHaveBeenCalled();
    });
  });

  describe('onSearch', () => {
    it('debería buscar clientes con query', () => {
      spyOn(component, 'loadClientes');
      
      component.onSearch('Juan');
      
      expect(component.searchQuery).toBe('Juan');
      expect(component.currentPage).toBe(1);
      expect(component.loadClientes).toHaveBeenCalledWith(1);
    });
  });

  describe('onCloseModal', () => {
    it('debería cerrar el modal y limpiar el cliente seleccionado', () => {
      component.isModalOpen = true;
      component.selectedCliente = mockClientes[0];
      
      component.onCloseModal();
      
      expect(component.isModalOpen).toBe(false);
      expect(component.selectedCliente).toBeNull();
    });
  });

  describe('closeNotification', () => {
    it('debería cerrar la notificación', () => {
      component.closeNotification();
      
      expect(notificationService.close).toHaveBeenCalled();
    });
  });

  describe('onCancelDelete', () => {
    it('debería cancelar la eliminación', () => {
      component.isConfirmationModalOpen = true;
      component.clienteToDelete = mockClientes[0];
      
      component.onCancelDelete();
      
      expect(component.isConfirmationModalOpen).toBe(false);
      expect(component.clienteToDelete).toBeNull();
    });
  });
});